using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surveyapp.Models
{
    public class ApplicationUser: IdentityUser
    {
        public ApplicationUser()
        {
            Surveys = new HashSet<Survey>();
            SurveyResponses = new HashSet<SurveyResponse>();
        }
        public virtual ICollection<Survey> Surveys { get; set; }
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; }
    }
}
