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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<EmployeeResponse>().HasNoKey();
            modelBuilder.Entity<EmployeeFullResponse>().HasNoKey();
            modelBuilder.Entity<UpdateEmployeeRequest>().HasNoKey();
            modelBuilder.Entity<DeleteEmployeeResponse>().HasNoKey();
            modelBuilder.Entity<EmployeeResult>().HasNoKey();
        }
    }
}