using CampusServicePortal.Models;
using CampusServicesPortal.Hostel.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ============================================================
        // DATABASE TABLES
        // ============================================================

        // Users / Authentication
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        // Student Module
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentMasterList> StudentMasterList { get; set; }
        public DbSet<Faculty> Faculties { get; set; }

        // Certificate Module
        public DbSet<CertificateType> CertificateTypes { get; set; }

        // Lab Module
        public DbSet<Lab> Labs { get; set; }
        public DbSet<LabSeat> LabSeats { get; set; }
        public DbSet<LabReservation> LabReservations { get; set; }
        public DbSet<ComplaintCategory> ComplaintCategories { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<FeeType> FeeTypes { get; set; }
        public DbSet<FeePayment> FeePayments { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<CampusEvent> Events { get; set; }
        public DbSet<EventSeat> EventSeats { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<CertificateRequest> CertificateRequests { get; set; }

        // Hostel Module
        public DbSet<Hostel> Hostels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<HostelApplication> HostelApplications { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        // ============================================================
        // MODEL CONFIGURATION
        // ============================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ========================================================
            // USER
            // ========================================================

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            // ========================================================
            // STUDENT
            // ========================================================

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


            // ========================================================
            // REFRESH TOKEN
            // ========================================================

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);


            // ========================================================
            // PASSWORD RESET TOKEN
            // ========================================================

            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(prt => prt.UserId);


            // ========================================================
            // STUDENT MASTER LIST
            // ========================================================

            modelBuilder.Entity<StudentMasterList>()
                .HasIndex(sml => sml.IndexNumber)
                .IsUnique();


            // ========================================================
            // FACULTY
            // ========================================================

            modelBuilder.Entity<Faculty>()
                .HasIndex(f => f.Name)
                .IsUnique();


            // ========================================================
            // CERTIFICATE TYPE
            // ========================================================

            modelBuilder.Entity<CertificateType>()
                .HasIndex(c => c.Name)
                .IsUnique();


            // ========================================================
            // ROLE SEED DATA
            // ========================================================

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = 1,
                    RoleName = "Admin"
                },
                new Role
                {
                    RoleId = 2,
                    RoleName = "Student"
                }
            );


            // ========================================================
            // LAB RESERVATION → USER
            // ========================================================

            modelBuilder.Entity<LabReservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.LabReservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================================
            // LAB RESERVATION → LAB
            // ========================================================

            modelBuilder.Entity<LabReservation>()
                .HasOne(r => r.Lab)
                .WithMany()
                .HasForeignKey(r => r.LabId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================================
            // COMPLAINT MODULE
            // ========================================================

            modelBuilder.Entity<ComplaintCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Category)
                .WithMany(c => c.Complaints)
                .HasForeignKey(c => c.ComplaintCategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================================
            // FEE MODULE
            // ========================================================

            modelBuilder.Entity<FeeType>()
                .HasIndex(f => f.Name)
                .IsUnique();

            modelBuilder.Entity<FeePayment>()
                .HasOne(f => f.User)
                .WithMany(u => u.FeePayments)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeePayment>()
                .HasOne(f => f.FeeType)
                .WithMany(t => t.FeePayments)
                .HasForeignKey(f => f.FeeTypeId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================================
            // EVENT MODULE
            // ========================================================

            modelBuilder.Entity<Venue>()
                .HasIndex(v => v.Name)
                .IsUnique();

            modelBuilder.Entity<CampusEvent>()
                .HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventSeat>()
                .HasOne(s => s.Event)
                .WithMany(e => e.EventSeats)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventSeat>()
                .HasIndex(s => new { s.EventId, s.SeatNumber })
                .IsUnique();

            modelBuilder.Entity<EventRegistration>()
                .HasOne(r => r.Event)
                .WithMany(e => e.EventRegistrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventRegistration>()
                .HasOne(r => r.User)
                .WithMany(u => u.EventRegistrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventRegistration>()
                .HasOne(r => r.Seat)
                .WithMany(s => s.Registrations)
                .HasForeignKey(r => r.EventSeatId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================================
            // CERTIFICATE REQUEST
            // ========================================================

            modelBuilder.Entity<CertificateRequest>()
                .HasOne(r => r.User)
                .WithMany(u => u.CertificateRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CertificateRequest>()
                .HasOne(r => r.CertificateType)
                .WithMany(t => t.CertificateRequests)
                .HasForeignKey(r => r.CertificateTypeId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================================
            // LAB → LAB SEAT
            // ========================================================

            modelBuilder.Entity<LabSeat>()
                .HasOne(s => s.Lab)
                .WithMany(l => l.LabSeats)
                .HasForeignKey(s => s.LabId)
                .OnDelete(DeleteBehavior.Cascade);


            // ========================================================
            // LAB RESERVATION → LAB SEAT
            // ========================================================

            modelBuilder.Entity<LabReservation>()
                .HasOne(r => r.Seat)
                .WithMany(s => s.LabReservations)
                .HasForeignKey(r => r.SeatId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================================
            // MODULE 9 — FACULTY SEED DATA
            // ========================================================

            modelBuilder.Entity<Faculty>().HasData(
                new Faculty
                {
                    FacultyId = 1,
                    Name = "Computing",
                    Code = "COMP",
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                },

                new Faculty
                {
                    FacultyId = 2,
                    Name = "Engineering",
                    Code = "ENG",
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                },

                new Faculty
                {
                    FacultyId = 3,
                    Name = "Business",
                    Code = "BUS",
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                },

                new Faculty
                {
                    FacultyId = 4,
                    Name = "Science",
                    Code = "SCI",
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                },

                new Faculty
                {
                    FacultyId = 5,
                    Name = "Arts",
                    Code = "ARTS",
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                }
            );


            // ========================================================
            // CERTIFICATE TYPE SEED DATA
            // ========================================================

            modelBuilder.Entity<CertificateType>().HasData(
                new CertificateType
                {
                    CertificateTypeId = 1,
                    Name = "Bonafide Certificate",
                    Description = "Confirms student enrolment status",
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                },

                new CertificateType
                {
                    CertificateTypeId = 2,
                    Name = "Transcript",
                    Description = "Academic transcript of results",
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                },

                new CertificateType
                {
                    CertificateTypeId = 3,
                    Name = "Completion Letter",
                    Description = "Confirms programme completion",
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc)
                }
            );


            // ========================================================
            // LAB SEED DATA
            // ========================================================

            modelBuilder.Entity<Lab>().HasData(
                new Lab
                {
                    LabId = 1,
                    Name = "Advanced Computing Lab",
                    RoomNumber = "L-301",
                    IsActive = true,
                    LabType = LabType.Computer,
                    Capacity = 30
                },

                new Lab
                {
                    LabId = 2,
                    Name = "Electronics & IoT Lab",
                    RoomNumber = "L-102",
                    IsActive = true,
                    LabType = LabType.Computer,
                    Capacity = 20
                },

                new Lab
                {
                    LabId = 3,
                    Name = "Chemistry Research Lab",
                    RoomNumber = "L-204",
                    IsActive = true,
                    LabType = LabType.Science,
                    Capacity = 15
                }
            );
        }
    }
}