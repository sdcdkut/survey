using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Surveyapp.Models
{
    public class ResponseType
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string ResponseName { get; set; }
        [Required]
        public int SubjectId { get; set; }
        // [Required]
        public Dictionary<string, string> ResponseDictionary
        {
            get; set;
        }
        [ForeignKey("SubjectId")]
        public virtual SurveySubject Subject { get; set; }
    }
}
