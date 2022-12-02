using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using Community.Microsoft.Extensions.Caching.PostgreSql;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surveyapp.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using Surveyapp.Services;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

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
            var lockOutOption = new LockoutOptions
            {
                AllowedForNewUsers = true,
                MaxFailedAccessAttempts = 13,
                DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10)
            };
            var connectionString = Configuration.GetConnectionString("LiveConnection"); //LiveConnection;DefaultConnection
            services.AddDbContextPool<SurveyContext>(options => options.UseNpgsql(connectionString));
            /*services.AddIdentity<ApplicationUser, IdentityRole>(Options =>
            {
                Options.Lockout = LockOutOption;
                Options.SignIn.RequireConfirmedEmail = true;

            });*/
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Lockout = lockOutOption;
                    options.SignIn.RequireConfirmedEmail = true;
                    options.User.AllowedUserNameCharacters = string.Empty
                        /*"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+'"*/;
                })
                .AddEntityFrameworkStores<SurveyContext>()
                .AddDefaultTokenProviders()
                .AddUserValidator<UsernameValidator<ApplicationUser>>();
            services.AddSession(option => { option.IdleTimeout = TimeSpan.FromHours(12); }
            );

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddMemoryCache();
            services.AddDistributedPostgreSqlCache(setup =>
            {
                setup.ConnectionString = connectionString;
                setup.SchemaName = Configuration["PgCache:SchemaName"];
                setup.TableName = Configuration["PgCache:TableName"];
                setup.CreateInfrastructure = !string.IsNullOrWhiteSpace(Configuration["PgCache:CreateInfrastructure"]);
                setup.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(5);
            });
            //services.AddDataProtection().PersistKeysToDbContext<Infrastructure.DataProtectionDbContext>();
            // OR .PersistKeysToStackExchangeRedis
            //var configManager = new ConfigurationManager<OpenIdConnectConfiguration>("https://accounts.google.com/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            /*var configManager = new OpenIdConnectConfiguration
            {
                AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
                EndSessionEndpoint = "https://accounts.google.com/o/oauth2/revoke",
                Issuer = "https://accounts.google.com",
                JwksUri = "https://www.googleapis.com/oauth2/v3/certs",
                TokenEndpoint = "https://oauth2.googleapis.com/token",
                UserInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo",
                ResponseTypesSupported = { "code", "token", "id_token", "code token", "code id_token", "token id_token", "code token id_token", "none" },
                SubjectTypesSupported = { "public" },
                IdTokenSigningAlgValuesSupported = { "RS256" },
                ScopesSupported = { "openid", "email", "profile" },
                TokenEndpointAuthMethodsSupported = { "client_secret_post", "client_secret_basic" },
                ClaimsSupported = { "aud", "email", "email_verified", "exp", "family_name", "given_name", "iat", "iss", "locale", "name", "picture", "sub" },
                RequestUriParameterSupported = false,
                GrantTypesSupported =
                {
                    "authorization_code",
                    "refresh_token",
                    "urn:ietf:params:oauth:grant-type:device_code",
                    "urn:ietf:params:oauth:grant-type:jwt-bearer"
                },
            };*/

            services.AddAuthentication()
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, "University Account", config =>
                {
                    config.Authority = HostingEnvironment.IsDevelopment() ? Configuration["openIdAuthority:live"] : Configuration["openIdAuthority:live"];
                    config.ClientId = Configuration["Client:ClientId"];
                    config.ClientSecret = Configuration["Client:ClientSecret"];
                    config.SaveTokens = true;
                    config.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                    config.NonceCookie.SameSite = SameSiteMode.Unspecified;
                    config.RequireHttpsMetadata = false;
                    config.ResponseType = "code";
                    config.SignedOutRedirectUri = "/Home";
                    config.RemoteSignOutPath = "/Home/Index";
                    //config.SignOutScheme = /*OpenIdConnectDefaults.AuthenticationScheme*/IdentityConstants.ExternalScheme;
                    config.SignedOutCallbackPath = "/signout-callback-oid";
                    config.GetClaimsFromUserInfoEndpoint = true;
                    config.Scope.Add("openid");
                    config.Scope.Add("profile");
                    //config.Scope.Add("offline_access");
                    config.ClaimActions.MapJsonKey("Role", "Role");
                    config.UsePkce = true;
                    config.SignInScheme = IdentityConstants.ExternalScheme;
                    /*config.Events = new OpenIdConnectEvents
                    {
                        /*OpenIdEvents.OnTicketReceived(scope)#1#
                        OnTicketReceived = async (context) =>
                        {
                            /*var scope = services.BuildServiceProvider();
                            var userManager = scope?.GetRequiredService<UserManager<ApplicationUser>>();
                            var signInManager = scope?.GetService<SignInManager<ApplicationUser>>();#1#
                            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                            var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

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
                                        /*foreach (var claim in claims!)
                                        {
                                            await userManager!.AddClaimAsync(user, claim);
                                        }#1#
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
                                        //var props = new AuthenticationProperties();
                                        //props.StoreTokens(context.Properties?.GetTokens() ?? Array.Empty<AuthenticationToken>());
                                        //props.IsPersistent = true;
                                        
                                        await signInManager?.SignInAsync(user, false)!;
                                    }
                                }
                                else
                                {
                                    /*foreach (var claim in claims!)
                                    {
                                        await userManager?.AddClaimAsync(emailUser, claim)!;
                                    }#1#
                                    // Include the access token in the properties
                                    //var props = new AuthenticationProperties();
                                    //props.StoreTokens(context.Properties?.GetTokens() ?? Array.Empty<AuthenticationToken>());
                                    //props.IsPersistent = true;

                                    await signInManager?.SignInAsync(emailUser, false)!;
                                }
                                
                            }
                            return;
                        }
                    };*/
                })
                /*.AddGoogle("University Gmail Account", "University Gmail Account", options =>
                {
                    var clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    clientHandler.SslProtocols = SslProtocols.Tls12;
                    options.BackchannelHttpHandler = clientHandler;
                    options.ClientId = Configuration["Google:ClientId"];
                    options.ClientSecret = Configuration["Google:ClientSecret"];
                    options.SaveTokens = true;
                })*/
                .AddOpenIdConnect("University Gmail Account", "University Gmail Account", options =>
                {
                    var clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    clientHandler.SslProtocols = SslProtocols.Tls12;
                    options.BackchannelHttpHandler = clientHandler;
                    options.ClientId = Configuration["Google:ClientId"];
                    options.ClientSecret = Configuration["Google:ClientSecret"];
                    options.SaveTokens = true;
                    options.Authority = "https://accounts.google.com";
                    options.CallbackPath = "/signin-oidc-google";
                    //options.Configuration = configManager;
                    options.MetadataAddress = "https://accounts.google.com/.well-known/openid-configuration";
                    /*options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        ValidateIssuerSigningKey = true
                    };*/
                    /*options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                        {
                            /*var _clientHandler = new SocketsHttpHandler();
                            _clientHandler.SslOptions.RemoteCertificateValidationCallback = (message, cert, chain, errors) => true;#1#
                            //var certificate = new X509Certificate2(filePath, password, X509KeyStorageFlags.PersistKeySet);
                            var _clientHandler = new HttpClientHandler();
                            _clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                            _clientHandler.SslProtocols = SslProtocols.Tls12;
                            var client = new HttpClient(_clientHandler);
                            var response = client.GetAsync("https://www.googleapis.com/oauth2/v3/certs").Result;
                            var responseString = response.Content.ReadAsStringAsync().Result;
                            var keys = JsonConvert.DeserializeObject<List<JsonWebKey>>(responseString);
                            return keys;
                            /*?.Select(key => new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(key.X5c[0]))))#1#
                            //return new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(token)));
                        },
                        ValidIssuers = new List<string>{"https://accounts.google.com"},
                    };*/
                    /*options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidAudience = options.ClientId,

                        ValidateIssuer = true,
                        ValidIssuers = new[] { options.Authority },

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = configManager.SigningKeys,

                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        RequireSignedTokens = true,
                    };*/
                    //var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>("https://accounts.google.com/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                    //options.ResponseType = "code";
                    //options.RequireHttpsMetadata = true;
                    options.SaveTokens = true;
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("openid");
                    options.UsePkce = true;
                    //options.SignInScheme= "Google";
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                    options.ClaimActions.MapJsonKey("urn:google:profile", "link");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                })
                .Services.ConfigureApplicationCookie(options => { CookieSettings(services, options); });
            services.AddHttpClient("Workman", httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://workman.dkut.ac.ke/");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "HttpRequestsSample");
            });
            services.AddHttpClient("googleWellKnown", httpClient => { httpClient.BaseAddress = new Uri("https://accounts.google.com/"); });
            services.AddHostedService<QueuedHostedService>();
            services.AddHangfireServer();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(connectionString /*, new PostgreSqlStorageOptions{QueuePollInterval = TimeSpan.Zero,}*/));
            //services.Configure<CookiePolicyOptions>(options => { options.Secure = CookieSecurePolicy.Always; });
            services.AddScoped<ISyncStudent, SyncStudent>();
            services.Configure<KestrelServerOptions>(opt =>
            {
                opt.Limits.MaxRequestBodySize = int.MaxValue;
                opt.Limits.MaxRequestHeaderCount = int.MaxValue;
                opt.Limits.MaxRequestHeadersTotalSize = int.MaxValue;
                opt.Limits.MaxRequestBufferSize = int.MaxValue;
                opt.Limits.MaxRequestLineSize = int.MaxValue;
                opt.Limits.MaxRequestHeaderCount = int.MaxValue;
                //opt.ConfigureEndpointDefaults(endpoint =>
                //{
                //    endpoint.
                //});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });*/
            app.UseDeveloperExceptionPage();
            app.UseSession();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
                //app.UseDatabaseErrorPage();
            }
            else
            {
                //app.UseDeveloperExceptionPage();
                app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            //app.UseCookiePolicy();

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
            /*app.UseCookiePolicy(new CookiePolicyOptions()
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
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
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            });
            RecurringJob.AddOrUpdate<ISyncStudent>("update-Student", x => x.SyncStudentTask(), Cron.Daily(3, 0));
        }

        private static void CookieSettings(IServiceCollection services, CookieAuthenticationOptions options)
        {
            var scope = services.BuildServiceProvider();
            //var memoryCache = scope?.GetRequiredService<IMemoryCache>();
            var memoryCache = scope?.GetRequiredService<IDistributedCache>();
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(12);
            options.LoginPath = "/Identity/Account/Login";
            options.LogoutPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.SlidingExpiration = true;
            options.SessionStore = new InMemoryTicketStore(memoryCache);
        }
    }
}