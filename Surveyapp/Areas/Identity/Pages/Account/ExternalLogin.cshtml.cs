using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Surveyapp.Models;

namespace Surveyapp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SurveyContext _context;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            SurveyContext context,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty] public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required] [EmailAddress] public string Email { get; set; }
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
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            var email = info.Principal.FindFirstValue("Email");
            var PFNO = info.Principal.FindFirstValue("PFNO");
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

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

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, "Password12#");
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
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