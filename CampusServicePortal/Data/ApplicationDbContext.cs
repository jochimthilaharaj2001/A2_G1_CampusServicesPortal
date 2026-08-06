using CampusServicePortal.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<StudentMasterList> StudentMasterList { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User ─────────────────────────────────────────────────────────────
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ── Student ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.IndexNumber)
                .IsUnique();

            // ── RefreshToken ──────────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);

            // ── PasswordResetToken ────────────────────────────────────────────────
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(prt => prt.UserId);

            // ── StudentMasterList ─────────────────────────────────────────────────
            modelBuilder.Entity<StudentMasterList>()
                .HasIndex(sml => sml.IndexNumber)
                .IsUnique();

            // ── Role Seed Data ────────────────────────────────────────────────────
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Student" }
            );
        }
    }
}
