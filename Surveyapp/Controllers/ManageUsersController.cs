using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;
using Surveyapp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Surveyapp.CodeHelpers;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Surveyapp.Controllers
{
    public class ManageUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SurveyContext _context;

        public ManageUsersController(SurveyContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> UserSurvey()
        {
            var surveyContext = _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor);
            return View(await surveyContext.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ReassignSurvey(string id)
        {
            var surveyId = Convert.ToInt32(id);
            var surveyContext = _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor)
                .Where(x => x.Id == surveyId);
            return View(await surveyContext.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ReassignSurvey(Survey survey, string surveyOwner)
        {
            var surveyupdate = _context.Survey.Include(c => c.Surveyors).SingleOrDefault(x => x.Id == survey.Id);
            var surveyors = _context.Surveyors.Where(c => c.SurveyId == survey.Id).ToList();
            //surveyupdate.SurveyerId = survey.SurveyerId;
            try
            {
                foreach (var surveyor in surveyors)
                {
                    surveyor.Owner = surveyor.SurveyorId == surveyOwner;
                    _context.Surveyors.Update(surveyor);
                }

                if (!_context.Surveyors.Any(c => c.SurveyorId == surveyOwner && c.SurveyId == survey.Id))
                {
                    await _context.Surveyors.AddAsync(new Surveyors
                    {
                        Owner = true,
                        Permission = SurveyPermission.AllPermissions,
                        SurveyorId = surveyOwner,
                        ActiveStatus = true,
                        SurveyId = survey.Id
                    });
                }

                //_context.Survey.Update(surveyupdate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SurveyExists(survey.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["FeedbackMessage"] = $"survey edited successfully";
            return RedirectToAction(nameof(UserSurvey));
        }

        private bool SurveyExists(int id)
        {
            return _context.Survey.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleid)
        {
            ViewBag.roleId = roleid;

            var role = await roleManager.FindByIdAsync(roleid);
            ViewBag.roleName = role.Name;
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with {roleid}  cannot be found";
                return View("NotFound");
            }

            var users = new List<UserRole>();
            foreach (var user in userManager.Users)
            {
                var userrole = new UserRole
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userrole.IsSelected = true;
                }
                else
                {
                    userrole.IsSelected = false;
                }

                users.Add(userrole);
            }

            return View(users);
        }


        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRole> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with {roleId}  cannot be found";
                return View("NotFound");
            }

            for (var i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;
                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && (await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                    {
                        continue;
                    }
                    else
                    {
                        return RedirectToAction("EditRole", new { Id = roleId });
                    }
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users.Include(c => c.Course).Include(c => c.Department);
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> LoadUsers([FromForm] Datatable.DtParameters parameters)
        {
            var sd = HttpContext.Request.Query["draw"];
            var recordsTotal = 0;

            try
            {
                var userList = (from user in _context.Users.Include(x => x.Course)
                        .Include(x => x.Department).ToList()
                    select new
                    {
                        user.Id,
                        name = user.UserName,
                        user.Email,
                        user.PhoneNumber,
                        userType = user.UserType.ToString(),
                        user.No,
                        groupName = user.Course?.Name ?? user.Department?.Name
                    }).ToList();
                var searchBy = parameters.Search?.Value;
                var orderCriteria = "semester";
                var orderAscendingDirection = true;
                if (parameters.Order != null)
                {
                    orderCriteria = parameters.Columns[parameters.Order[0].Column].Data ?? "name";
                    orderAscendingDirection = parameters.Order[0].Dir.ToString().ToLower() == "asc";
                }

                if (!string.IsNullOrEmpty(searchBy))
                {
                    userList = userList.Where(m => m.name?.ToUpper().Contains(searchBy.ToUpper()) == true
                                                   || m.Email?.ToUpper().Contains(searchBy.ToUpper()) == true
                                                   || m.PhoneNumber?.ToUpper().Contains(searchBy.ToUpper()) == true
                                                   || m.userType?.ToUpper().Contains(searchBy.ToUpper()) == true
                                                   || m.No?.ToUpper().Contains(searchBy.ToUpper()) == true
                                                   || m.groupName?.ToUpper().Contains(searchBy.ToUpper()) == true).ToList();
                }

                userList = orderAscendingDirection
                    ? userList.AsQueryable().OrderByDynamic(orderCriteria, Datatable.DtOrderDir.Asc).ToList()
                    : userList.AsQueryable().OrderByDynamic(orderCriteria, Datatable.DtOrderDir.Desc).ToList();

                var filteredResultsCount = userList.Count;
                var totalResultsCount = await _context.Users.CountAsync();

                return Json(new Datatable.DtResult<dynamic>
                {
                    Draw = parameters.Draw,
                    RecordsTotal = totalResultsCount,
                    RecordsFiltered = filteredResultsCount,
                    Data = userList
                        .Skip(parameters.Start)
                        .Take(parameters.Length).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public IActionResult GetCustomers()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var pageSize = length != null ? Convert.ToInt32(length) : 0;
                var skip = start != null ? Convert.ToInt32(start) : 0;
                var recordsTotal = 0;
                var customerData = (from user in _context.Users.Include(x => x.Course)
                        .Include(x => x.Department).ToList()
                    select new
                    {
                        user.Id,
                        name = user.UserName,
                        user.Email,
                        user.PhoneNumber,
                        userType = user.UserType.ToString(),
                        user.No,
                        groupName = user.Course?.Name ?? user.Department?.Name
                    }).ToList();
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    customerData = customerData.AsQueryable().OrderBy(sortColumn + " " + sortColumnDirection).ToList();
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerData = customerData.Where(m => m.name?.ToUpper().Contains(searchValue.ToUpper()) == true
                                                           || m.Email?.ToUpper().Contains(searchValue.ToUpper()) == true
                                                           || m.PhoneNumber?.ToUpper().Contains(searchValue.ToUpper()) == true
                                                           || m.userType?.ToUpper().Contains(searchValue.ToUpper()) == true
                                                           || m.No?.ToUpper().Contains(searchValue.ToUpper()) == true
                                                           || m.groupName?.ToUpper().Contains(searchValue.ToUpper()) == true).ToList();
                }

                recordsTotal = customerData.Count;
                var data = customerData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;
            var user = await userManager.FindByIdAsync(userId);
            ViewBag.userName = user.UserName;
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {userId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRolesViewModel>();
            foreach (var role in roleManager.Roles)
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }

                model.Add(userRolesViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {userId} cannot be found";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user from existing roles");
                return View(model);
            }

            foreach (var role in model.Where(x => x.IsSelected).Select(y => y.RoleName))
            {
                result = await userManager.AddToRoleAsync(user, role);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            if (result.Succeeded)
            {
                TempData["FeedbackMessage"] = $"user role edited successfully";
            }

            return RedirectToAction("EditUser", new { Id = userId });
        }

        //[HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["FeedbackMessage"] = $"user deleted successfully";
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListUsers");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    TempData["FeedbackMessage"] = $"role deleted successfully";
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListRoles");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {id} cannot be found";
                return View("NotFound");
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var locked = false;
            if (user.LockoutEnd != null)
            {
                locked = user.LockoutEnd > DateTime.Now;
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user?.UserName,
                PhoneNumber = user?.PhoneNumber,
                Roles = userRoles,
                locked = locked,
                //LockEnd = Convert.ToDateTime(user.LockoutEnd.ToString())
            };
            if (user.LockoutEnd.HasValue) model.LockEnd = user.LockoutEnd.Value.DateTime;
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == user.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == user.DepartmentId
            });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == user.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == user.DepartmentId
            });
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {model.Id} cannot be found";
                return View("NotFound");
            }
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            if (model.LockEnd != null)user.LockoutEnd = model.LockEnd;
            user.CourseId = model.CourseId;
            user.DepartmentId = model.DepartmentId;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["FeedbackMessage"] = $"user edited successfully";
                return RedirectToAction("ListUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                var result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    TempData["FeedbackMessage"] = $"role created successfully";
                    return RedirectToAction("ListRoles", "ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };
            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    TempData["FeedbackMessage"] = $"role edited successfully";
                    return RedirectToAction("ListRoles", "ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        public IActionResult Create()
        {
            throw new NotImplementedException();
        }

        //[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        //[RequestSizeLimit(1074790400)]
        //[DisableRequestSizeLimit]
        public IActionResult ExternalLogin()
        {
            /*if (!HttpContext.User.Identity.IsAuthenticated)
            {
                /*var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "sd"),
                    new Claim(ClaimTypes.Name, "sc"),
                    new Claim("onelogin-access-token", "sd")
                };
                var userIdentity = new ClaimsIdentity(claims, "login");
                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);#1#
                //await HttpContext.SignInAsync(principal);
                //HttpContext.GetOwinContext().Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
            }*/

            return RedirectToAction("Index", "Home");
        }

        [Authorize( /*AuthenticationSchemes = (/*CookieAuthenticationDefaults.AuthenticationScheme + "," +#1# OpenIdConnectDefaults.AuthenticationScheme)*/)]
        public async Task<IActionResult> Signout()
        {
            //return RedirectToAction(nameof(SignIn));
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
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
            //return RedirectToAction(nameof(Index));
            return SignOut(authSignOut, /*CookieAuthenticationDefaults.AuthenticationScheme,*/ OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}