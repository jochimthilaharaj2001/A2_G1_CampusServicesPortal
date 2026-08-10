using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<ActionResult<IEnumerable<RoomModel>>> GetRooms() =>
            Ok(await _roomService.GetAllAsync());

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<RoomModel>> GetRoom(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            return room is null ? NotFound() : Ok(room);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomModel>> UpdateRoom(int id, UpdateRoomDto dto)
        {
            try
            {
                var room = await _roomService.UpdateAsync(id, dto);
                return room is null ? NotFound() : Ok(room);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                return await _roomService.DeleteAsync(id) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}