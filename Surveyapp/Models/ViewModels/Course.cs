using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surveyapp.Models.ViewModels
{
    public class Course
    {
        
        public int Id { get; set; }

        public string CouserName { get; set; }
        public string Code { get; set; }
        public int? DepartmentId { get; set; }

        public int Duration { get; set; } = 0;

        public string? CourseType { get; set; }

        [ForeignKey("DepartmentId")] public virtual Department Department { get; set; }
        //public virtual ICollection<Academicyear> Academicyears { get; set; }
    }

    public enum CourseType
    {
        Certificate = 0,
        Short_Course = 1,
        Diploma = 2,
        Degree = 3,
        Masters = 4,
        PhD = 5
    }

    public class Department
    {
        public Department()
        {
            Courses = new HashSet<Course>();
            //Academicyears = new HashSet<Academicyear>();
            //Units = new HashSet<Unit>();
            DepartmentUsers = new HashSet<ApplicationUser>();
            //SubActivityScheduleItemDepartmentsInvolved = new HashSet<SubActivityScheduleItemDepartmentsInvolved>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? SchoolOrInstitutionId { get; set; }
        [ForeignKey("SchoolOrInstitutionId")] public SchoolOrInstitution SchoolOrInstitution { get; set; }
        //public virtual ICollection<Academicyear> Academicyears { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
        //public virtual ICollection<Unit> Units { get; set; }
        public virtual ICollection<ApplicationUser> DepartmentUsers { get; set; }
        ///public ICollection<SubActivityScheduleItemDepartmentsInvolved> SubActivityScheduleItemDepartmentsInvolved { get; set; }
        public string ReferenceName => $"{Code}:{Name}";
    }

    public class SchoolOrInstitution
    {
        public SchoolOrInstitution()
        {
            Departments = new HashSet<Department>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Department> Departments { get; set; }
    }
}