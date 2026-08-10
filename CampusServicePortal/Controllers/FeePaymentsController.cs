using System.Security.Claims;
using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Fees;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Controllers;

[Route("api/fee-payments")]
[ApiController]
[Authorize]
public class FeePaymentsController(IFeePaymentService service, ApplicationDbContext context) : ControllerBase
{
    [HttpGet("student/{studentId:int}")]
    public async Task<IActionResult> GetByStudent(int studentId) { if (!User.IsInRole("Admin") && studentId != await CurrentStudentIdAsync()) return Forbid(); var userId = await context.Students.Where(s => s.StudentId == studentId).Select(s => (int?)s.UserId).FirstOrDefaultAsync(); return userId is null ? NotFound(new { message = "Student not found." }) : Ok(await service.GetForUserAsync(userId.Value)); }
    [HttpGet, Authorize(Roles = "Admin")] public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int? feeTypeId) => Ok(await service.GetAllAsync(status, feeTypeId));
    [HttpPost("assign"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign(AssignFeeDto dto) { try { var count = await service.AssignAsync(dto); return Ok(new { message = $"Fee assigned to {count} student(s).", assignedCount = count }); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateFeeDto dto) { try { return Ok(await service.UpdateAsync(id, dto)); } catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(int id) { try { await service.CancelAsync(id); return Ok(new { message = "Fee assignment cancelled." }); } catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> Pay(int id) { try { return Ok(await service.PayAsync(id, CurrentUserId(), User.IsInRole("Admin"))); } catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); } catch (UnauthorizedAccessException) { return Forbid(); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpGet("{id:int}/receipt")]
    public async Task<IActionResult> Receipt(int id) { try { return Ok(await service.GetReceiptAsync(id, CurrentUserId(), User.IsInRole("Admin"))); } catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); } catch (UnauthorizedAccessException) { return Forbid(); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    private int CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id) ? id : 0;
    private Task<int> CurrentStudentIdAsync() => context.Students.Where(s => s.UserId == CurrentUserId()).Select(s => s.StudentId).FirstOrDefaultAsync();
}
