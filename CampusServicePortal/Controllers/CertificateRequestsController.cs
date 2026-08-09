using System.Security.Claims;
using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Certificates;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CampusServicePortal.Controllers;
[Route("api/certificate-requests")][ApiController][Authorize]
public class CertificateRequestsController(ICertificateRequestService service, ApplicationDbContext context) : ControllerBase { [HttpPost] public async Task<IActionResult> Create(CreateCertificateRequestDto dto){try{return Ok(await service.CreateAsync(CurrentUserId(),dto));}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}} [HttpGet("student/{studentId:int}")] public async Task<IActionResult> GetByStudent(int studentId){if(!User.IsInRole("Admin")&&studentId!=await CurrentStudentIdAsync())return Forbid();var userId=await context.Students.Where(s=>s.StudentId==studentId).Select(s=>(int?)s.UserId).FirstOrDefaultAsync();return userId is null?NotFound(new{message="Student not found."}):Ok(await service.GetForUserAsync(userId.Value));} [HttpGet,Authorize(Roles="Admin")] public async Task<IActionResult> GetAll([FromQuery]string? status)=>Ok(await service.GetAllAsync(status)); [HttpPut("{id:int}/status"),Authorize(Roles="Admin")] public async Task<IActionResult> UpdateStatus(int id,UpdateCertificateRequestStatusDto dto){try{return Ok(await service.UpdateStatusAsync(id,dto));}catch(KeyNotFoundException e){return NotFound(new{message=e.Message});}} private int CurrentUserId()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier)??User.FindFirstValue("sub"),out var id)?id:0;private Task<int> CurrentStudentIdAsync()=>context.Students.Where(s=>s.UserId==CurrentUserId()).Select(s=>s.StudentId).FirstOrDefaultAsync(); }
