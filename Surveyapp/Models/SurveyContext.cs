using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Surveyapp.Models
{
    public class SurveyContext : IdentityDbContext<ApplicationUser>
    {
        public SurveyContext(DbContextOptions<SurveyContext> options) : base(options)
        {

        }
        public DbSet<Survey> Survey { get; set; }
        public DbSet<Surveyors> Surveyors { get; set; }
        public DbSet<SurveyCategory> SurveyCategory { get; set; }
        public DbSet<SurveySubject> SurveySubject { get; set; }
        public DbSet<ResponseType> ResponseType { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<QuestionGroup> QuestionGroups { get; set; }
        public DbSet<SurveyResponse> SurveyResponse { get; set; }
        public DbSet<SurveyParticipants> SurveyParticipants { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<SchoolOrInstitution> SchoolOrInstitutions { get; set; }
        public DbSet<Campus> Campus { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ResponseType>(entity =>
            {
                entity.HasIndex(e => e.Id);
                /*entity.Property(e => e.ResponseDictionary).IsRequired().
                HasConversion(
                    x => JsonConvert.SerializeObject(x),
                    v => v== null? new Dictionary<string, string>(): JsonConvert.DeserializeObject<Dictionary<string, string>>(v));*/
                /*entity.HasOne(d => d.Subject)
                    .WithOne(p => p.ResponseTypes)
                    //.HasForeignKey(d => d.SubjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull);*/
            });
            modelBuilder.Entity<Survey>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(e => e.Id);
                entity.HasIndex(c => new { c.Name, c.Startdate, c.EndDate, c.status }).IsUnique();
            });
            modelBuilder.Entity<SurveyCategory>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.SurveyCategorys)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<SurveySubject>(entity =>
            {
                entity.HasIndex(e => e.Id);

                /*entity.Property(e => e.DynamicSubjectValue).IsRequired().
                    HasConversion(
                        x => JsonConvert.SerializeObject(x),
                        v => v== null? new List<DynamicSubjectValue>(): JsonConvert.DeserializeObject<List<DynamicSubjectValue>>(v));*/

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.SurveySubjects)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.SurveySubjects)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<SurveySubject>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasMany(p => p.Questions)
                .WithOne(t => t.Subject)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<QuestionGroup>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasMany(p => p.Questions)
                    .WithOne(t => t.QuestionGroup)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasIndex(c => new { c.question, c.SubjectId }).IsUnique();
                entity.HasMany(p => p.SurveyResponses)
                    .WithOne(t => t.question)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<SurveyResponse>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasOne(d => d.Respondant)
                    .WithMany(p => p.SurveyResponses)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(p => p.question)
                    .WithMany(t => t.SurveyResponses)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
