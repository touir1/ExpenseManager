using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Infrastructure
{
    public class ExpensesDbContext : DbContext
    {
        public ExpensesDbContext(DbContextOptions<ExpensesDbContext> options) : base(options) { }

        // Application entities
        public DbSet<User> Users { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<FamilyMembership> FamilyMemberships { get; set; }
        public DbSet<FamilyInvitation> FamilyInvitations { get; set; }
        public DbSet<ExpenseFamilyAttribution> ExpenseFamilyAttributions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ExpenseTag> ExpenseTags { get; set; }
        public DbSet<CurrencyDailyRate> CurrencyDailyRates { get; set; }
        public DbSet<CurrencyPairDefault> CurrencyPairDefaults { get; set; }
        public DbSet<CurrencyRateConflict> CurrencyRateConflicts { get; set; }
        public DbSet<ExpenseAuditLog> ExpenseAuditLogs { get; set; }
        public DbSet<ExpenseAuditSnapshot> ExpenseAuditSnapshots { get; set; }

        public DbSet<InboxEvent> InboxEvents { get; set; }

        // Lookup tables
        public DbSet<OperationSource> OperationSources { get; set; }
        public DbSet<ModifiedSource> ModifiedSources { get; set; }
        public DbSet<FamilyRole> FamilyRoles { get; set; }
        public DbSet<RateSource> RateSources { get; set; }
        public DbSet<ConflictStatus> ConflictStatuses { get; set; }
        public DbSet<ConflictResolution> ConflictResolutions { get; set; }
        public DbSet<AuditOperation> AuditOperations { get; set; }
        public DbSet<SnapshotType> SnapshotTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Lookup tables ─────────────────────────────────────────────────

            modelBuilder.Entity<OperationSource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasData(
                    new OperationSource { Id = 1, Name = "SingleWeb" },
                    new OperationSource { Id = 2, Name = "SingleMobile" },
                    new OperationSource { Id = 3, Name = "BulkWeb" });
            });

            modelBuilder.Entity<ModifiedSource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasData(
                    new ModifiedSource { Id = 1, Name = "Web" },
                    new ModifiedSource { Id = 2, Name = "Mobile" });
            });

            modelBuilder.Entity<FamilyRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasData(
                    new FamilyRole { Id = 1, Name = "Head" },
                    new FamilyRole { Id = 2, Name = "Member" });
            });

            modelBuilder.Entity<RateSource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasData(
                    new RateSource { Id = 1, Name = "Auto" },
                    new RateSource { Id = 2, Name = "Manual" });
            });

            modelBuilder.Entity<ConflictStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasData(
                    new ConflictStatus { Id = 1, Name = "Pending" },
                    new ConflictStatus { Id = 2, Name = "Resolved" });
            });

            modelBuilder.Entity<ConflictResolution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasData(
                    new ConflictResolution { Id = 1, Name = "AcceptAuto" },
                    new ConflictResolution { Id = 2, Name = "KeepManual" },
                    new ConflictResolution { Id = 3, Name = "Custom" });
            });

            modelBuilder.Entity<AuditOperation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasData(
                    new AuditOperation { Id = 1, Name = "Add" },
                    new AuditOperation { Id = 2, Name = "Update" },
                    new AuditOperation { Id = 3, Name = "Delete" });
            });

            modelBuilder.Entity<SnapshotType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasData(
                    new SnapshotType { Id = 1, Name = "Before" },
                    new SnapshotType { Id = 2, Name = "After" });
            });

            // ── External ──────────────────────────────────────────────────────

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

            // ── Currency ──────────────────────────────────────────────────────

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Symbol).HasMaxLength(10).IsRequired();
            });

            // ── Category ──────────────────────────────────────────────────────

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.DeletedAt).IsRequired(false);
                entity.HasOne(e => e.ParentCategory)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Expense ───────────────────────────────────────────────────────

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 4);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.ModifiedById)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Currency)
                      .WithMany()
                      .HasForeignKey(e => e.CurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Subcategory)
                      .WithMany()
                      .HasForeignKey(e => e.SubcategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CreatedFrom)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedFromId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ModifiedFrom)
                      .WithMany()
                      .HasForeignKey(e => e.ModifiedFromId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.DeletedAt).IsRequired(false);

                entity.HasIndex(e => new { e.UserId, e.Date });
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.CurrencyId);
            });

            // ── Family ────────────────────────────────────────────────────────

            modelBuilder.Entity<Family>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.DeletedAt).IsRequired(false);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── FamilyMembership ──────────────────────────────────────────────

            modelBuilder.Entity<FamilyMembership>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Family)
                      .WithMany()
                      .HasForeignKey(e => e.FamilyId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.FamilyId);
            });

            // ── FamilyInvitation ──────────────────────────────────────────────

            modelBuilder.Entity<FamilyInvitation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InviteeEmail).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Token).HasMaxLength(36).IsRequired();
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasOne(e => e.Family)
                      .WithMany()
                      .HasForeignKey(e => e.FamilyId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.InvitedById)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.AcceptedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.FamilyId, e.InviteeEmail });
            });

            // ── ExpenseFamilyAttribution ──────────────────────────────────────

            modelBuilder.Entity<ExpenseFamilyAttribution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Expense)
                      .WithMany()
                      .HasForeignKey(e => e.ExpenseId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Family)
                      .WithMany()
                      .HasForeignKey(e => e.FamilyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.AttributedById)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.FamilyId);
                entity.HasIndex(e => e.ExpenseId);
            });

            // ── Tag ───────────────────────────────────────────────────────────

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ExpenseTag ────────────────────────────────────────────────────

            modelBuilder.Entity<ExpenseTag>(entity =>
            {
                entity.HasKey(e => new { e.ExpenseId, e.TagId });
                entity.HasOne(e => e.Expense)
                      .WithMany()
                      .HasForeignKey(e => e.ExpenseId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Tag)
                      .WithMany()
                      .HasForeignKey(e => e.TagId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── CurrencyDailyRate ─────────────────────────────────────────────

            modelBuilder.Entity<CurrencyDailyRate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rate).HasPrecision(18, 8);
                entity.HasOne(e => e.SourceCurrency)
                      .WithMany()
                      .HasForeignKey(e => e.SourceCurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.DestinationCurrency)
                      .WithMany()
                      .HasForeignKey(e => e.DestinationCurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.RateSource)
                      .WithMany()
                      .HasForeignKey(e => e.RateSourceId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.SourceCurrencyId, e.DestinationCurrencyId, e.Date })
                      .IsUnique();
            });

            // ── CurrencyPairDefault ───────────────────────────────────────────

            modelBuilder.Entity<CurrencyPairDefault>(entity =>
            {
                entity.HasKey(e => new { e.SourceCurrencyId, e.DestinationCurrencyId });
                entity.Property(e => e.Rate).HasPrecision(18, 8);
                entity.HasOne(e => e.SourceCurrency)
                      .WithMany()
                      .HasForeignKey(e => e.SourceCurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.DestinationCurrency)
                      .WithMany()
                      .HasForeignKey(e => e.DestinationCurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── CurrencyRateConflict ──────────────────────────────────────────

            modelBuilder.Entity<CurrencyRateConflict>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AutomaticRate).HasPrecision(18, 8);
                entity.Property(e => e.ManualRate).HasPrecision(18, 8);
                entity.Property(e => e.CustomRate).HasPrecision(18, 8);
                entity.HasOne(e => e.SourceCurrency)
                      .WithMany()
                      .HasForeignKey(e => e.SourceCurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.DestinationCurrency)
                      .WithMany()
                      .HasForeignKey(e => e.DestinationCurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Status)
                      .WithMany()
                      .HasForeignKey(e => e.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Resolution)
                      .WithMany()
                      .HasForeignKey(e => e.ResolutionId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.ResolvedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ExpenseAuditLog ───────────────────────────────────────────────

            modelBuilder.Entity<ExpenseAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Expense)
                      .WithMany()
                      .HasForeignKey(e => e.ExpenseId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Operation)
                      .WithMany()
                      .HasForeignKey(e => e.OperationId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.PerformedFrom)
                      .WithMany()
                      .HasForeignKey(e => e.PerformedFromId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.PerformedById)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.ExpenseId);
            });

            // ── ExpenseAuditSnapshot ──────────────────────────────────────────

            modelBuilder.Entity<ExpenseAuditSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 4);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(1000);
                entity.Property(e => e.Families).HasMaxLength(1000);
                entity.HasOne(e => e.AuditLog)
                      .WithMany()
                      .HasForeignKey(e => e.AuditLogId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Currency)
                      .WithMany()
                      .HasForeignKey(e => e.CurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SnapshotType)
                      .WithMany()
                      .HasForeignKey(e => e.SnapshotTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Inbox ─────────────────────────────────────────────────────────

            modelBuilder.Entity<InboxEvent>(entity =>
            {
                entity.ToTable("InboxEvents");
                entity.HasKey(e => e.MessageId);
                entity.Property(e => e.MessageId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ReceivedAt).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Error).HasMaxLength(2000);
                entity.HasIndex(e => e.ReceivedAt);
            });
        }
    }
}
