using Microsoft.AspNetCore.Mvc;
using System.Net;
using trucki.DTOs;
using trucki.Interfaces.IServices;
using trucki.Services;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyServices _companyService;
        public CompanyController(ICompanyServices companyServices)
        {
            _companyService = companyServices;
            
        }

        [HttpPost("CreateNewCompany")]
        public async Task<IActionResult> CreateNewCompany([FromBody] CreateCompanyDto model)
        {
            var result = await _companyService.CreateTruckiComapnyAsync(model);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut("UpdateNewCompany")]
        public async Task<IActionResult> UpdateNewCompany([FromBody] CreateCompanyDto model)
        {
            var result = await _companyService.UpdateTruckiComapnyAsync(model);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        [HttpGet("FetchAllTruckiCompanies")]
        [ProducesResponseType(typeof(GenericResponse<IEnumerable<CompanyResponseDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchAllTruckiCompanies([FromQuery] CompanyParameter companyParameter)
        {
            var result = await _companyService.FetchAllTruckiDriversAsync(companyParameter);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        [HttpGet("FetchTruckiCompanyById")]
        [ProducesResponseType(typeof(GenericResponse<IEnumerable<CompanyResponseDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchTruckiCompanyById([FromQuery] string companyId)
        {
            var result = await _companyService.FetchTruckiDriverAsync(companyId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }


    }
}
