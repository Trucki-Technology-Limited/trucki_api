using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using trucki.DTOs;
using trucki.Interfaces.IServices;


namespace trucki.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService; 
        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
            
        }


        [HttpPost("CreateTruckiDriver")]
        [ProducesResponseType(typeof(GenericResponse<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateTruckiDriver([FromBody] CreateDriverDto request)
        {

            var result = await _driverService.CreateTruckiDriverAsync(request);
            if (result == null) 
            {
                return NotFound();  
            }
            return Ok(result);
        }


        [HttpPost("UpdateTruckiDriver")]
        [ProducesResponseType(typeof(GenericResponse<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateTruckiDriver([FromBody] CreateDriverDto request)
        {

            var result = await _driverService.UpdateTruckiDriverAsync(request);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("FetchTruckiDriver")]
        [ProducesResponseType(typeof(GenericResponse<DriverResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchTruckiDriver([FromQuery] string driverId)
        {

            var result = await _driverService.FetchTruckiDriverAsync(driverId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        [HttpGet("FetchAllTruckiDrivers")]
        [ProducesResponseType(typeof(GenericResponse<IEnumerable<DriversResponse>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchAllTruckiDrivers([FromQuery] DriverParameter driverParameter)
        {

            var result = await _driverService.FetchAllTruckiDriversAsync(driverParameter);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
