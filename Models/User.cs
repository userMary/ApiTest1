using ApiTest1.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ApiTest1.Models
{
    public class User : IdentityUser
    {
        public User() { }
        public User(RegistrationRequest model)
        {
            Email = model.Email;
            //BirthDay = DateOnly.Parse(model.BirthDay);
            BirthDay = model.BirthDay;
            PasswordHasher<User> hasher = new();
            PasswordHash = hasher.HashPassword(this, model.Password!);
            EmailConfirmed = false;
            PhoneNumberConfirmed = false;
            TwoFactorEnabled = false;
            LockoutEnabled = false;
            AccessFailedCount = 0;
        }
        public DateOnly BirthDay { get; set; }
        //public string BirthDay { get; set; } = "";
    }
}
