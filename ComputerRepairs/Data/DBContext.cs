using ComputerRepairs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ComputerRepairs.Data
{
    public class AppDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        public DbSet<Ticket> Tickets { get; set; }
    }
}
