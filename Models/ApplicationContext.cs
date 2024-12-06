using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiTest1.Models
{
    public class ApplicationContext(DbContextOptions<ApplicationContext> options) : IdentityDbContext<User>(options)
    {

    }
}
