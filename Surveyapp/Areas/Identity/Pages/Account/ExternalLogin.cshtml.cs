using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Surveyapp.Models;
using Surveyapp.Models.ViewModels;

namespace Surveyapp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SurveyContext _context;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            SurveyContext context,
            ILogger<ExternalLoginModel> logger,
            IHttpClientFactory httpClientFactory)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty] public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required] [EmailAddress] public string Email { get; set; }

            [Display(Name = "Reg/Staff No_")]
            [Required]
            public string RegStaffNo { get; set; }

            [Display(Name = "As Dkut Student")] public bool IsStudent { get; set; } = false;
            [Display(Name = "As Dkut Staff")]public bool IsStaff { get; set; } = false;
            [Display(Name = "Course")] public int? CourseId { get; set; }
            [Display(Name = "Department")] public int? DepartmentId { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            ViewData["Courses"] = _context.Courses.Select(c => new SelectListItem
            {
                Text = $"{c.Code} : {c.Name}",
                Value = c.Id.ToString()
            });
            ViewData["Departments"] = _context.Departments.Select(c => new SelectListItem
            {
                Text = $"{c.Code} : {c.Name}",
                Value = c.Id.ToString()
            });
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info.LoginProvider != "University Gmail Account")
            {
                var email = info.Principal.FindFirstValue("Email");
                var PFNO = info.Principal.FindFirstValue("PFNO");

                // Sign in the user with this external login provider if the user already has a login.
                //var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

                var user1 = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if ( /*result.Succeeded*/ user1 != null)
                {
                    if (await _userManager.IsLockedOutAsync(user1))
                    {
                        return RedirectToPage("./Lockout");
                    }

                    user1.UserType = UserType.Normal;
                    user1.EmailConfirmed = true;
                    user1.No = PFNO;
                    await _context.SaveChangesAsync();
                    var props3 = new AuthenticationProperties();
                    props3.StoreTokens(info.AuthenticationTokens ?? Array.Empty<AuthenticationToken>());
                    props3.IsPersistent = true;
                    //var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user1);
                    /*if (info.AuthenticationTokens != null)
                    {
                        /*foreach (var infoAuthenticationToken in info.AuthenticationTokens)
                        {
                            ((ClaimsIdentity)claimsPrincipal.Identity)?.AddClaim(new Claim(infoAuthenticationToken.Name, infoAuthenticationToken.Value));
                        }#1#
    
                        ((ClaimsIdentity)claimsPrincipal.Identity)?.AddClaim(new Claim("accessToken",
                            info.AuthenticationTokens?.Single(t => t.Name == "access_token").Value));
                    }*/

                    foreach (var claim in info.Principal?.Claims.ToList()!)
                    {
                        await _userManager.AddClaimAsync(user1, claim);
                    }

                    //await HttpContext.SignInAsync("Identity.Application", claimsPrincipal);
                    //await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal, props3);
                    //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    await _signInManager.SignInAsync(user1, props3);
                    /*await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme,
                        claimsPrincipal, props3);*/
                    _logger.LogInformation("{Name} logged in with {LoginProvider} provider", info.Principal.Identity?.Name, info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }
                /*if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }*/
                else
                {
                    // If the user does not have an account, then ask the user to create an account.
                    ReturnUrl = returnUrl;
                    LoginProvider = info.LoginProvider;
                    if (info.Principal.HasClaim(c => c.Type == "Email"))
                    {
                        /*Input = new InputModel
                        {
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };*/
                        var claims = info.Principal?.Claims.ToList();

                        ;
                        var excitingUser = await _userManager.FindByEmailAsync(email);
                        if (excitingUser is null)
                        {
                            var user = new ApplicationUser
                            {
                                UserName = email,
                                Email = email,
                                UserType = UserType.Normal,
                                EmailConfirmed = true,
                                No = PFNO
                            };
                            var newUser = await _userManager?.CreateAsync(user, "Password12#")!;
                            if (!newUser.Succeeded) return Page();
                            foreach (var claim in claims!)
                            {
                                await _userManager!.AddClaimAsync(user, claim);
                            }

                            var props = new AuthenticationProperties();
                            props.StoreTokens(info.AuthenticationTokens ?? Array.Empty<AuthenticationToken>());
                            props.IsPersistent = true;
                            await _userManager.AddLoginAsync(user, info);
                            await _signInManager?.SignInAsync(user, props);
                            return LocalRedirect(returnUrl);
                        }

                        excitingUser.UserType = UserType.Normal;
                        excitingUser.EmailConfirmed = true;
                        excitingUser.No = PFNO;
                        await _context.SaveChangesAsync();
                        foreach (var claim in claims)
                        {
                            await _userManager!.AddClaimAsync(excitingUser, claim);
                        }

                        var props1 = new AuthenticationProperties();
                        props1.StoreTokens(info.AuthenticationTokens ?? Array.Empty<AuthenticationToken>());
                        props1.IsPersistent = true;
                        await _userManager.AddLoginAsync(excitingUser, info);
                        await _signInManager?.SignInAsync(excitingUser, props1);
                        return LocalRedirect(returnUrl);
                    }

                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                    return Page();
                }
            }

            //var gmail = info.Principal.FindFirstValue("Email");
            var gmail = info.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            if (string.IsNullOrEmpty(gmail))
            {
                ModelState.AddModelError(string.Empty, "Email claim not received from: " + info.LoginProvider);
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl, ErrorMessage = "Email claim not received from: " + info.LoginProvider });
            }

            if (!gmail.Split("@")[1].Contains("dkut.ac.ke"))
            {
                ModelState.AddModelError(string.Empty, "Email is not from DKUT");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl = ErrorMessage = "Email is not from DKUT" });
            }

            var gmailUser = await _userManager.FindByEmailAsync(gmail);
            if (gmailUser != null)
            {
                if (await _userManager.IsLockedOutAsync(gmailUser))
                {
                    return RedirectToPage("./Lockout");
                }

                gmailUser.UserType = UserType.Normal;
                gmailUser.EmailConfirmed = true;
                await _context.SaveChangesAsync();
                var props3 = new AuthenticationProperties();
                props3.StoreTokens(info.AuthenticationTokens ?? Array.Empty<AuthenticationToken>());
                props3.IsPersistent = true;
                foreach (var claim in info.Principal?.Claims.ToList()!)
                {
                    await _userManager.AddClaimAsync(gmailUser, claim);
                }

                await _signInManager.SignInAsync(gmailUser, props3);
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider", info.Principal.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            // If the user does not have an account, then ask the user to create an account.
            ReturnUrl = returnUrl;
            LoginProvider = info.LoginProvider;
            var httpClient = _httpClientFactory.CreateClient("Workman");
            var httpResponseMessage = await httpClient.GetAsync("api/Units/Students");
            var students = await httpResponseMessage.Content.ReadFromJsonAsync<List<Student>>();
            var student = students?.FirstOrDefault(s => s.Email == gmail);
            if (info.Principal.HasClaim(c => c.Type == "Email") && student != null)
            {
                /*Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };*/
                var claims = info.Principal?.Claims.ToList();
                var excitingUser = await _userManager.FindByEmailAsync(gmail);
                if (excitingUser is null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = student.StudentReg,
                        Email = gmail,
                        UserType = UserType.Normal,
                        EmailConfirmed = true,
                        No = student.StudentReg
                    };
                    var newUser = await _userManager?.CreateAsync(user, "Password12#")!;
                    if (!newUser.Succeeded) return Page();
                    foreach (var claim in claims!)
                    {
                        await _userManager!.AddClaimAsync(user, claim);
                    }

                    var props = new AuthenticationProperties();
                    props.StoreTokens(info.AuthenticationTokens ?? Array.Empty<AuthenticationToken>());
                    props.IsPersistent = true;
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager?.SignInAsync(user, props);
                    return LocalRedirect(returnUrl);
                }

                excitingUser.UserType = UserType.Normal;
                excitingUser.EmailConfirmed = true;
                excitingUser.No = student.StudentReg;
                await _context.SaveChangesAsync();
                foreach (var claim in claims)
                {
                    await _userManager!.AddClaimAsync(excitingUser, claim);
                }

                var props1 = new AuthenticationProperties();
                props1.StoreTokens(info.AuthenticationTokens ?? Array.Empty<AuthenticationToken>());
                props1.IsPersistent = true;
                await _userManager.AddLoginAsync(excitingUser, info);
                await _signInManager?.SignInAsync(excitingUser, props1);
                return LocalRedirect(returnUrl);
            }

            Input = new InputModel
            {
                Email = info.Principal.FindFirstValue(ClaimTypes.Email)
            };
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            ViewData["Courses"] = _context.Courses.Select(c => new SelectListItem
            {
                Text = $"{c.Code} : {c.Name}",
                Value = c.Id.ToString()
            });
            ViewData["Departments"] = _context.Departments.Select(c => new SelectListItem
            {
                Text = $"{c.Code} : {c.Name}",
                Value = c.Id.ToString()
            });
            returnUrl ??= Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var userType = Input.IsStudent?UserType.Student:UserType.Normal;
                var user = new ApplicationUser { UserName = Input.RegStaffNo, Email = Input.Email, No = Input.RegStaffNo, EmailConfirmed = true, UserType = userType };
                var result = await _userManager.CreateAsync(user, "Password@123");
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        //add user to  role
                        if (Input.IsStudent)
                        {
                            await _userManager.AddToRoleAsync(user, "Student");
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(user, "Surveyor");
                        }

                        if (Input.CourseId != null && Input.IsStudent)user.CourseId = Input.CourseId;
                        if (Input.DepartmentId != null && Input.IsStaff)user.DepartmentId = Input.DepartmentId;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("User created an account using {Name} provider", info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            LoginProvider = info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}