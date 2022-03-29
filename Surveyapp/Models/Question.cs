using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surveyapp.Models
{
    public class Question
    {
        public Question()
        {
            SurveyResponses = new HashSet<SurveyResponse>();
        }
        public int Id { get; set; }
        [Required]
        public int SubjectId { get; set; }

        public int? QuestionGroupId { get; set; }
        [Required]
        public int ResponseTypeId { get; set; }

        public bool? AnswerRequired { get; set; } = true;
        [Required]
        [DataType(DataType.Text)]
        public string question { get; set; }
        [ForeignKey("SubjectId")]
        public virtual SurveySubject Subject { get; set; }
        [ForeignKey("QuestionGroupId")]
        public virtual QuestionGroup QuestionGroup { get; set; }

        [ForeignKey("ResponseTypeId")] public virtual ResponseType ResponseType { get; set; }
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; }
    }
}
