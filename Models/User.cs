using ApiTest1.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTest1.Models
{
    public class User
    {
        public User() { }
        public User(RegistrationRequest model)
        {
            Email = model.Email;
            UserName = model.Login;
            //BirthDay = DateOnly.Parse(model.BirthDay);
            BirthDay = model.BirthDay;
            //DirthDay = DateOnly.Parse(model.DirthDay);
            PasswordHasher<User> hasher = new();
            PasswordHash = hasher.HashPassword(this, model.Password!);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;
        //public DateOnly DirthDay { get; set; }
        public string? BirthDay { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        public string? RefreshToken { get; set; }

        public virtual Role Roles { get; set; } = null!;

    }
}
