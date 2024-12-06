using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiTest1.Models;
using ApiTest1.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiTest1.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    [Produces("application/json")]
    public class AccountController(ApplicationContext context) : Controller
    {
        /// <summary>
        /// Метод регистрации пользователей
        /// </summary>
        /// <param name="model">RegistrationRequest</param>
        /// <returns>RegistrationResponse</returns>
        [HttpPost]
        public async Task<RegistrationResponse> RegistrationAsync([FromBody] RegistrationRequest model)
        { 
            if (!ModelState.IsValid)
            {
                Dictionary<string, string[]> errorList = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return new RegistrationResponse
                {
                    ErrorList = errorList,
                    Status = false
                };
            }
            User? user = await context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user is not null)
            {
                Dictionary<string, string[]> errorList = new()
                {
                    {
                        "email", ["Email is already taken"]
                    }
                };
                return new RegistrationResponse
                {
                    ErrorList = errorList,
                    Status = false
                };
            }


            User newUser = new(model);
            context.Users.Add(newUser);
            context.SaveChanges();

            // создаем один claim
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultRoleClaimType, "Admin"),
                new(ClaimsIdentity.DefaultNameClaimType, "Test username")
            };
            return new RegistrationResponse()
            {
                Token = AuthenticateJWT(claims),
                Status = true
            };
        }

        [HttpGet]
        public IActionResult Login()
        {
            return Ok("<h1>Страница логина<h1>");
        }

        private async void Authenticate(List<Claim> claims)
        {
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        private string AuthenticateJWT(List<Claim> claims)
        {            
            DateTime expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(2));
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes("rOe7o1N80fT1+f4tw5o4oxHRVGcoiYZ1ow5hIBEHvtk="));
            SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken jwt = new(
                    issuer: "http://localhost:5272",
                    audience: "http://localhost:5272",
                    claims: claims,
                    expires: expires,
                    signingCredentials: credentials
            );
            JwtSecurityTokenHandler handler = new();
            return handler.WriteToken(jwt);
        }
    }
}
