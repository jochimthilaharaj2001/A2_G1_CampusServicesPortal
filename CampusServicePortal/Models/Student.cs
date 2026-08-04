using System.ComponentModel.DataAnnotations;
namespace CampusServicePortal.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        public int UserId { get; set; }

        public string StudentNumber { get; set; }
            = string.Empty;

        public string Faculty { get; set; }
            = string.Empty;

        public string DegreeProgram { get; set; }
            = string.Empty;

        public int EnrollmentYear { get; set; }

        public User? User { get; set; }
    }
}
