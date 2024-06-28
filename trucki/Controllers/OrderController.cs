using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OrderController: ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpPost("CreateOrder")]
    [Authorize(Roles = "admin,manager,field officer")]
    public async Task<ActionResult<ApiResponseModel<string>>> CreateNewOrder([FromBody] CreateOrderRequestModel model)
    {
        //var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        //string managerId = await _orderService.GetManagerIdAsync(userId);

        var response = await _orderService.CreateNewOrder(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("UpdateOrder")]
    [Authorize(Roles = "admin,manager,field officer")]
    public async Task<ActionResult<ApiResponseModel<string>>> EditOrder([FromBody] EditOrderRequestModel model)
    {
        var response = await _orderService.EditOrder(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("AssignTruckToOrder")]
    [Authorize(Roles = "admin,manager,field officer")]
    public async Task<ActionResult<ApiResponseModel<string>>> AssignTruckToOrders([FromBody] AssignTruckRequestModel model)
    {
        var response = await _orderService.AssignTruckToOrder(model);
        return response;
    }
    [HttpGet("GetAllOrders")]
    [Authorize(Roles = "admin,manager,field officer,finance")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOrderResponseModel>>>> GetAllOrders()
    {
        var response = await _orderService.GetAllOrders();
        return StatusCode(response.StatusCode, response);   
    }
    [HttpGet("GetOrderById")]
    [Authorize(Roles = "admin,manager,finance")]
    public async Task<ActionResult<ApiResponseModel<OrderResponseModel>>> GetOrderById(string orderId)
    {
        var response = await _orderService.GetOrderById(orderId);
        return StatusCode(response.StatusCode, response);
    }
     
    [HttpPost("UploadOrderDocuments")]
    [Authorize(Roles = "manager,field officer")]
    public async Task<ActionResult<ApiResponseModel<List<RouteResponseModel>>>> UploadOrderDocuments([FromForm]UploadOrderManifestRequestModel model)
    {
        var response = await _orderService.UploadOrderManifest(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetOrdersByStatus")]
    [Authorize(Roles = "admin,manager,field officer,finance")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOrderResponseModel>>>> GetOrdersByStatus([FromQuery] OrderStatus status)
    {
        var response = await _orderService.GetOrdersByStatus(status);
        return StatusCode(response.StatusCode, response);   
    }
    [HttpPost("uploadDeliveryManifest")]
    [Authorize(Roles = "manager,field officer")]
    public async Task<ActionResult<ApiResponseModel<List<RouteResponseModel>>>> UploadDeliveryManifest([FromForm]UploadOrderManifestRequestModel model)
    {
        var response = await _orderService.UploadDeliveryManifest(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("pay40Percent")]
    [Authorize(Roles = "finance")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOrderResponseModel>>>> Pay40Percent([FromBody] string status)
    {
        var response = await _orderService.Pay40Percent(status);
        return StatusCode(response.StatusCode, response);   
    }
    [HttpPost("pay60Percent")]
    [Authorize(Roles = "finance")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOrderResponseModel>>>> Pay60Percent([FromBody] string status)
    {
        var response = await _orderService.Pay60Percent(status);
        return StatusCode(response.StatusCode, response);   
    }
}