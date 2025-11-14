using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DispatcherController : ControllerBase
{
    private readonly ITruckOwnerService _truckOwnerService;
    private readonly ICargoOrderService _cargoOrderService;
    private readonly IDocumentTypeService _documentTypeService;

    public DispatcherController(ITruckOwnerService truckOwnerService, ICargoOrderService cargoOrderService, IDocumentTypeService documentTypeService)
    {
        _truckOwnerService = truckOwnerService;
        _cargoOrderService = cargoOrderService;
        _documentTypeService = documentTypeService;
    }

    // UNIFIED REGISTRATION ENDPOINT (handles both US Dispatcher and Nigeria Transporter)
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponseModel<bool>>> RegisterFleetOwner([FromBody] AddDispatcherRequestBody model)
    {
        var result = await _truckOwnerService.RegisterFleetOwner(model);
        return Ok(result);
    }

    // UNIFIED PROFILE ENDPOINT (works for both dispatcher and transporter)
    [HttpGet("profile")]
    [Authorize(Roles = "dispatcher,transporter,admin")]
    public async Task<ActionResult<ApiResponseModel<TruckOwnerResponseModel>>> GetFleetOwnerProfile()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var result = await _truckOwnerService.GetFleetOwnerProfile(userId);
        return Ok(result);
    }

    // Driver onboarding endpoints
    [HttpPost("{dispatcherId}/drivers/{driverId}/documents")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UploadDriverDocuments(string dispatcherId, string driverId, [FromBody] UploadDriverDocumentsForDispatcherDto model)
    {
        model.DispatcherId = dispatcherId; // Ensure dispatcher ID matches route
        model.DriverId = driverId; // Ensure driver ID matches route
        var result = await _truckOwnerService.UploadDriverDocuments(model);
        return Ok(result);
    }

    [HttpPost("{dispatcherId}/drivers/{driverId}/truck")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AddTruckForDriver(string dispatcherId, string driverId, [FromBody] AddTruckForDispatcherDriverDto model)
    {
        model.DispatcherId = dispatcherId; // Ensure dispatcher ID matches route
        model.DriverId = driverId; // Ensure driver ID matches route
        var result = await _truckOwnerService.AddTruckForDriver(model);
        return Ok(result);
    }

    [HttpPost("{dispatcherId}/drivers/{driverId}/complete-onboarding")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CompleteDriverOnboarding(string dispatcherId, string driverId, [FromBody] CompleteDriverOnboardingDto model)
    {
        model.DispatcherId = dispatcherId; // Ensure dispatcher ID matches route
        model.DriverId = driverId; // Ensure driver ID matches route
        var result = await _truckOwnerService.CompleteDriverOnboarding(model);
        return Ok(result);
    }

    // Driver management - Set or Update Commission
    [HttpPut("drivers/{driverId}/commission")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> SetOrUpdateCommission(string driverId, [FromBody] UpdateCommissionRequestModel model)
    {
        model.DriverId = driverId; // Ensure driver ID matches route
        var result = await _truckOwnerService.SetOrUpdateDriverCommission(model.DriverId, model.DispatcherId, model.NewCommissionPercentage);
        return Ok(result);
    }

    // Get commission history for a driver
    [HttpGet("drivers/{driverId}/commission")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<DriverCommissionHistoryResponseModel>>> GetDriverCommissionHistory(string driverId, [FromQuery] string dispatcherId)
    {
        var result = await _truckOwnerService.GetDriverCommissionHistory(driverId, dispatcherId);
        return Ok(result);
    }

    // Bidding
    [HttpPost("bids")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CreateBidOnBehalf([FromBody] CreateBidOnBehalfDto model)
    {
        var result = await _cargoOrderService.CreateBidOnBehalfAsync(model);
        return Ok(result);
    }

    [HttpGet("{dispatcherId}/orders/available")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>>> GetAvailableOrders(string dispatcherId)
    {
        var result = await _cargoOrderService.GetAvailableOrdersForDispatcherAsync(dispatcherId);
        return Ok(result);
    }

    // Order document management (similar to driver flow)
    [HttpPost("orders/{orderId}/manifest")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UploadManifestOnBehalf(string orderId, [FromBody] UploadManifestOnBehalfDto model)
    {
        model.OrderId = orderId; // Ensure order ID matches route
        var result = await _cargoOrderService.UploadManifestOnBehalfAsync(model);
        return Ok(result);
    }

    [HttpPost("orders/{orderId}/delivery")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CompleteDeliveryOnBehalf(string orderId, [FromBody] CompleteDeliveryOnBehalfDto model)
    {
        model.OrderId = orderId; // Ensure order ID matches route
        var result = await _cargoOrderService.CompleteDeliveryOnBehalfAsync(model);
        return Ok(result);
    }

    // Get required document types for driver onboarding
    [HttpGet("document-types/{country}")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<List<DocumentTypeResponseModel>>>> GetRequiredDocumentTypes(string country)
    {
        var documentTypes = await _documentTypeService.GetDocumentTypesByCountryAndEntityAsync(country, "Driver");
        var responseModels = documentTypes.Select(dt => new DocumentTypeResponseModel
        {
            Id = dt.Id,
            Country = dt.Country,
            EntityType = dt.EntityType,
            Name = dt.Name,
            IsRequired = dt.IsRequired,
            Description = dt.Description,
            HasTemplate = dt.HasTemplate,
            TemplateUrl = dt.TemplateUrl
        }).ToList();

        return Ok(ApiResponseModel<List<DocumentTypeResponseModel>>.Success("Document types retrieved successfully", responseModels, 200));
    }

    // Update dispatcher DOT and MC numbers
    [HttpPut("{dispatcherId}/dot-mc-numbers")]
    [Authorize(Roles = "dispatcher,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UpdateDotMcNumbers(string dispatcherId, [FromBody] UpdateDispatcherDotMcNumbersRequestModel model)
    {
        model.DispatcherId = dispatcherId; // Ensure dispatcher ID matches route
        var result = await _truckOwnerService.UpdateDispatcherDotMcNumbers(model);
        return Ok(result);
    }
}