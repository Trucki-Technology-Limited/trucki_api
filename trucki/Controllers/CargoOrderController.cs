using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> CreateOrder([FromBody] CreateCargoOrderDto createOrderDto)
        {
            var result = await _cargoOrderService.CreateOrderAsync(createOrderDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] CreateCargoOrderDto updateOrderDto)
        {
            var result = await _cargoOrderService.UpdateOrderAsync(orderId, updateOrderDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("OpenOrderForBidding")]
        public async Task<IActionResult> OpenOrderForBidding([FromBody]OpenOrderForBiddingDto model)
        {
            var result = await _cargoOrderService.OpenOrderForBiddingAsync(model.OrderId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetOpenCargoOrders")]
        public async Task<IActionResult> GetOpenCargoOrders([FromQuery] string? driverId)
        {
            var result = await _cargoOrderService.GetOpenCargoOrdersAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("driver/GetAcceptedOrdersForDriver")]
        public async Task<IActionResult> GetAcceptedOrdersForDriver(string driverId)
        {
            var result = await _cargoOrderService.GetAcceptedOrdersForDriverAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetCargoOrderById(string orderId)
        {
            var result = await _cargoOrderService.GetCargoOrderByIdAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("CreateBidAsyncid")]
        public async Task<IActionResult> CreateBid([FromBody] CreateBidDto createBidDto)
        {
            var result = await _cargoOrderService.CreateBidAsync(createBidDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("bid/{bidId}/SelectDriverBid")]
        public async Task<IActionResult> SelectDriverBid([FromBody] SelectDriverDto selectDriverDto)
        {
            var result = await _cargoOrderService.SelectDriverBidAsync(selectDriverDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("bid/DriverAcknowledgeBid")]
        public async Task<IActionResult> DriverAcknowledgeBid([FromBody] DriverAcknowledgementDto acknowledgementDto)
        {
            var result = await _cargoOrderService.DriverAcknowledgeBidAsync(acknowledgementDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{orderId}/UploadManifest")]
        public async Task<IActionResult> UploadManifest([FromBody] UploadManifestDto uploadManifestDto)
        {
            var result = await _cargoOrderService.UploadManifestAsync(uploadManifestDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{orderId}/UpdateLocation")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationDto updateLocationDto)
        {
            var result = await _cargoOrderService.UpdateLocationAsync(updateLocationDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{orderId}/CompleteDelivery")]
        public async Task<IActionResult> CompleteDelivery([FromBody] CompleteDeliveryDto completeDeliveryDto)
        {
            var result = await _cargoOrderService.CompleteDeliveryAsync(completeDeliveryDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{orderId}/GetDeliveryUpdates")]
        public async Task<IActionResult> GetDeliveryUpdates(string orderId)
        {
            var result = await _cargoOrderService.GetDeliveryUpdatesAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
