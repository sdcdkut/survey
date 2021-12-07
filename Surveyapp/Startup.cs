using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surveyapp.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Surveyapp.Services;

namespace Surveyapp
{
    public class Startup
    {
        public IWebHostEnvironment HostingEnvironment { get; private set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            HostingEnvironment = hostEnvironment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            var LockOutOption = new LockoutOptions()
            {
                AllowedForNewUsers = true,
                MaxFailedAccessAttempts = 13,
                DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10)
            };
            services.AddDbContext<SurveyContext>(options =>
                options.UseNpgsql( /*"Server=172.16.32.24;Database=survey;user id=dkutexams;password=D3kut@3xams123#@!??*;MultipleActiveResultSets=true"*/
                    Configuration.GetConnectionString("DefaultConnection")));
            /*services.AddIdentity<ApplicationUser, IdentityRole>(Options =>
            {
                Options.Lockout = LockOutOption;
                Options.SignIn.RequireConfirmedEmail = true;

            });*/
            services.AddIdentity<ApplicationUser, IdentityRole>(Options =>
                {
                    Options.Lockout = LockOutOption;
                    Options.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<SurveyContext>()
                .AddDefaultTokenProviders();
            services.AddSession(option => { option.IdleTimeout = TimeSpan.FromHours(12); }
            );

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddAuthentication()
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, config =>
                {
                    config.Authority = HostingEnvironment.IsDevelopment()? Configuration["openIdAuthority:live"] : Configuration["openIdAuthority:live"];
                    config.ClientId = Configuration["Client:ClientId"];
                    config.ClientSecret = Configuration["Client:ClientSecret"];
                    config.SaveTokens = true;
                    //config.CorrelationCookie.SameSite = SameSiteMode.Lax;
                    //config.NonceCookie.SameSite = SameSiteMode.Lax;
                    config.RequireHttpsMetadata = false;
                    config.ResponseType = "code";
                    config.SignedOutRedirectUri = "/Home";
                    config.RemoteSignOutPath = "/Home/Index";
                    config.SignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    config.SignedOutCallbackPath = "/signout-callback-oid";
                    config.GetClaimsFromUserInfoEndpoint = true;
                    config.Scope.Add("openid");
                    config.Scope.Add("profile");
                    //config.Scope.Add("offline_access");
                    config.ClaimActions.MapJsonKey("Role", "Role");
                    config.UsePkce = true;
                    config.Events = new OpenIdConnectEvents
                    {
                        /*OpenIdEvents.OnTicketReceived(scope)*/
                        OnTicketReceived = async (context) =>
                        {
                            var scope = services.BuildServiceProvider();
                            var userManager = scope?.GetRequiredService<UserManager<ApplicationUser>>();
                            var signInManager = scope?.GetService<SignInManager<ApplicationUser>>();
                            var applicationUsers = userManager?.Users.ToList();
                            var claims = context.Principal?.Claims.ToList();
                            var email = claims?.FirstOrDefault(c => c.Type == "Email")?.Value;
                            if (!string.IsNullOrEmpty(email))
                            {
                                var emailUser = applicationUsers?.FirstOrDefault(c => c?.Email == email);
                                if (emailUser is null)
                                {
                                    var user = new ApplicationUser
                                    {
                                        UserName = email,
                                        Email = email
                                    };

                                    var result = await userManager?.CreateAsync(user, "Password12#")!;

                                    if (result.Succeeded)
                                    {
                                        foreach (var claim in claims!)
                                        {
                                            await userManager!.AddClaimAsync(user, claim);
                                        }
                                        // var signInResult = _signInManager.SignInAsync(user,false);
                                        //
                                        // if (signInResult.IsCompletedSuccessfully)
                                        // {
                                        // If they exist, add claims to the user for:
                                        //    Given (first) name
                                        //    Locale
                                        //    Picture
                                        // if (info.Principal.HasClaim(c => c.Type == ClaimTypes.GivenName))
                                        // {
                                        //     await _userManager.AddClaimAsync(user,
                                        //         info.Principal.FindFirst(ClaimTypes.GivenName));
                                        // }
                                        // Include the access token in the properties
                                        var props = new AuthenticationProperties();
                                        props.StoreTokens(context.Properties?.GetTokens() ?? Array.Empty<AuthenticationToken>());
                                        props.IsPersistent = true;

                                        await signInManager?.SignInAsync(user, props)!;
                                    }
                                }
                                else
                                {
                                    foreach (var claim in claims!)
                                    {
                                        await userManager?.AddClaimAsync(emailUser, claim)!;
                                    }
                                    // Include the access token in the properties
                                    var props = new AuthenticationProperties();
                                    props.StoreTokens(context.Properties?.GetTokens() ?? Array.Empty<AuthenticationToken>());
                                    props.IsPersistent = true;

                                    await signInManager?.SignInAsync(emailUser, props)!;
                                }
                            }
                            return;
                        }
                    };
                })
                .Services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                    options.LoginPath = "/Identity/Account/Login";
                    options.LogoutPath = "/Identity/Account/Login";
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSession();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseAuthorization();
            /*app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });*/

            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                    name: "default",
                    "{controller=Home}/{action=Index}/{id?}");
                routes.MapControllerRoute(
                    name: "second",
                    pattern: "{controller=Home}/{action=Index}/{userId?}");
                routes.MapRazorPages();
            });
        }
    }
}