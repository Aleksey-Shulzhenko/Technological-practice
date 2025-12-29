using Microsoft.EntityFrameworkCore;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Cylinder> Cylinders { get; set; }
        public DbSet<Salary> Salaries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=PoltavaPromTehGazDB;Trusted_Connection=True;TrustServerCertificate=True;"
            );
        }
    }
}