using System.ComponentModel.DataAnnotations;

namespace CampusServicesPortal.Hostel.DTOs
{
    public class AssignHostelRoomDto
    {
        [Range(1, int.MaxValue)]
        public int RoomId { get; set; }
    }
}