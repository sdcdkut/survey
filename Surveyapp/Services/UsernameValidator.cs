using System.Linq;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;
using Surveyapp.Models;

namespace Surveyapp.Services
{
    public class UsernameValidator<TUser> : IUserValidator<TUser>
        where TUser : ApplicationUser
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {                
            if (user.UserName?.Any(x=>x =='*') ==true)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "UsernameContainsInvalidCharacters",
                    Description = "Username cannot contain *"
                }));
            }
            return Task.FromResult(IdentityResult.Success);
        }        
    }
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return httpContext.User.Identity is { IsAuthenticated: true };
        }
    }
}