using CampusServicePortal.DTOs.Fees;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers;

[Route("api/fee-types")]
[ApiController]
[Authorize]
public class FeeTypesController(IFeePaymentService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false) => Ok(await service.GetFeeTypesAsync(User.IsInRole("Admin") && includeInactive));
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(SaveFeeTypeDto dto) { try { return Ok(await service.SaveFeeTypeAsync(null, dto)); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, SaveFeeTypeDto dto) { try { return Ok(await service.SaveFeeTypeAsync(id, dto)); } catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); } catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); } }
}
