// TransportLogistics.Api/Controllers/DriverController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.DTOs;
using TransportLogistics.Api.DTOs.QueryParams; // Додано для DriverQueryParams
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Controllers
{
    [Authorize] // Додано авторизацію для всього контролера
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        // Оновлено метод для прийому DriverQueryParams
        [HttpGet]
        [ProducesResponseType(typeof(List<DriverDto>), 200)]
        public async Task<IActionResult> GetAllDrivers([FromQuery] DriverQueryParams queryParams)
        {
            var drivers = await _driverService.GetAllDriversAsync(queryParams);
            return Ok(drivers);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DriverDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDriverById(Guid id)
        {
            var driver = await _driverService.GetDriverByIdAsync(id);
            if (driver == null)
            {
                return NotFound($"Driver with ID {id} not found.");
            }
            return Ok(driver);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DriverDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverRequest request)
        {
            try
            {
                var newDriver = await _driverService.CreateDriverAsync(request);
                return CreatedAtAction(nameof(GetDriverById), new { id = newDriver.Id }, newDriver);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DriverDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] UpdateDriverRequest request)
        {
            try
            {
                var updatedDriver = await _driverService.UpdateDriverAsync(id, request);
                if (updatedDriver == null)
                {
                    return NotFound($"Driver with ID {id} not found.");
                }
                return Ok(updatedDriver);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteDriver(Guid id)
        {
            var deleted = await _driverService.DeleteDriverAsync(id);
            if (!deleted)
            {
                return NotFound($"Driver with ID {id} not found.");
            }
            return NoContent(); // 204 No Content for successful deletion
        }
    }
}
