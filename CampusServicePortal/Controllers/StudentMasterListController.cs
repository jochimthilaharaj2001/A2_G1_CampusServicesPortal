using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentMasterListController : ControllerBase
    {
        private readonly IStudentMasterListService _masterListService;

        public StudentMasterListController(IStudentMasterListService masterListService)
        {
            _masterListService = masterListService;
        }

        // GET: api/student-master/{indexNumber}
        // Public endpoint called during registration step to verify an index number
        [HttpGet("/api/student-master/{indexNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyIndexNumber(string indexNumber)
        {
            try
            {
                var record = await _masterListService.VerifyIndexNumberAsync(indexNumber);
                return Ok(record);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/student-master?search=
        // Admin search of master list
        [HttpGet("/api/student-master")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchMasterList([FromQuery] string? search)
        {
            try
            {
                var records = await _masterListService.SearchMasterListAsync(search);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/student-master/import
        // Admin CSV bulk import
        [HttpPost("/api/student-master/import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportMasterList(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please upload a non-empty CSV file." });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "File must be a .csv file." });

            try
            {
                using var stream = file.OpenReadStream();
                var count = await _masterListService.ImportFromCsvAsync(stream);
                return Ok(new { message = $"Successfully imported {count} student master records." });
            }
            catch (FormatException ex)
            {
                return BadRequest(new { message = $"CSV Format Error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Import failed: {ex.Message}" });
            }
        }
    }
}
