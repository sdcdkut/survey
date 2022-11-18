#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surveyapp.Models
{
    public class Course
    {
        public Course()
        {
            ApplicationUsers = new HashSet<ApplicationUser>();
        }

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        [NotMapped]
        public string? couserName { get; set; }
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")] public virtual Department? Department { get; set; }

        public ICollection<ApplicationUser>? ApplicationUsers { get; set; }
    }

    public class Department
    {
        public Department()
        {
            Courses = new HashSet<Course>();
            ApplicationUsers = new HashSet<ApplicationUser>();
        }

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int? SchoolOrInstitutionId { get; set; }
        [ForeignKey("SchoolOrInstitutionId")] public virtual SchoolOrInstitution? SchoolOrInstitution { get; set; }
        public ICollection<Course>? Courses { get; set; }
        public ICollection<ApplicationUser>? ApplicationUsers { get; set; }
    }

    public class SchoolOrInstitution
    {
        public SchoolOrInstitution()
        {
            Departments = new HashSet<Department>();
        }

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int? CampusId { get; set; }
        [ForeignKey("CampusId")] public virtual Campus? Campus { get; set; }
        public ICollection<Department>? Departments { get; set; }
    }

    public class Campus
    {
        public Campus()
        {
            SchoolOrInstitutions = new HashSet<SchoolOrInstitution>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<SchoolOrInstitution>? SchoolOrInstitutions { get; set; }
    }
}