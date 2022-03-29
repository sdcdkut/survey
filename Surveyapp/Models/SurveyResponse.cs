using System.ComponentModel.DataAnnotations.Schema;

namespace Surveyapp.Models
{
    public class SurveyResponse
    {
        public int Id { get; set; }
        public string RespondantId { get; set; }
        public int QuestionId { get; set; }
        public int Response { get; set; }
        public string ResponseText { get; set; }
        [ForeignKey("RespondantId")]
        public virtual ApplicationUser Respondant { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question question { get; set; }

    }
}
