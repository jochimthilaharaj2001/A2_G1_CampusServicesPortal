using CampusServicePortal.DTOs.MasterData;
using CampusServicePortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusServicePortal.Controllers
{
    [Route("api/certificate-types")]
    [ApiController]
    public class CertificateTypesController : ControllerBase
    {
        private readonly ICertificateTypeService _service;

        public CertificateTypesController(ICertificateTypeService service)
        {
            _service = service;
        }

        // GET: api/certificate-types
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            var items = await _service.GetAllAsync(activeOnly);
            return Ok(items);
        }

        // GET: api/certificate-types/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Certificate type not found." });
            return Ok(item);
        }

        // POST: api/certificate-types
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCertificateTypeDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.CertificateTypeId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/certificate-types/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCertificateTypeDto dto)
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

        // DELETE: api/certificate-types/{id} — soft deactivate
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeactivateAsync(id);
                return Ok(new { message = "Certificate type deactivated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
