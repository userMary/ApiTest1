using System.ComponentModel.DataAnnotations;

namespace ApiTest1.ViewModels
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Password can not be empty")]
        public string? Password { get; set; }
    }
}
