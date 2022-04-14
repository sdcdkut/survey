using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surveyapp.Models
{
    public class QuestionGroup
    {
        public QuestionGroup()
        {
            Questions = new HashSet<Question>();
        }
        public int Id { get; set; }
        public string Name { get; set; }

        [Display(Name = "Subject")]
        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        [Display(Name = "Subject")]
        public virtual SurveySubject SurveySubject { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}