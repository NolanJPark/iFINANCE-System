using System.ComponentModel.DataAnnotations;

namespace Group12_iFINANCEAPP.Models
{
    //Model for login screne
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
