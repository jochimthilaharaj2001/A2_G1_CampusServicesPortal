using System.Security.Claims;
using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Events;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CampusServicePortal.Controllers;
[Route("api/event-registrations")][ApiController][Authorize]
public class EventRegistrationsController(IEventService service, ApplicationDbContext context) : ControllerBase { [HttpPost] public async Task<IActionResult> Create(CreateEventRegistrationDto dto){try{return Ok(await service.RegisterAsync(CurrentUserId(),dto));}catch(KeyNotFoundException e){return NotFound(new{message=e.Message});}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}} [HttpGet("student/{studentId:int}")] public async Task<IActionResult> GetByStudent(int studentId){if(!User.IsInRole("Admin")&&studentId!=await CurrentStudentIdAsync())return Forbid();var userId=await context.Students.Where(s=>s.StudentId==studentId).Select(s=>(int?)s.UserId).FirstOrDefaultAsync();return userId is null?NotFound(new{message="Student not found."}):Ok(await service.GetRegistrationsAsync(userId.Value));} [HttpDelete("{id:int}")] public async Task<IActionResult> Cancel(int id){try{await service.CancelRegistrationAsync(id,CurrentUserId(),User.IsInRole("Admin"));return Ok(new{message="Event registration cancelled."});}catch(KeyNotFoundException e){return NotFound(new{message=e.Message});}catch(UnauthorizedAccessException){return Forbid();}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}} private int CurrentUserId()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier)??User.FindFirstValue("sub"),out var id)?id:0;private Task<int> CurrentStudentIdAsync()=>context.Students.Where(s=>s.UserId==CurrentUserId()).Select(s=>s.StudentId).FirstOrDefaultAsync(); }
