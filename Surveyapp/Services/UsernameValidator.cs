using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Surveyapp.Models;

namespace Surveyapp.Services
{
    public class UsernameValidator<TUser> : IUserValidator<TUser>
        where TUser : ApplicationUser
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {                
            if (user.UserName.Any(x=>x =='*'))
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
}