using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using trucki.DTOs;
using trucki.Interfaces.IServices;
using trucki.Services;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
            
        }

        [HttpPost("CreateTruckiBusiness")]
        [ProducesResponseType(typeof(GenericResponse<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateTruckiBusiness([FromBody] CreateBusinessDto request)
        {

            var result = await _businessService.CreateTruckiBusinessAsync(request);    
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut("UpdateTruckiBusiness")]
        [ProducesResponseType(typeof(GenericResponse<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateTruckiBusiness([FromBody] CreateBusinessDto request)
        {

            var result = await _businessService.UpdateTruckiBusinessAsync(request);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpDelete("DeleteTruckiBusiness")]
        [ProducesResponseType(typeof(GenericResponse<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteTruckiBusiness([FromQuery] string businessId)
        {

            var result = await _businessService.DeleteTruckiBusinessAsync(businessId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("FetchTruckiBusinessById")]
        [ProducesResponseType(typeof(GenericResponse<BusinessResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchTruckiBusiness([FromQuery] string businessId)
        {

            var result = await _businessService.FetchTruckiBusinessAsync(businessId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("FetchAllTruckiBusinesses")]
        [ProducesResponseType(typeof(GenericResponse<IEnumerable<BusinessResponse>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchAllTruckiBusinesses([FromQuery] BusinessParameter businessParameter)
        {
            var result = await _businessService.FetchAllTruckiBusinessesAsync(businessParameter);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

    }
}
