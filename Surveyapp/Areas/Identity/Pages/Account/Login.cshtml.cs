using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Surveyapp.Models;

namespace Surveyapp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger,
            RoleManager<IdentityRole> userRole, UserManager<ApplicationUser> usermanager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = userRole;
            _userManager = usermanager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            // if (await _roleManager.RoleExistsAsync("Admin") == false)
            // {
            //     IdentityResult Admin = await _roleManager
            //         .CreateAsync(new IdentityRole
            //         {
            //             Name = "Admin",
            //         });
            // }

            // if (await _roleManager.RoleExistsAsync("Employer") == false)
            // {
            //     IdentityResult Surveyor = await _roleManager
            //         .CreateAsync(new IdentityRole
            //         {
            //             Name = "Surveyor",
            //         });
            // }

            // if (await _roleManager.RoleExistsAsync("Freelancer") == false)
            // {

            //     IdentityResult Surveyee = await _roleManager
            //         .CreateAsync(new IdentityRole
            //         {
            //             Name = "Surveyee",
            //         });
            // }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                ApplicationUser userid = _userManager.FindByEmailAsync(Input.Email).Result;
                if (userid != null)
                {
                    if (!_userManager.IsEmailConfirmedAsync(userid).Result)
                    {
                        ModelState.AddModelError(string.Empty, "Email not confirmed!");
                        return Page();
                    }
                }
                if (userid == null)
                {
                    ModelState.AddModelError(string.Empty, "User Email not found!");
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
