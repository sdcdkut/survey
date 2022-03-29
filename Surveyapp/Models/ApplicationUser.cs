using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surveyapp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Surveys = new HashSet<Survey>();
            SurveyResponses = new HashSet<SurveyResponse>();
        }

        public UserType? UserType { get; set; }
        public string No { get; set; }
        public int? CourseId { get; set; }
        public int? DepartmentId { get; set; }
        public virtual ICollection<Survey> Surveys { get; set; }
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; }
        [ForeignKey("CourseId")] public virtual Course Course { get; set; }
        [ForeignKey("DepartmentId")] public virtual Department Department { get; set; }
    }

    public enum UserType
    {
        Normal,
        Student
    }
}