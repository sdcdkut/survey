using Surveyapp.Models;
using System.Collections.Generic;

namespace Surveyapp.ViewModel
{
    public class ManageUsers
    {
        public string Name { set; get; }
        public string ID { set; get; } 
        public List<ApplicationUser> users { set; get; }
    }
}
