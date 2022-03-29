using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surveyapp.Models
{
    public class SurveyCategory
    {
        public SurveyCategory()
        {
            SurveySubjects = new HashSet<SurveySubject>();
        }
        public int Id { get; set; }
        [Required]
        public string CategoryName { get; set; }
        [Required]
        public int SurveyId { get; set; }
        [ForeignKey("SurveyId")]
        public virtual Survey Survey { get; set; }
        public virtual ICollection<SurveySubject> SurveySubjects { get; set; }
    }
}
