using System.ComponentModel.DataAnnotations;

namespace ApiTest1.ViewModels
{
    public class RegistrationRequest
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress]
        public string? Email { get; set; }


        [Required(ErrorMessage = "BirthDay is required field")]
        //public string? BirthDay { get; set; }
        public DateOnly BirthDay { get; set; }

        
        
        [Required(ErrorMessage = "Password can not be empty")]
        public string? Password { get; set; }
        
        
        [Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }
    }
}
