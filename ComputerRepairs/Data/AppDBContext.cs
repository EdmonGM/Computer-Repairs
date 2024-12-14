using ComputerRepairs.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ComputerRepairs.Data
{
    public class AppDBContext(DbContextOptions dbContextOptions) : IdentityDbContext<AppUser>(dbContextOptions)
    {
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            modelBuilder.Entity<AppUser>().HasMany(a => a.Tickets).WithOne(t => t.AppUser).HasForeignKey(t => t.UserId); 
            base.OnModelCreating(modelBuilder); 
        }
    }
}
