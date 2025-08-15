using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("CreateOrder")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<string>>> CreateNewOrder([FromBody] CreateOrderRequestModel model)
    {
        //var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        //string managerId = await _orderService.GetManagerIdAsync(userId);

        var response = await _orderService.CreateNewOrder(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("UpdateOrder")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<string>>> EditOrder([FromBody] EditOrderRequestModel model)
    {
        var response = await _orderService.EditOrder(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("AssignTruckToOrder")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<string>>> AssignTruckToOrders([FromBody] AssignTruckRequestModel model)
    {
        var response = await _orderService.AssignTruckToOrder(model);
        return response;
    }
    [HttpGet("GetAllOrders")]
    [Authorize(Roles = "admin,manager,field officer,finance,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<PaginatedListDto<AllOrderResponseModel>>>> GetAllOrders(
        [FromQuery] GetAllOrdersRequestModel request)
    {
        try
        {
            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                return BadRequest(new ApiResponseModel<PaginatedListDto<AllOrderResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "Page number must be greater than 0",
                    StatusCode = 400
                });
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                return BadRequest(new ApiResponseModel<PaginatedListDto<AllOrderResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "Page size must be between 1 and 100",
                    StatusCode = 400
                });
            }

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Validate and normalize request parameters
            request.ValidateAndNormalize();

            var response = await _orderService.GetAllOrders(roles, userId, request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseModel<PaginatedListDto<AllOrderResponseModel>>
            {
                IsSuccessful = false,
                Message = $"An error occurred: {ex.Message}",
                StatusCode = 500
            });
        }
    }
    [HttpGet("GetOrderById")]
    [Authorize(Roles = "admin,manager,finance,field officer,driver,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<OrderResponseModel>>> GetOrderById(string orderId)
    {
        var response = await _orderService.GetOrderById(orderId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("UploadOrderDocuments")]
    [Authorize(Roles = "manager,field officer,chiefmanager,transporter,driver")]
    public async Task<ActionResult<ApiResponseModel<List<RouteResponseModel>>>> UploadOrderDocuments([FromBody] UploadOrderManifestRequestModel model)
    {
        var response = await _orderService.UploadOrderManifest(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetOrdersByStatus")]
    [Authorize(Roles = "admin,manager,field officer,finance,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOrderResponseModel>>>> GetOrdersByStatus([FromQuery] OrderStatus status)
    {
        var response = await _orderService.GetOrdersByStatus(status);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("uploadDeliveryManifest")]
    [Authorize(Roles = "manager,field officer,chiefmanager,transporter,driver")]
    public async Task<ActionResult<ApiResponseModel<List<RouteResponseModel>>>> UploadDeliveryManifest([FromBody] UploadOrderManifestRequestModel model)
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
    [HttpGet("GetOrderByIdForMobile")]
    [Authorize(Roles = "driver,transporter")]
    public async Task<ActionResult<ApiResponseModel<OrderResponseModelForMobile>>> GetOrderByIdForMobile(string orderId)
    {
        var response = await _orderService.GetOrderByIdForMobile(orderId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("AcceptOrderRequest")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AcceptOrderRequest([FromBody] AcceptOrderRequestModel model)
    {
        var response = await _orderService.AcceptOrderRequest(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("SearchOrders")]
    [Authorize(Roles = "manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOrderResponseModel>>>> SearchOrders([FromBody] SearchOrderRequestModel model)
    {
        var response = await _orderService.SearchOrders(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetPendingOrders")]
    [Authorize()]
    public async Task<ActionResult<ApiResponseModel<List<AllOrderResponseModel>>>> GetPendingOrders()
    {
        var response = await _orderService.GetPendingOrders();
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("AssignOrderToTruckAsTransporter")]
    [Authorize(Roles = "transporter")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AssignOrderToTruckAsTransporter([FromBody] AssignOrderToTruckAsTransporter model)
    {
        var response = await _orderService.AssignOrderToTruckAsTransporter(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTransporterActiveOrdersAsync")]
    [Authorize(Roles = "transporter")]
    public async Task<ActionResult<ApiResponseModel<List<AllOrderResponseModel>>>> GetTransporterActiveOrdersAsync(string ownerId)
    {
        var response = await _orderService.GetTransporterOrdersAsync(ownerId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTransporterCompletedOrdersAsync")]
    [Authorize(Roles = "transporter")]
    public async Task<ActionResult<ApiResponseModel<List<AllOrderResponseModel>>>> GetTransporterCompletedOrdersAsync(string ownerId)
    {
        var response = await _orderService.GetTransporterCompletedOrdersAsync(ownerId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetDriverOrdersAsync")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<List<AllOrderResponseModel>>>> GetDriverOrdersAsync(string driverId)
    {
        var response = await _orderService.GetDriverOrdersAsync(driverId);
        return StatusCode(response.StatusCode, response);
    }
}