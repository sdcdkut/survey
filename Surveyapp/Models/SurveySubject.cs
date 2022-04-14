using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surveyapp.Models
{
    public class SurveySubject
    {
        public SurveySubject()
        {
            Questions = new HashSet<Question>();
            QuestionGroups = new HashSet<QuestionGroup>();
            //ResponseTypes = new ResponseType();
        }

        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "jsonb")] public List<OtherProperties> OtherProperties { get; set; }

        /*public string StateCorporation { get; set; }
        public string Chairpersion { get; set; }
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndofTerm { get; set; }*/
        [Display(Name = "Category")] public int? CategoryId { get; set; }
        [Display(Name = "Response Type")] public int? ResponseTypeId { get; set; }
        [Display(Name = "Survey")] public int SurveyId { get; set; }

        [Display(Name = "Add Subject On Survey Take")]
        public bool AddAnotherSubjectOnSurveyTake { get; set; } = false;

        [Display(Name = "Course")] public int? CourseId { get; set; }

        [Display(Name = "Department")] public int? DepartmentId { get; set; }
        [Column(TypeName = "jsonb")] public List<DynamicSubjectValue> DynamicSubjectValue { get; set; } = new();
        [ForeignKey("SurveyId")] public virtual Survey Survey { get; set; }

        [ForeignKey("ResponseTypeId")] public virtual ResponseType ResponseType { get; set; }

        /*public int SubjectTypeId { get; set; }*/
        [ForeignKey("CategoryId")] public virtual SurveyCategory Category { get; set; }
        [ForeignKey("CourseId")] public virtual Course Course { get; set; }

        [ForeignKey("DepartmentId")] public virtual Department Department { get; set; }

        //[ForeignKey("SubjectTypeId")]
        //public virtual SurveySubject SubjectType { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<QuestionGroup> QuestionGroups { get; set; }

        [Display(Name = "Dynamically Created (on survey take)")]
        public bool DynamicallyCreated { get; set; } = false;
        //public virtual ResponseType ResponseTypes { get; set; }
    }

    public class DynamicSubjectValue
    {
        [Display(Name = "Field name")] public string Name { get; set; }
        public List<string> SelectValueOptions { get; set; }
    }

    public class OtherProperties
    {
        public string Name { get; set; }

        [Display(Name = "value of the property")]
        public string Value { get; set; }
    }
}