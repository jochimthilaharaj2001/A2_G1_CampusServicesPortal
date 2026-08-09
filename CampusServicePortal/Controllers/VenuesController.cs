using CampusServicePortal.DTOs.Events;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampusServicePortal.Controllers;
[Route("api/venues")][ApiController][Authorize]
public class VenuesController(IEventService service) : ControllerBase { [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool includeInactive=false)=>Ok(await service.GetVenuesAsync(User.IsInRole("Admin")&&includeInactive)); [HttpPost,Authorize(Roles="Admin")] public async Task<IActionResult> Create(SaveVenueDto dto){try{return Ok(await service.SaveVenueAsync(null,dto));}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}} [HttpPut("{id:int}"),Authorize(Roles="Admin")] public async Task<IActionResult> Update(int id,SaveVenueDto dto){try{return Ok(await service.SaveVenueAsync(id,dto));}catch(KeyNotFoundException e){return NotFound(new{message=e.Message});}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}} [HttpDelete("{id:int}"),Authorize(Roles="Admin")] public async Task<IActionResult> Delete(int id){try{await service.DeactivateVenueAsync(id);return Ok(new{message="Venue deactivated."});}catch(KeyNotFoundException e){return NotFound(new{message=e.Message});}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}} }
