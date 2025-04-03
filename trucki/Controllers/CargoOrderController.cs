using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;


namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CargoOrderController : ControllerBase
    {
        private readonly ICargoOrderService _cargoOrderService;

        public CargoOrderController(ICargoOrderService cargoOrderService)
        {
            _cargoOrderService = cargoOrderService;
        }

        [HttpPost("CreateOrderAsync")]
        public async Task<ActionResult<ApiResponseModel<bool>>> CreateOrder([FromBody] CreateCargoOrderDto createOrderDto)
        {
            var result = await _cargoOrderService.CreateOrderAsync(createOrderDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{orderId}")]
        public async Task<ActionResult<ApiResponseModel<bool>>> UpdateOrder(string orderId, [FromBody] CreateCargoOrderDto updateOrderDto)
        {
            var result = await _cargoOrderService.UpdateOrderAsync(orderId, updateOrderDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("OpenOrderForBidding")]
        public async Task<ActionResult<ApiResponseModel<bool>>> OpenOrderForBidding([FromBody] OpenOrderForBiddingDto model)
        {
            var result = await _cargoOrderService.OpenOrderForBiddingAsync(model.OrderId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetOpenCargoOrders")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>>> GetOpenCargoOrders([FromQuery] string? driverId)
        {
            var result = await _cargoOrderService.GetOpenCargoOrdersAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("driver/GetAcceptedOrdersForDriver")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>>> GetAcceptedOrdersForDriver(string driverId)
        {
            var result = await _cargoOrderService.GetAcceptedOrdersForDriverAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<ApiResponseModel<CargoOrderResponseModel>>> GetCargoOrderById(string orderId)
        {
            var result = await _cargoOrderService.GetCargoOrderByIdAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("CreateBidAsyncid")]
        public async Task<ActionResult<ApiResponseModel<bool>>> CreateBid([FromBody] CreateBidDto createBidDto)
        {
            var result = await _cargoOrderService.CreateBidAsync(createBidDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("bid/{bidId}/SelectDriverBid")]
        public async Task<ActionResult<ApiResponseModel<StripePaymentResponse>>> SelectDriverBid([FromBody] SelectDriverDto selectDriverDto)
        {
            var result = await _cargoOrderService.SelectDriverBidAsync(selectDriverDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("bid/DriverAcknowledgeBid")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DriverAcknowledgeBid([FromBody] DriverAcknowledgementDto acknowledgementDto)
        {
            var result = await _cargoOrderService.DriverAcknowledgeBidAsync(acknowledgementDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("UploadManifest")]
        public async Task<ActionResult<ApiResponseModel<bool>>> UploadManifest([FromBody] UploadManifestDto uploadManifestDto)
        {
            var result = await _cargoOrderService.UploadManifestAsync(uploadManifestDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("UpdateLocation")]
        public async Task<ActionResult<ApiResponseModel<bool>>> UpdateLocation([FromBody] UpdateLocationDto updateLocationDto)
        {
            var result = await _cargoOrderService.UpdateLocationAsync(updateLocationDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("CompleteDelivery")]
        public async Task<ActionResult<ApiResponseModel<bool>>> CompleteDelivery([FromBody] CompleteDeliveryDto completeDeliveryDto)
        {
            var result = await _cargoOrderService.CompleteDeliveryAsync(completeDeliveryDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetDeliveryUpdates/{orderId}")]
        public async Task<ActionResult<ApiResponseModel<List<DeliveryLocationUpdate>>>> GetDeliveryUpdates(string orderId)
        {
            var result = await _cargoOrderService.GetDeliveryUpdatesAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("StartOrderDto")]
        [Authorize(Roles = "driver")]
        public async Task<ActionResult<ApiResponseModel<bool>>> StartOrder([FromBody] StartOrderDto startOrderDto)
        {
            var result = await _cargoOrderService.StartOrderAsync(startOrderDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("driver/GetCompletedOrders")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>>> GetCompletedOrdersForDriver(string driverId)
        {
            var result = await _cargoOrderService.GetCompletedOrdersForDriverAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("driver/summary")]
        public async Task<ActionResult<ApiResponseModel<DriverSummaryResponseModel>>> GetDriverSummary(string driverId)
        {
            var result = await _cargoOrderService.GetDriverSummaryAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("bid/UpdateBid")]
        [Authorize(Roles = "driver")]
        public async Task<ActionResult<ApiResponseModel<bool>>> UpdateBid([FromBody] UpdateBidDto updateBidDto)
        {
            var result = await _cargoOrderService.UpdateBidAsync(updateBidDto);
            return StatusCode(result.StatusCode, result);
        }
    }
}