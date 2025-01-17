using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ApiTest1.Models;
using ApiTest1.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiTest1.Controllers
{
    [ApiController]
    [EnableCors("MyPolicy")]
    [Route("[controller]/[action]")]
    [Produces("application/json")]
    public class AccountController(ApplicationContext context) : Controller
    {
        /// <summary>
        /// Метод регистрации пользователей
        /// </summary>
        /// <param name="model">RegistrationRequest</param>
        /// <returns>RegistrationResponse</returns>
        [HttpPost]
        public async Task<ActionResult> Registration([FromBody] RegistrationRequest model)
        {
            // Проверяем, что такой пользователь существует
            User? user = await context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user is not null)
            {
                return BadRequest(new
                {
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = new Dictionary<string, string[]>() { { "Email", ["Email is already taken"] } }
                });
            }

            string refreshToker = GetRefreshToken();

            // Создаем нового пользователя с ролью "User" по умолчанию
            User newUser = new(model) {
                Roles = context.Roles.FirstOrDefault(x => x.Name == "User"),
                RefreshToken = refreshToker
            };
            context.Users.Add(newUser);
            context.SaveChanges();

            // Добавляем в http-only cookie refresh токен
            CookieOptions options = new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", GetRefreshToken(), options);

            // создаем один claim с ролью пользователя
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultRoleClaimType, newUser.Roles.Name)
            };
            return Json(new RegistrationResponse()
            {
                Token = AuthenticateJWT(claims),
                Status = true
            });
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            // Проверяем, что такой пользователь существует
            User? user = await context.Users
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user is null)
            {
                return BadRequest(new
                {
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = new Dictionary<string, string[]>() { { "Email", ["Email or password is incorrect"] } }
                });
            }

            string refreshToken = GetRefreshToken();
            user.RefreshToken = refreshToken;
            context.Update(user);
            await context.SaveChangesAsync();

            // Проверяем, что пароли совпадают
            PasswordHasher<User> hasher = new();
            PasswordVerificationResult result = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == 0)
            {
                return BadRequest(new
                {
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = new Dictionary<string, string[]>() { { "Email", ["Email or password is incorrect"] } }
                });
            }

            // Добавляем в http-only cookie refresh токен
            CookieOptions options = new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", GetRefreshToken(), options);

            // Создаем один claim с ролью пользователя
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultRoleClaimType, user.Roles.Name)
            };
            return Json(new RegistrationResponse()
            {
                Token = AuthenticateJWT(claims),
                Status = true
            });
        }

        /// <summary>
        /// User refresh token method
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Refresh()
        {
            string? refreshToken = Request.Cookies["refreshToken"];
            Console.WriteLine(refreshToken);
            User? user = await context.Users
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (user is null)
            {
                return BadRequest();
            }

            //user.RefreshToken = GetRefreshToken();
            //context.SaveChanges();

            // создаем один claim с ролью пользователя
            var claims = new List<Claim>
            {
                new (ClaimsIdentity.DefaultRoleClaimType, user.Roles.Name)
            };
            return Json(new RegistrationResponse()
            {
                Token = AuthenticateJWT(claims),
                Status = true
            });
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

        private string GetRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToHexString(randomNumber);
        }
    }
}
