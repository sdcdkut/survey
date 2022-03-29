using System;
using System.Threading.Tasks;
using Surveyapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Surveyapp.Services
{
    public static class Seed
    {
        public static async Task CreateRoles(IServiceProvider serviceProvider, IConfiguration Configuration)
        {
            //adding customs roles
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string[] roleNames = { "Admin", "Surveyor", "Surveyee" ,"Student"};
            IdentityResult roleResult;
            foreach (var roleName in roleNames)
            {
                // creating the roles and seeding them to the database
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            // creating a super user who could maintain the web app
            var poweruser = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
            };
            var user = await UserManager.FindByEmailAsync("admin@gmail.com");
            if(user == null)
            {
                var createPowerUser = await UserManager.CreateAsync(poweruser, "Password12#");
                if (createPowerUser.Succeeded)
                {
                    //assign the new user the "Admin" role 
                    await UserManager.AddToRoleAsync(poweruser, "Admin");
                }
            }

            if (user != null)
            {
                await UserManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
} 