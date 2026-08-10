using Microsoft.AspNetCore.Mvc;
using CampusServicesPortal.Hostel.DTOs;
using CampusServicesPortal.Hostel.Services;
using RoomModel = CampusServicesPortal.Hostel.Models.Room;

namespace CampusServicesPortal.Hostel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomModel>>> GetRooms()
        {
            var rooms = await _roomService.GetAllAsync();
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomModel>> GetRoom(int id)
        {
            var room = await _roomService.GetByIdAsync(id);

            if (room == null)
                return NotFound();

            return Ok(room);
        }

        [HttpPost]
        public async Task<ActionResult<RoomModel>> CreateRoom(CreateRoomDto dto)
        {
            try
            {
                var room = await _roomService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetRoom), new { id = room.RoomId }, room);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoomModel>> UpdateRoom(int id, UpdateRoomDto dto)
        {
            try
            {
                var room = await _roomService.UpdateAsync(id, dto);

                if (room == null)
                    return NotFound();

                return Ok(room);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var deleted = await _roomService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}