using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Surveyapp.Models
{
    public class SurveySubject
    {
        public SurveySubject()
        {
            Questions = new HashSet<Question>();
            //ResponseTypes = new ResponseType();
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public string SubjectName { get; set; }
        public Dictionary<string, string> OtherProperties { get; set;}
        /*public string StateCorporation { get; set; }
        public string Chairpersion { get; set; }
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndofTerm { get; set; }*/
        public int CategoryId { get; set; }
        /*public int SubjectTypeId { get; set; }*/
        [ForeignKey("CategoryId")]
        public virtual SurveyCategory Category { get; set; }
        //[ForeignKey("SubjectTypeId")]
        //public virtual SurveySubject SubjectType { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ResponseType ResponseTypes { get; set; }
    }
}
