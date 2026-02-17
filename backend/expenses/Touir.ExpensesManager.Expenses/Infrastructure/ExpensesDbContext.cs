using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Models.External;

namespace Touir.ExpensesManager.Expenses.Infrastructure
{
    public class ExpensesDbContext : DbContext
    {
        public ExpensesDbContext(DbContextOptions<ExpensesDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USR_Users", schema: "ext");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("USR_Id");
                entity.Property(e => e.FirstName).HasColumnName("USR_FirstName");
                entity.Property(e => e.LastName).HasColumnName("USR_LastName");
                entity.Property(e => e.Email).HasColumnName("USR_Email");
                entity.Property(e => e.FamilyId).HasColumnName("USR_FamilyId");
                entity.Property(e => e.IsDeleted).HasColumnName("USR_IsDeleted");
            });
        }
    }
}