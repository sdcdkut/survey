using System;
using System.Collections.Generic;
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
            ResponseTypes = new HashSet<ResponseType>();
        }
        public int Id { get; set; }
        public string SubjectName { get; set; }
        public string StateCorporation { get; set; }
        public string Chairpersion { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime EndofTerm { get; set; }
        public int CategoryId { get; set; }
        /*public int SubjectTypeId { get; set; }*/
        [ForeignKey("CategoryId")]
        public virtual SurveyCategory Category { get; set; }
        //[ForeignKey("SubjectTypeId")]
        //public virtual SurveySubject SubjectType { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<ResponseType> ResponseTypes { get; set; }
    }
}
