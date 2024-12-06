using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiTest1.Controllers
{
    public class HomeController() : Controller
    {
        [Authorize(CookieAuthenticationDefaults.AuthenticationScheme, Policy = "EmployeeOnly")]
        public IActionResult Index(string username = "")
        {
            ClaimsPrincipal user = HttpContext.User;
            //return Ok("Hello, " + username); // {{ _.url }}?username=Nomnnn // http://localhost:5272?username=Tom
            return Ok("All good cookie");
        }
        [Authorize(JwtBearerDefaults.AuthenticationScheme, Policy = "EmployeeOnly")]
        public IActionResult Test()
        {
            return Ok("All good JWT");
        }
    }
}