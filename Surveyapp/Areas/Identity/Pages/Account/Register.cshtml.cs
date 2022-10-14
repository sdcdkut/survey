using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Surveyapp.Models;

namespace Surveyapp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly SurveyContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            SurveyContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _roleManager = roleManager;
        }

        [BindProperty] public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "As DKUT Student")] public bool IsStudent { get; set; } = false;
            [Display(Name = "Course")] public int? CourseId { get; set; }
            [Display(Name = "Department")] public int? DepartmentId { get; set; }
            [Display(Name = "Registration/Staff Number")]
            public string RegStaffNo { get; set; }
            [Display(Name = "As DKUT Staff")]public bool IsStaff { get; set; } = false;
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
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
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            var login = Url.Page("/Account/Login");
            returnUrl ??= login;
            ViewData["Courses"] = _context.Courses.Select(c => new SelectListItem
            {
                Text = $"{c.Code} : {c.Name}",
                Value = c.Id.ToString(),
                Selected = c.Id == Input.CourseId
            });
            ViewData["Departments"] = _context.Departments.Select(c => new SelectListItem
            {
                Text = $"{c.Code} : {c.Name}",
                Value = c.Id.ToString(),
                Selected = c.Id == Input.DepartmentId
            });
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    CourseId = Input.IsStudent? Input?.CourseId: null,
                    DepartmentId = Input.IsStaff?Input?.DepartmentId: null,
                    No = Input?.RegStaffNo,
                    UserType = Input.IsStudent?UserType.Student:UserType.Normal
                };

                if (_context.Users.Any(c => c.Email == Input.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email already exists");
                    return Page();
                }

                if (!string.IsNullOrEmpty(Input.RegStaffNo) && _context.Users.Any(c => c.No == Input.RegStaffNo))
                {
                    ModelState.AddModelError(string.Empty, "Student/Staff already exists");
                    return Page();
                }

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    //add user to  role
                    if (Input.IsStudent)
                    {
                        await _userManager.AddToRoleAsync(user, "Student");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "Surveyor");
                    }
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"<h3>Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<h3>");

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["ConfirmMessage"] = $"{user.UserName} Please Check your email for  email confirmation";
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}