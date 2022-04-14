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
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        [Display(Name = "Question Group")]
        public int? QuestionGroupId { get; set; }
        [Required]
        [Display(Name = "Response Type")]
        public int ResponseTypeId { get; set; }

        public bool? AnswerRequired { get; set; } = true;
        [Required]
        [DataType(DataType.Text)]
        public string question { get; set; }
        [ForeignKey("SubjectId")]
        public virtual SurveySubject Subject { get; set; }
        [ForeignKey("QuestionGroupId")]
        [Display(Name = "Question Group")]
        public virtual QuestionGroup QuestionGroup { get; set; }
        [Display(Name = "Response Type")]
        [ForeignKey("ResponseTypeId")] public virtual ResponseType ResponseType { get; set; }
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; }
    }
}
