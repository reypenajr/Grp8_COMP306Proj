using Group8_BrarPena.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Group8_BrarPena.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Group8_BrarPena.Models.Course> Course { get; set; } = default!;
        public DbSet<Group8_BrarPena.Models.Review> Review { get; set; } = default!;

    }
}
