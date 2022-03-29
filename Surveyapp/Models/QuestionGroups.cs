using System.Collections.Generic;
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
        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public virtual SurveySubject SurveySubject { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}