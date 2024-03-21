using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using trucki.DTOs;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;
        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
            
        }

        [HttpPost("CreateNewManager")]
        [ProducesResponseType(typeof(ApiResponseModel<string>), (int)HttpStatusCode.OK)]
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
        [ProducesResponseType(typeof(ApiResponseModel<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateNewManager([FromBody] UpdateManagerDto model)
        {
            var result = await _managerService.UpdateTruckiManagerAsync(model);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        [HttpGet("FetchAllTruckiManagers")]
        [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<ManagerResponseDto>>), (int)HttpStatusCode.OK)]
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
        [ProducesResponseType(typeof(ApiResponseModel<ManagerResponseDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchTruckiManagerById([FromQuery] string managerId)
        {
            var result = await _managerService.FetchTruckiManagerAsync(managerId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

    }
}
