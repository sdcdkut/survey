using System.ComponentModel.DataAnnotations;

namespace Surveyapp.ViewModel
{
    public class CreateRoleViewModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}
