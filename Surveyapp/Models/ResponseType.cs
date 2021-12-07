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
        public ResponseType()
        {
            SurveySubjects = new HashSet<SurveySubject>();
            Questions = new HashSet<Question>();
        }
        public int Id { get; set; }
        [Required] [DataType(DataType.Text)] public string ResponseName { get; set; }

        /*[Required]
        public int SubjectId { get; set; }*/
        // [Required]
        [Column(TypeName = "jsonb")] public List<ResponseDictionary> ResponseDictionary { get; set; }
        [Display(Name = "Display as")] public DisplayOptionType? DisplayOptionType { get; set; }

        public string? CreatorId { get; set; }
        [ForeignKey("CreatorId")] public virtual ApplicationUser Creator { get; set; }
        public ICollection<SurveySubject> SurveySubjects { get; set; }
        public ICollection<Question> Questions { get; set; }
    }

    public enum DisplayOptionType
    {
        RadioButton = 1,
        DropDown = 2,
        Input = 3
    }

    public class ResponseDictionary
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}