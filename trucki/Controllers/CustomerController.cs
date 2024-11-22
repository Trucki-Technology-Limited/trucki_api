using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{

    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }


    [HttpPost("AddNewCustomer")]
    [Authorize(Roles = "admin,manager,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<string>>> AddNewCustomer([FromBody] AddCustomerRequestModel model)
    {
        var response = await _customerService.AddNewCustomer(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("EditCustomer")]
    [Authorize(Roles = "admin,manager,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<string>>> EditCustomer([FromBody] EditCustomerRequestModel model)
    {
        var response = await _customerService.EditCustomer(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetCustomerById")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<AllCustomerResponseModel>>> GetCustomerById(string customerId)
    {
        var response = await _customerService.GetCustomerById(customerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetAllCustomers")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<List<AllCustomerResponseModel>>>> GetAllCustomers()
    {
        var response = await _customerService.GetAllCustomers();
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("RemoveCustomer")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<string>>> DeleteCustomer(string customerId)
    {
        var response = await _customerService.DeleteCustomer(customerId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("SearchCustomers")]
    [Authorize(Roles = "admin,manager,chiefmanager")]
    public async Task<ActionResult<IEnumerable<AllCustomerResponseModel>>> SearchCustomers(string searchWords)
    {
        var response = await _customerService.SearchCustomers(searchWords);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetCustomersByBusinessId")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<List<AllCustomerResponseModel>>>> GetCustomersByBusinessId(string businessId)
    {
        var response = await _customerService.GetCustomersByBusinessId(businessId);
        return StatusCode(response.StatusCode, response);
    }

}