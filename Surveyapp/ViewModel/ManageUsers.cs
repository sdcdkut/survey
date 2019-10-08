using Surveyapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surveyapp.ViewModel
{
    public class ManageUsers
    {
        public string Name { set; get; }
        public string ID { set; get; } 
        public List<ApplicationUser> users { set; get; }
    }
}
