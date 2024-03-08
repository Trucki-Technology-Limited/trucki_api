using Microsoft.AspNetCore.Mvc;
using System.Net;
using trucki.DTOs;
using trucki.Interfaces.IServices;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;
        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
            
        }

        [HttpPost("CreateNewManager")]
        [ProducesResponseType(typeof(GenericResponse<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateNewManager([FromBody] CreateManagerDto model)
        {
            var result = await _managerService.CreateTruckiManagerAsync(model);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut("UpdateNewManager")]
        [ProducesResponseType(typeof(GenericResponse<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateNewManager([FromBody] CreateManagerDto model)
        {
            var result = await _managerService.UpdateTruckiManagerAsync(model);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        [HttpGet("FetchAllTruckiManagers")]
        [ProducesResponseType(typeof(GenericResponse<IEnumerable<ManagerResponseDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchAllTruckiManagers([FromQuery] ManagerParameter companyParameter)
        {
            var result = await _managerService.FetchAllTruckiManagersAsync(companyParameter);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        [HttpGet("FetchTruckiManagerById")]
        [ProducesResponseType(typeof(GenericResponse<IEnumerable<ManagerResponseDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchTruckiManagerById([FromQuery] string companyId)
        {
            var result = await _managerService.FetchTruckiManagerAsync(companyId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

    }
}
