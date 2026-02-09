
using DatabridgeServer.Models;
using DatabridgeServer.Models.MyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabridgeServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<MessageResponse> MessageResponses { get; set; }
        public DbSet<EmployeeResponse> EmployeeResponses { get; set; }
        public DbSet<EmployeeFullResponse> EmployeeFullResponses { get; set; }
        public DbSet<EmployeeResult> EmployeeResults { get; set; }

        public DbSet<UpdateEmployeeRequest> UpdateEmployeeResponses { get; set; }
        public DbSet<DeleteEmployeeResponse> DeleteEmployeeResponses { get; set; }
        // checking process


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure keyless entity types
            modelBuilder.Entity<EmployeeResponse>().HasNoKey();
            modelBuilder.Entity<EmployeeFullResponse>().HasNoKey();
            //modelBuilder.Entity<EmployeeByIdResponse>().HasNoKey();
            modelBuilder.Entity<UpdateEmployeeRequest>().HasNoKey();
            modelBuilder.Entity<DeleteEmployeeResponse>().HasNoKey();
            //checking process
            modelBuilder.Entity<EmployeeResult>().HasNoKey();

        }
    }
}

