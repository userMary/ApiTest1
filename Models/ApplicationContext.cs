using Microsoft.EntityFrameworkCore;

namespace ApiTest1.Models
{
    public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
    {
        public DbSet<User> Users {  get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
