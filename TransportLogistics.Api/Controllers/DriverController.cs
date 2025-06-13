using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Всі методи в цьому контролері вимагають аутентифікації
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DriverDto>>> GetAll()
        {
            var drivers = await _driverService.GetAllDriversAsync();
            return Ok(drivers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DriverDto>> GetById(Guid id)
        {
            var driver = await _driverService.GetDriverByIdAsync(id);
            if (driver == null)
            {
                return NotFound();
            }
            return Ok(driver);
        }

        [HttpPost]
        public async Task<ActionResult<DriverDto>> Create([FromBody] CreateDriverRequest request)
        {
            try
            {
                var driver = await _driverService.CreateDriverAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = driver.Id }, driver);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DriverDto>> Update(Guid id, [FromBody] UpdateDriverRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID in URL does not match ID in request body.");
            }

            var updatedDriver = await _driverService.UpdateDriverAsync(request);
            if (updatedDriver == null)
            {
                return NotFound();
            }
            return Ok(updatedDriver);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _driverService.DeleteDriverAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent(); // 204 No Content - успішно видалено
        }
    }
}