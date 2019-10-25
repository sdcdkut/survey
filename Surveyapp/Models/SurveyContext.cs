using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surveyapp.Models
{
    public class SurveyContext : IdentityDbContext<ApplicationUser>
    {
        public SurveyContext()
        {

        }
        public SurveyContext(DbContextOptions<SurveyContext> options) : base(options)
        {

        }
        public DbSet<Survey> Survey { get; set; }
        public DbSet<SurveyCategory> SurveyCategory { get; set; }
        public DbSet<SurveySubject> SurveySubject { get; set; }
        public DbSet<ResponseType> ResponseType { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<SurveyResponse> SurveyResponse { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ResponseType>(entity =>
            {
                entity.HasIndex(e => e.Id);

                entity.Property(e => e.ResponseDictionary).IsRequired().
                HasConversion(
                    x => JsonConvert.SerializeObject(x),
                    v => v== null? new Dictionary<string, string>(): JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

                entity.HasOne(d => d.Subject)
                    .WithOne(p => p.ResponseTypes)
                    //.HasForeignKey(d => d.SubjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

            });
            modelBuilder.Entity<SurveyCategory>(entity =>
            {
                entity.HasIndex(e => e.Id);


                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.SurveyCategorys)
                    .OnDelete(DeleteBehavior.ClientSetNull);

            });
            modelBuilder.Entity<SurveySubject>(entity =>
            {
                entity.HasIndex(e => e.Id);

                entity.Property(e => e.OtherProperties).IsRequired().
                    HasConversion(
                        x => JsonConvert.SerializeObject(x),
                        v => v== null? new Dictionary<string, string>(): JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.SurveySubjects)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
            modelBuilder.Entity<SurveySubject>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasMany(p => p.Questions)
                .WithOne(t => t.Subject)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
            modelBuilder.Entity<SurveyResponse>(entity =>
            {
                entity.HasIndex(e => e.Id);

                entity.HasOne(d => d.Respondant)
                    .WithMany(p => p.SurveyResponses)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.question)
                    .WithMany(p => p.SurveyResponses)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }
    }
}
