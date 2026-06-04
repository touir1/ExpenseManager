using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Notifications.Models;

namespace Touir.ExpensesManager.Notifications.Infrastructure
{
    public class NotificationsDbContext : DbContext
    {
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<InboxEvent> InboxEvents { get; set; }

        public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                if (Database.IsNpgsql())
                    entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                else
                    entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Type).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Payload).IsRequired();
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.ReadAt).IsRequired(false);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                if (Database.IsNpgsql())
                    entity.HasIndex(e => e.UserId)
                        .HasFilter("\"IsRead\" = false")
                        .HasDatabaseName("IX_Notifications_UserId_Unread");
            });

            modelBuilder.Entity<InboxEvent>(entity =>
            {
                entity.HasKey(e => e.MessageId);
                entity.Property(e => e.MessageId).HasMaxLength(100);
                entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Error).IsRequired(false);
            });
        }
    }
}
