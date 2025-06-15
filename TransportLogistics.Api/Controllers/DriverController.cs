// TransportLogistics.Api/Controllers/DriverController.cs
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
    [Authorize] // Застосовуємо авторизацію до всього контролера
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        // GET: api/Driver
        [HttpGet]
        [ProducesResponseType(typeof(List<DriverDto>), 200)]
        public async Task<ActionResult<List<DriverDto>>> GetAllDrivers()
        {
            var drivers = await _driverService.GetAllDriversAsync();
            return Ok(drivers);
        }

        // GET: api/Driver/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DriverDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DriverDto>> GetDriverById(Guid id)
        {
            var driver = await _driverService.GetDriverByIdAsync(id);
            if (driver == null)
            {
                return NotFound($"Driver with ID {id} not found.");
            }
            return Ok(driver);
        }

        // POST: api/Driver
        [HttpPost]
        [ProducesResponseType(typeof(DriverDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<DriverDto>> CreateDriver([FromBody] CreateDriverRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

        // PUT: api/Driver/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DriverDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DriverDto>> UpdateDriver(Guid id, [FromBody] UpdateDriverRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // ПОМИЛКА БУЛА ТУТ - ТРЕБА ПЕРЕДАТИ ОБ'ЄКТ request
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

        // DELETE: api/Driver/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteDriver(Guid id)
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