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
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<CertificateType> CertificateTypes { get; set; }
        public DbSet<Lab> Labs { get; set; }
        public DbSet<LabSeat> LabSeats { get; set; }
        public DbSet<LabReservation> LabReservations { get; set; }

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

            modelBuilder.Entity<Student>()
                .HasOne(s => s.FacultyNav)
                .WithMany(f => f.Students)
                .HasForeignKey(s => s.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // ── Faculty ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Faculty>()
                .HasIndex(f => f.Name)
                .IsUnique();

            // ── CertificateType ───────────────────────────────────────────────────
            modelBuilder.Entity<CertificateType>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // ── Role Seed Data ────────────────────────────────────────────────────
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Student" }
            );

            // ── LabReservation → User ─────────────────────────────────────────
            modelBuilder.Entity<LabReservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.LabReservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── LabReservation → Lab ──────────────────────────────────────────
            modelBuilder.Entity<LabReservation>()
                .HasOne(r => r.Lab)
                .WithMany()
                .HasForeignKey(r => r.LabId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Lab → LabSeat ─────────────────────────────────────────────────
            modelBuilder.Entity<LabSeat>()
                .HasOne(s => s.Lab)
                .WithMany(l => l.LabSeats)
                .HasForeignKey(s => s.LabId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── LabReservation → LabSeat ─────────────────────────────────────
            modelBuilder.Entity<LabReservation>()
                .HasOne(r => r.Seat)
                .WithMany(s => s.LabReservations)
                .HasForeignKey(r => r.SeatId)
                .OnDelete(DeleteBehavior.Restrict);



            // ── Module 9 seed data ────────────────────────────────────────────────
            modelBuilder.Entity<Faculty>().HasData(
                new Faculty { FacultyId = 1, Name = "Computing", Code = "COMP", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Faculty { FacultyId = 2, Name = "Engineering", Code = "ENG", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Faculty { FacultyId = 3, Name = "Business", Code = "BUS", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Faculty { FacultyId = 4, Name = "Science", Code = "SCI", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Faculty { FacultyId = 5, Name = "Arts", Code = "ARTS", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<CertificateType>().HasData(
                new CertificateType { CertificateTypeId = 1, Name = "Bonafide Certificate", Description = "Confirms student enrolment status", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new CertificateType { CertificateTypeId = 2, Name = "Transcript", Description = "Academic transcript of results", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new CertificateType { CertificateTypeId = 3, Name = "Completion Letter", Description = "Confirms programme completion", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<Lab>().HasData(
                new Lab { LabId = 1, Name = "Advanced Computing Lab", RoomNumber = "L-301", IsActive = true, LabType = LabType.Computer, Capacity = 30 },
                new Lab { LabId = 2, Name = "Electronics & IoT Lab", RoomNumber = "L-102", IsActive = true, LabType = LabType.Computer, Capacity = 20 },
                new Lab { LabId = 3, Name = "Chemistry Research Lab", RoomNumber = "L-204", IsActive = true, LabType = LabType.Science, Capacity = 15 }
            );
        }
    }
}
