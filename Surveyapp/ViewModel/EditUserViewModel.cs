using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Surveyapp.ViewModel
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            Roles = new List<string>();
        }

        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required][EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public IList<string> Roles { get; set; }

        public bool locked { set; get; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime? LockEnd { get; set; }
        public int? CourseId { get; set; }
        public int? DepartmentId { get; set; }

    }
}
