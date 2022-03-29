using System;

namespace Surveyapp.Models.ViewModels
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        //public string Gender { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string StudentReg { get; set; }
        public string StatusActive { get; set; }
        public string UserId { get; set; }
        public int CourseId { get; set; }
        public string FullName => StudentReg + " " + FirstName + " " + MiddleName + " " + LastName;
        public Guid? CohortId { get; set; }
        public Stage PreviousStage { get; set; } = new Stage();
        public Stage CurrentStage { get; set; } = new Stage();
        public string HomeCounty { get; set; }
        public string SubCounty { get; set; }
        public string NameOfChief { get; set; }
        public string ModeOfEntry { get; set; }
        public string KSCEGrade { get; set; }
        public virtual Course Course { get; set; }

        public virtual ApplicationUser studUser { get; set; }
        //public Cohort Cohort { get; set; }
        //public virtual ICollection<SemesterGroupStudents> SemesterGroupStudents { get; set; }
        //public virtual ICollection<ParentsGuardianDetails> ParentsGuardianDetails { get; set; }
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum ModeOfEntry
    {
        KUCCPS,
        SSP,
        APP,
    }

    public enum StatusActive
    {
        Active,
        InActive,
        Differed,
        OnLeave
    }

    public class Stage
    {
        public string SemesterName { get; set; }
        public string LevelOfStudyName { get; set; }
    }
}