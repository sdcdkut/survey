using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Surveyapp.Models
{
    public class Survey
    {
        public Survey()
        {
            SurveyCategorys = new HashSet<SurveyCategory>();
            Surveyors = new HashSet<Surveyors>();
            SurveySubjects = new HashSet<SurveySubject>();
            SurveyParticipants = new HashSet<SurveyParticipants>();
        }

        public int Id { get; set; }
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        [DataType(DataType.Date)] [Required] public DateTime Startdate { get; set; }
        [DataType(DataType.Date)] [Required] public DateTime EndDate { get; set; }
        [Required] public string status { get; set; }
        public string approvalStatus { get; set; }
        [Display(Name = "Listed On SurveyList Page")]
        public bool ListedOnSurveyListPage { get; set; } = true;

        //[Required]
        //public string SurveyerId { get; set; }
        //[ForeignKey("SurveyerId")]
        //public virtual ApplicationUser Surveyer { get; set; }
        public virtual ICollection<SurveyCategory> SurveyCategorys { get; set; }
        public virtual ICollection<SurveySubject> SurveySubjects { get; set; }
        public virtual ICollection<Surveyors> Surveyors { get; set; }
        public virtual ICollection<SurveyParticipants> SurveyParticipants { get; set; }

        public string SurveyorNames => string.Join(",", Surveyors.Where(c => c?.ActiveStatus == true).Select(c => c?.Surveyor?.UserName));
        // public string AttributesData
        // {
        //     get
        //     {
        //         var xElem = new XElement(
        //             "items",
        //             Attributes.Select(x => new XElement("item", new XAttribute("key", x.Key), new XAttribute("value", x.Value)))
        //          );
        //         return xElem.ToString();
        //     }
        //     set
        //     {
        //         var xElem = XElement.Parse(value);
        //         var dict = xElem.Descendants("item")
        //                             .ToDictionary(
        //                                 x => (string)x.Attribute("key"),
        //                                 x => (string)x.Attribute("value"));
        //         Attributes = dict;
        //     }
        // }

        //Some other stuff

        /// <summary>
        /// Some cool description
        /// </summary>
        // [NotMapped]
        // public Dictionary<string, string> Attributes { get; set; }
        // [NotMapped]
        // public Dictionary<string, string> MyDictionary
        // {
        //     get; set;
        // }

        //public string DictionaryAsXml
        //{
        //    get
        //    {
        //        return MyDictionary == null ? null : JsonConvert.DeserializeObject<Dictionary<string,string>>(value);
        //        //return ToXml(MyDictionary);
        //    }
        //    set
        //    {
        //        MyDictionary = FromXml(value);
        //    }
        //}
    }

    public class Surveyors
    {
        public Guid Id { get; set; }
        public string SurveyorId { get; set; }
        public int SurveyId { get; set; }
        public bool ActiveStatus { get; set; }
        public SurveyPermission Permission { get; set; }
        [ForeignKey("SurveyorId")] public virtual ApplicationUser Surveyor { get; set; }
        [ForeignKey("SurveyId")] public virtual Survey Survey { get; set; }
        public bool Owner { get; set; } = true;
    }

    public class SurveyParticipants
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string ParticipantId { get; set; }
        [ForeignKey("ParticipantId")] public virtual ApplicationUser Participant { get; set; }
        [ForeignKey("SurveyId")] public virtual Survey Survey { get; set; }
    }

    public enum SurveyPermission
    {
        AllPermissions = 1,
        Read = 2,
        Write = 3,
        ViewReport = 4
    }
}