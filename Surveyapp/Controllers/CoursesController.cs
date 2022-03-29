using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Surveyapp.Models;
using Surveyapp.Services;

namespace Surveyapp.Controllers
{
    public class CoursesController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CoursesController(IWebHostEnvironment hostingEnvironment, SurveyContext context, UserManager<ApplicationUser> userManager
            , IHttpClientFactory httpClientFactory, IBackgroundTaskQueue queue, IServiceScopeFactory serviceScopeFactory)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var surveyContext = _context.Courses.Include(c => c.Department);
            return View(await surveyContext.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Name,DepartmentId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", course.DepartmentId);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", course.DepartmentId);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,DepartmentId")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", course.DepartmentId);
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null) _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ImportCourses()
        {
            _queue.QueueBackgroundWorkItem(async token =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var httpClient = _httpClientFactory.CreateClient("Workman");
                var surveyContext = scopedServices.GetRequiredService<SurveyContext>();
                var httpResponseMessage = await httpClient.GetAsync(
                    "api/Units/Courses", token);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var courses =
                        await httpResponseMessage.Content.ReadFromJsonAsync<List<Surveyapp.Models.ViewModels.Course>>(cancellationToken: token);
                    if (courses != null && courses.Any())
                    {
                        var departments = courses.Select(c => c.Department).Distinct().ToList();
                        var schools = courses.Select(c => c.Department.SchoolOrInstitution).Distinct().ToList();
                        var campus = surveyContext.Campus.FirstOrDefault(c => c.Name == "Dedan Kimathi University of Technology");
                        if (campus is null)
                        {
                            await surveyContext.Campus.AddAsync(new Campus
                            {
                                Name = "Dedan Kimathi University of Technology"
                            }, token);
                            await surveyContext.SaveChangesAsync(token);
                        }

                        foreach (var newSchoolOrInstitution in from schoolOrInstitution in schools let schoolCheck = surveyContext.SchoolOrInstitutions.FirstOrDefault(c => c.Name == schoolOrInstitution.Name) let newSchoolOrInstitution = new SchoolOrInstitution
                        {
                            Name = schoolOrInstitution.Name,
                            CampusId = surveyContext.Campus.FirstOrDefault(c => c.Name == "Dedan Kimathi University of Technology")!.Id
                        } where schoolCheck is null select newSchoolOrInstitution)
                        {
                            surveyContext.SchoolOrInstitutions.Add(newSchoolOrInstitution);
                            await surveyContext.SaveChangesAsync(token);
                        }

                        foreach (var department in departments)
                        {
                            var departmentCheck = surveyContext.Departments.ToList().FirstOrDefault(c => c.Code == department?.Code && c.Name == department?.Name);
                            var schoolOrInstitution = surveyContext.SchoolOrInstitutions.ToList().FirstOrDefault(c => c.Name == department?.SchoolOrInstitution?.Name);
                            if(schoolOrInstitution is null) continue;
                            var newDepartment = new Department
                            {
                                Code = department?.Code,
                                Name = department?.Name,
                                SchoolOrInstitutionId = schoolOrInstitution.Id
                            };
                            if (departmentCheck is not null) continue;
                            surveyContext.Departments.Add(newDepartment);
                            await surveyContext.SaveChangesAsync(token);
                        }

                        foreach (var course in courses)
                        {
                            var courseCheck = surveyContext.Courses.ToList().FirstOrDefault(c => c.Code == course?.Code && c.Name == course?.CouserName);
                            var department = surveyContext.Departments.ToList().FirstOrDefault(c => c.Code == course?.Department?.Code && c.Name == course?.Department?.Name);
                           if(department is null) continue;
                            var newCourse = new Course
                            {
                                Code = course?.Code,
                                Name = course?.CouserName,
                                DepartmentId = department.Id
                            };
                            if (courseCheck is not null) continue;
                            surveyContext.Courses.Add(newCourse);
                            await surveyContext.SaveChangesAsync(token);
                        }

                        /*var schoolList = surveyContext.SchoolOrInstitutions.ToList();
                        foreach (var schoolOrInstitution in schools)
                        {
                            var schoolCheck = schoolList.FirstOrDefault(c => c.Name == schoolOrInstitution.Name);
                            var newSchoolOrInstitution = new SchoolOrInstitution
                            {
                                Name = schoolOrInstitution.Name,
                                CampusId = surveyContext.Campus.FirstOrDefault(c => c.Name == "Dedan Kimathi University of Technology")!.Id
                            };
                            if (schoolCheck is null)
                            {
                                surveyContext.SchoolOrInstitutions.Add(newSchoolOrInstitution);
                                await surveyContext.SaveChangesAsync(token);
                            }

                            var schoolDepartments = schoolOrInstitution.Departments.Where(c => c != null).ToList();
                            foreach (var department in schoolDepartments)
                            {
                                var departmentCheck = surveyContext.Departments.ToList().FirstOrDefault(c => c.Code == department?.Code && c.Name == department?.Name);
                                var newDepartment = new Department
                                {
                                    Code = department?.Code,
                                    Name = department?.Name,
                                    SchoolOrInstitutionId = schoolCheck?.Id ?? newSchoolOrInstitution.Id
                                };
                                if (departmentCheck is null)
                                {
                                    surveyContext.Departments.Add(newDepartment);
                                    await surveyContext.SaveChangesAsync(token);
                                }

                                var schoolCourses = schoolDepartments.SelectMany(c => c.Courses).Where(c => c != null).ToList();
                                foreach (var course in schoolCourses)
                                {
                                    var courseCheck = surveyContext.Courses.ToList().FirstOrDefault(c => c.Code == course?.Code && c.Name == course?.CouserName);
                                    var newCourse = new Course
                                    {
                                        Code = course?.Code,
                                        Name = course?.CouserName,
                                        DepartmentId = departmentCheck?.Id ?? newDepartment.Id
                                    };
                                    if (courseCheck is not null) continue;
                                    surveyContext.Courses.Add(newCourse);
                                    await surveyContext.SaveChangesAsync(token);
                                }
                            }
                        }*/
                    }
                    
                }
                else
                {
                    Console.WriteLine("{0} ({1})",
                        (int)httpResponseMessage.StatusCode,
                        httpResponseMessage.ReasonPhrase);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            });
            TempData["FeedbackMessage"] = $"Update of courses In progress..";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ImportStudents()
        {
            _queue.QueueBackgroundWorkItem(async token =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var httpClient = _httpClientFactory.CreateClient("Workman");
                var surveyContext = scopedServices.GetRequiredService<SurveyContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                var httpResponseMessage = await httpClient.GetAsync(
                    "api/Units/Students", token);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var students =
                        await httpResponseMessage.Content.ReadFromJsonAsync<List<Models.ViewModels.Student>>(cancellationToken: token);
                    if (students != null && students.Any())
                    {
                        var appUsers = surveyContext.Users.ToList();
                        var courseList = surveyContext.Courses.ToList();
                        foreach (var student in students)
                        {
                            var course = courseList.FirstOrDefault(c=>c.Code == student?.Course?.Code && c.Name == student?.Course?.CouserName);
                            if (course is null) continue;
                            var user = appUsers.FirstOrDefault(u => u.No == student?.StudentReg);
                            if (user is not null)
                            {
                                user.No = student.StudentReg;
                                user.CourseId = course.Id;
                                user.Email = student.Email;
                                user.UserType = UserType.Student;
                                user.UserName = student?.FirstName?.Trim() + "_" + student?.MiddleName?.Trim() + "_" + student?.LastName?.Trim();
                                user.PhoneNumber = student?.PhoneNo;
                                surveyContext.Users.Update(user);
                                await surveyContext.SaveChangesAsync(token);
                            };
                            var newUser = new ApplicationUser
                            {
                                CourseId = course.Id,
                                Email = student.Email,
                                UserType = UserType.Student,
                                UserName = student?.FirstName?.Trim() + "_" + student?.MiddleName?.Trim() + "_" + student?.LastName?.Trim(),
                                PhoneNumber = student?.PhoneNo,
                                No = student.StudentReg,
                                EmailConfirmed = true,
                            };
                            var appUser= await userManager.CreateAsync(newUser, "Password@123");
                            if (appUser.Succeeded)
                            {
                                await userManager.AddToRoleAsync(newUser, "Student");
                            }
                        }
                    }
                    
                }
                else
                {
                    Console.WriteLine("{0} ({1})",
                        (int)httpResponseMessage.StatusCode,
                        httpResponseMessage.ReasonPhrase);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            });
            TempData["FeedbackMessage"] = $"Update of Students as user In progress..";
            return RedirectToAction(nameof(Index));
        }
    }
}