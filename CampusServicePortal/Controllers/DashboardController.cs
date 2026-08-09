using System.Security.Claims;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize]
public class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet("student")] public async Task<IActionResult> Student() => Ok(await service.GetStudentDashboardAsync(CurrentUserId()));
    [HttpGet("admin"), Authorize(Roles = "Admin")] public async Task<IActionResult> Admin() => Ok(await service.GetAdminDashboardAsync());
    private int CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id) ? id : 0;
}
