using System.ComponentModel.DataAnnotations;

namespace Group12_iFINANCEB.Models
{
    //Model for the changing password
    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }

        //Basic error trap
        [Required, DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords must match.")]
        public string ConfirmPassword { get; set; }
    }
}
