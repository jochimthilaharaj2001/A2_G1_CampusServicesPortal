using CampusServicePortal.DTOs.Complaints;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers;

[Route("api/complaint-categories")]
[ApiController]
[Authorize]
public class ComplaintCategoriesController(IComplaintService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false) => Ok(await service.GetCategoriesAsync(User.IsInRole("Admin") && includeInactive));
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(SaveComplaintCategoryDto dto) { try { var category = await service.SaveCategoryAsync(null, dto); return CreatedAtAction(nameof(GetAll), category); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, SaveComplaintCategoryDto dto) { try { return Ok(await service.SaveCategoryAsync(id, dto)); } catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) { try { await service.DeactivateCategoryAsync(id); return Ok(new { message = "Complaint category deactivated successfully." }); } catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); } }
}
