using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;

namespace ServiceLocator.Models
{
    public class Dbcontext : DbContext
    {
        public Dbcontext(DbContextOptions<Dbcontext> options)
            : base(options)
        { }
        public DbSet<Customersignup> Customer { get; set; } = null!;
        public DbSet<Providersignup> Provider { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

    }
}

