using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Surveyapp.Models;

namespace Surveyapp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _signInManager = signInManager;
            _logger = logger;
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            /*await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new OpenIdConnectChallengeProperties { RedirectUri = "/" });
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme, new AuthenticationProperties { RedirectUri = "/" });

            await HttpContext.SignOutAsync();
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            /*var authSignOut = new AuthenticationProperties
            {
                RedirectUri = returnUrl != null ? LocalRedirect(returnUrl).Url : Url.Action("Index", "Home")
            };#1#
            var authSignOut = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            };
            //return RedirectToAction(nameof(Index));
            _logger.LogInformation("User logged out");
            var result = await HttpContext.AuthenticateAsync();
            var r1 = await _authenticationSchemeProvider.GetRequestHandlerSchemesAsync();
            var r2 = await _authenticationSchemeProvider.GetDefaultAuthenticateSchemeAsync();
            var r3 = await _authenticationSchemeProvider.GetAllSchemesAsync();
            var authenticationScheme = result.Ticket?.AuthenticationScheme;
            if (authenticationScheme is OpenIdConnectDefaults.AuthenticationScheme)
            //if (r3.Any(c=>c.Name == OpenIdConnectDefaults.AuthenticationScheme))
            {
                return SignOut(authSignOut, OpenIdConnectDefaults.AuthenticationScheme);
            }

            var logoutId = Request.Query["logoutId"].ToString();
            return SignOut(authSignOut, /*CookieAuthenticationDefaults.AuthenticationScheme,#1# OpenIdConnectDefaults.AuthenticationScheme, IdentityConstants.ExternalScheme);

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return Page();
            }*/
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new OpenIdConnectChallengeProperties { RedirectUri = "/" });
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new OpenIdConnectChallengeProperties { RedirectUri = "/" });
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme, new AuthenticationProperties { RedirectUri = "/" });

            await HttpContext.SignOutAsync();
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            var authSignOut = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            };
            return SignOut(authSignOut, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}