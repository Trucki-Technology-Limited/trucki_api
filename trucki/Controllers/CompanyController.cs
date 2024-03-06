using Microsoft.AspNetCore.Mvc;
using trucki.DTOs;
using trucki.Interfaces.IServices;

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


    }
}
