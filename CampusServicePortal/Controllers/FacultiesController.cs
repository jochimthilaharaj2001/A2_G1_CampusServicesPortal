using CampusServicePortal.DTOs.MasterData;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers
{
    [Route("api/faculties")]
    [ApiController]
    public class FacultiesController : ControllerBase
    {
        private readonly IFacultyService _service;

        public FacultiesController(IFacultyService service)
        {
            _service = service;
        }

        // GET: api/faculties
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            var items = await _service.GetAllAsync(activeOnly);
            return Ok(items);
        }

        // GET: api/faculties/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Faculty not found." });
            return Ok(item);
        }

        // POST: api/faculties
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateFacultyDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.FacultyId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/faculties/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFacultyDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/faculties/{id} — soft deactivate
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeactivateAsync(id);
                return Ok(new { message = "Faculty deactivated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
