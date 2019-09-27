using Newtonsoft.Json;
using System;
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
        }
        public int Id { get; set; }
        [Required]
        public string name { get; set; }
        [DataType(DataType.Date)]
        [Required]
        public DateTime Startdate { get; set; }
        [DataType(DataType.Date)]
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string status { get; set; }
        [Required]
        public string SurveyerId { get; set; }
        [ForeignKey("SurveyerId")]
        public virtual ApplicationUser Surveyer { get; set; }
        public virtual ICollection<SurveyCategory> SurveyCategorys{ get; set; }
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
}
