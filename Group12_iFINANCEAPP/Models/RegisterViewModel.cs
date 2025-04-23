using System;
using System.ComponentModel.DataAnnotations;

namespace Group12_iFINANCEAPP.Models
{
    //Model for registering a user
    //this was changed to just Manage User since only the admin can see it.
    //Lets just leave it as this name cause changing it would be a headache
    public class RegisterViewModel
    {
        [Required] public string Name { get; set; }
        public string Address { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Administrator?")]
        public bool IsAdministrator { get; set; }
    }
}
