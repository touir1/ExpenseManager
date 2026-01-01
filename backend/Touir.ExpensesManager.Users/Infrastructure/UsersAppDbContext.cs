using Touir.ExpensesManager.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Touir.ExpensesManager.Users.Infrastructure
{
    public class UsersAppDbContext : DbContext
    {
        public UsersAppDbContext(DbContextOptions<UsersAppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Authentication> Authentications { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RequestAccess> RequestAccesses { get; set; }
        public DbSet<RoleRequestAccess> RoleRequestAccesses { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USR_Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id).HasColumnName("USR_Id").UseIdentityAlwaysColumn();
                entity.Property(u => u.FirstName).HasColumnName("USR_FirstName").IsRequired();
                entity.Property(u => u.LastName).HasColumnName("USR_LastName").IsRequired();
                entity.Property(u => u.Email).HasColumnName("USR_Email").IsRequired();
                entity.Property(u => u.FamilyId).HasColumnName("USR_FamilyId");
                entity.Property(u => u.CreatedAt).HasColumnName("USR_CreatedAt");
                entity.Property(u => u.CreatedById).HasColumnName("USR_CreatedBy");
                entity.Property(u => u.LastUpdatedAt).HasColumnName("USR_LastUpdatedAt");
                entity.Property(u => u.LastUpdatedById).HasColumnName("USR_LastUpdatedBy");
                entity.Property(u => u.IsEmailValidated).HasColumnName("USR_IsEmailValidated");
                entity.Property(u => u.EmailValidationHash).HasColumnName("USR_EmailValidationHash");
                entity.Property(u => u.IsDisabled).HasColumnName("USR_IsDisabled");

                entity.HasOne(u => u.CreatedBy)
                      .WithMany()
                      .HasForeignKey(u => u.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.LastUpdatedBy)
                      .WithMany()
                      .HasForeignKey(u => u.LastUpdatedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("RLE_Roles");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id).HasColumnName("RLE_Id").UseIdentityAlwaysColumn();
                entity.Property(r => r.Name).HasColumnName("RLE_Name").IsRequired();
                entity.Property(r => r.Description).HasColumnName("RLE_Description");
                entity.Property(r => r.IsDefault).HasColumnName("RLE_IsDefault").IsRequired();
                entity.Property(r => r.Code).HasColumnName("RLE_Code").IsRequired();
                entity.Property(r => r.ApplicationId).HasColumnName("RLE_ApplicationId");

                entity.HasOne(x => x.Application)
                    .WithMany(a => a.Roles)
                    .HasForeignKey(x => x.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Authentication>(entity =>
            {
                entity.ToTable("ATH_Authentications");

                entity.HasKey(a => a.UserId);

                entity.Property(a => a.UserId).HasColumnName("ATH_UserId");
                entity.Property(a => a.HashPassword).HasColumnName("ATH_HashPassword");
                entity.Property(a => a.HashSalt).HasColumnName("ATH_HashSalt");
                entity.Property(a => a.IsTemporaryPassword).HasColumnName("ATH_IsTemporaryPassword");
                entity.Property(a => a.PasswordResetHash).HasColumnName("ATH_PasswordResetHash");
                entity.Property(a => a.PasswordResetRequestedAt).HasColumnName("ATH_PasswordResetRequestedAt");

                entity.Ignore(a => a.HashPasswordBytes);
                entity.Ignore(a => a.HashSaltBytes);

                entity.HasOne(a => a.User)
                      .WithOne(u => u.Authentication)
                      .HasForeignKey<Authentication>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Application>(entity =>
            {
                entity.ToTable("APP_Applications");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("APP_Id").UseIdentityAlwaysColumn();
                entity.Property(x => x.Code).HasColumnName("APP_Code").IsRequired();
                entity.Property(x => x.Name).HasColumnName("APP_Name").IsRequired();
                entity.Property(x => x.Description).HasColumnName("APP_Description");
                entity.Property(x => x.UrlPath).HasColumnName("APP_UrlPath");
                entity.Property(x => x.ResetPasswordUrlPath).HasColumnName("APP_ResetPasswordUrlPath");

                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<RequestAccess>(entity =>
            {
                entity.ToTable("RQA_RequestAccesses");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("RQA_Id").UseIdentityAlwaysColumn();
                entity.Property(x => x.Name).HasColumnName("RQA_Name");
                entity.Property(x => x.Description).HasColumnName("RQA_Description");
                entity.Property(x => x.Path).HasColumnName("RQA_Path");
                entity.Property(x => x.Type).HasColumnName("RQA_Type");
                entity.Property(x => x.IsAuthenticationNeeded).HasColumnName("RQA_IsAuthenticationNeeded");
                entity.Property(x => x.ApplicationId).HasColumnName("RQA_ApplicationId");

                entity.HasOne(x => x.Application)
                    .WithMany(a => a.RequestAccesses)
                    .HasForeignKey(x => x.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RoleRequestAccess>(entity =>
            {
                entity.ToTable("RRA_RoleRequestAccesses");
                entity.HasKey(x => new { x.RoleId, x.RequestAccessId });

                entity.Property(x => x.RoleId).HasColumnName("RRA_RoleId");
                entity.Property(x => x.RequestAccessId).HasColumnName("RRA_RequestAccessId");

                entity.HasOne(x => x.Role)
                    .WithMany(r => r.RoleRequestAccesses)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RequestAccess)
                    .WithMany(r => r.RoleRequestAccesses)
                    .HasForeignKey(x => x.RequestAccessId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("URR_UserRoles");
                entity.HasKey(x => new { x.UserId, x.RoleId });

                entity.Property(x => x.UserId).HasColumnName("URR_UserId");
                entity.Property(x => x.RoleId).HasColumnName("URR_RoleId");
                entity.Property(x => x.CreatedAt).HasColumnName("URR_CreatedAt");
                entity.Property(x => x.CreatedById).HasColumnName("URR_CreatedById");

                entity.HasOne(u => u.CreatedBy)
                      .WithMany()
                      .HasForeignKey(u => u.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AllowedOrigin>(entity =>
            {
                entity.ToTable("ALW_AllowedOrigins");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("ALW_Id").UseIdentityAlwaysColumn();
                entity.Property(x => x.Url).HasColumnName("ALW_Url").IsRequired();
                entity.Property(x => x.IsGlobal).HasColumnName("ALW_IsGlobal").IsRequired();
                entity.Property(x => x.ApplicationId).HasColumnName("ALW_ApplicationId");

                entity.HasOne(x => x.Application)
                    .WithMany(a => a.AllowedOrigins)
                    .HasForeignKey(x => x.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
