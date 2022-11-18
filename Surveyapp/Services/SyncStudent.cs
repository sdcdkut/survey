using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Surveyapp.Models;

namespace Surveyapp.Services
{
    public class SyncStudent : ISyncStudent
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public SyncStudent(SurveyContext context, UserManager<ApplicationUser> userManager
            , IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SyncStudentTask()
        {
            //using var scope = _serviceScopeFactory.CreateScope();
            //var scopedServices = scope.ServiceProvider;
            var httpClient = _httpClientFactory.CreateClient("Workman");
            var httpResponseMessage = await httpClient.GetAsync("api/Units/Students");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var students = await httpResponseMessage.Content.ReadFromJsonAsync<List<Models.ViewModels.Student>>();
                if (students != null && students.Any())
                {
                    var appUsers = /* surveyContext.Users.ToList()*/
                        (await _userManager.GetUsersInRoleAsync("Student")).ToList();
                    var courseList = _context.Courses.ToList();
                    foreach (var student in students)
                    {
                        var course = courseList.FirstOrDefault(c => c.Code.ToUpper() == student?.Course?.Code.ToUpper() && c.Name?.ToUpper() == student?.Course?.CouserName?.ToUpper());
                        if (course is null) continue;
                        var user = appUsers.FirstOrDefault(u => u.No == student?.StudentReg);
                        if (user is not null)
                        {
                            user.No = student.StudentReg;
                            user.CourseId = course.Id;
                            user.Email = student.Email;
                            user.UserType = UserType.Student;
                            user.UserName = /*student.FirstName?.Trim() + "_" + student.MiddleName?.Trim() + "_" + student.LastName?.Trim()*/ student.StudentReg;
                            user.PhoneNumber = student.PhoneNo;
                            _context.Users.Update(user);
                            continue;
                        }

                        var newUser = new ApplicationUser
                        {
                            CourseId = course.Id,
                            Email = student.Email,
                            UserType = UserType.Student,
                            //UserName = /*Regex.Replace(*/(student?.FirstName?.Trim() + "_" + student?.MiddleName?.Trim() + "_" + student?.LastName?.Trim()) /*, @"[^0-9a-zA-Z\._]", string.Empty)*/,
                            UserName = student.StudentReg,
                            PhoneNumber = student?.PhoneNo,
                            No = student.StudentReg,
                            EmailConfirmed = true,
                        };
                        //userManager.UserValidators = new List<IUserValidator<ApplicationUser>> { new UserValidator<ApplicationUser>() };
                        var appUser = await _userManager.CreateAsync(newUser, "Password@123");
                        if (appUser.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(newUser, "Student");
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                Console.WriteLine("{0} ({1})",
                    (int)httpResponseMessage.StatusCode,
                    httpResponseMessage.ReasonPhrase);
            }
        }
    }

    public interface ISyncStudent
    {
        Task SyncStudentTask();
    }
}