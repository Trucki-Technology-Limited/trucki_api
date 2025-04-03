using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface ICargoOrderService
{

        Task<ApiResponseModel<bool>> CreateOrderAsync(CreateCargoOrderDto createOrderDto);
        Task<ApiResponseModel<bool>> CreateBidAsync(CreateBidDto createBidDto);
        Task<ApiResponseModel<CargoOrderResponseModel>> GetCargoOrderByIdAsync(string orderId);
        Task<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>> GetOpenCargoOrdersAsync(string? driverId = null);
        Task<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>> GetAcceptedOrdersForDriverAsync(string driverId);
        Task<ApiResponseModel<bool>> UpdateOrderAsync(string orderId, CreateCargoOrderDto updateOrderDto);
        Task<ApiResponseModel<bool>> OpenOrderForBiddingAsync(string orderId);
        Task<ApiResponseModel<StripePaymentResponse>> SelectDriverBidAsync(SelectDriverDto selectDriverDto);
        Task<ApiResponseModel<bool>> DriverAcknowledgeBidAsync(DriverAcknowledgementDto acknowledgementDto);
        Task<ApiResponseModel<List<DeliveryLocationUpdate>>> GetDeliveryUpdatesAsync(string orderId);
        Task<ApiResponseModel<bool>> CompleteDeliveryAsync(CompleteDeliveryDto completeDeliveryDto);
        Task<ApiResponseModel<bool>> UpdateLocationAsync(UpdateLocationDto updateLocationDto);
        Task<ApiResponseModel<bool>> UploadManifestAsync(UploadManifestDto uploadManifestDto);
        Task<ApiResponseModel<bool>> StartOrderAsync(StartOrderDto startOrderDto);
        Task<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>> GetCompletedOrdersForDriverAsync(string driverId);
        Task<ApiResponseModel<DriverSummaryResponseModel>> GetDriverSummaryAsync(string driverId);
        Task<ApiResponseModel<bool>> UpdateOrderPaymentStatusAsync(string orderId, string paymentIntentId);
        Task<ApiResponseModel<bool>> UpdateBidAsync(UpdateBidDto updateBidDto);

}