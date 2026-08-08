using CampusServicePortal.Models;

namespace CampusServicePortal.Data
{
    /// <summary>
    /// Idempotent development seed data for Module 1 admin access:
    /// - A default Administrator account (admin@university.edu / Admin@123)
    /// - Sample StudentMasterList rows so registration is testable
    /// - One demo student account (student1@university.edu / Student@123)
    /// </summary>
    public static class DatabaseSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            SeedAdminUser(db);
            SeedMasterList(db);
            SeedDemoStudent(db);
            SeedSeats(db);
        }

        private static void SeedSeats(ApplicationDbContext db)
        {
            if (db.LabSeats.Any()) return;

            // Seed seats for Lab 1 (Advanced Computing Lab) - capacity 30
            for (int i = 1; i <= 30; i++)
            {
                db.LabSeats.Add(new LabSeat
                {
                    LabId = 1,
                    SeatNumber = $"Seat {i}",
                    IsActive = true
                });
            }

            // Seed seats for Lab 2 (Electronics & IoT Lab) - capacity 20
            for (int i = 1; i <= 20; i++)
            {
                db.LabSeats.Add(new LabSeat
                {
                    LabId = 2,
                    SeatNumber = $"Seat {i}",
                    IsActive = true
                });
            }

            db.SaveChanges();
        }

        private static void SeedAdminUser(ApplicationDbContext db)
        {
            var admin = db.Users.FirstOrDefault(u => u.Email == "admin@university.edu");
            if (admin == null)
            {
                db.Users.Add(new User
                {
                    FullName = "System Administrator",
                    Email = "admin@university.edu",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    RoleId = 1, // Admin
                    IsActive = true,
                    EmailVerified = true,
                    CreatedDate = DateTime.UtcNow
                });
                db.SaveChanges();
            }
            else if (string.IsNullOrEmpty(admin.Username))
            {
                admin.Username = "admin";
                db.SaveChanges();
            }
        }

        private static void SeedMasterList(ApplicationDbContext db)
        {
            var samples = new[]
            {
                ("EG/2024/001", "Ashan Perera", "Engineering", "BSc Engineering", 2024),
                ("SC/2024/045", "Dilani Silva", "Science", "BSc Computer Science", 2024),
                ("AR/2024/012", "Kasun Fernando", "Arts", "BA Economics", 2024),
                ("CS/2024/033", "Nethmi Jayasinghe", "Computing", "BSc Computing", 2024),
                ("BU/2024/021", "Sahan Wickramasinghe", "Business", "BBA Management", 2024)
            };

            foreach (var (idx, name, faculty, degree, year) in samples)
            {
                if (db.StudentMasterList.Any(m => m.IndexNumber == idx)) continue;

                db.StudentMasterList.Add(new StudentMasterList
                {
                    IndexNumber = idx,
                    FullName = name,
                    Faculty = faculty,
                    DegreeProgram = degree,
                    EnrollmentYear = year,
                    IsRegistered = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            db.SaveChanges();
        }

        private static void SeedDemoStudent(ApplicationDbContext db)
        {
            const string indexNumber = "EG/2024/001";

            if (db.Students.Any(s => s.IndexNumber == indexNumber)) return;

            var user = db.Users.FirstOrDefault(u => u.Email == "student1@university.edu");
            if (user == null)
            {
                user = new User
                {
                    FullName = "Ashan Perera",
                    Email = "student1@university.edu",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    RoleId = 2, // Student
                    IsActive = true,
                    EmailVerified = true,
                    CreatedDate = DateTime.UtcNow
                };
                db.Users.Add(user);
                db.SaveChanges();
            }

            var faculty = db.Faculties.FirstOrDefault(f => f.Name == "Engineering");

            db.Students.Add(new Student
            {
                UserId = user.UserId,
                IndexNumber = indexNumber,
                FacultyId = faculty?.FacultyId,
                Faculty = faculty?.Name ?? "Engineering",
                DegreeProgram = "BSc Engineering",
                EnrollmentYear = 2024,
                IsActive = true
            });
            db.SaveChanges();

            var masterRow = db.StudentMasterList.FirstOrDefault(m => m.IndexNumber == indexNumber);
            if (masterRow != null && !masterRow.IsRegistered)
            {
                masterRow.IsRegistered = true;
                db.SaveChanges();
            }
        }
    }
}
