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
        Task<ApiResponseModel<PagedResponse<CargoOrderResponseModel>>> GetAllOrdersForDriverAsync(string driverId, GetDriverOrdersQueryDto query);
        #region Admin Methods

        /// <summary>
        /// Get cargo orders for admin with advanced filtering, pagination, and search
        /// </summary>
        Task<ApiResponseModel<PagedResponse<AdminCargoOrderResponseModel>>> GetCargoOrdersForAdminAsync(
                AdminGetCargoOrdersQueryDto query);

        /// <summary>
        /// Get detailed cargo order information for admin
        /// </summary>
        Task<ApiResponseModel<AdminCargoOrderDetailsResponseModel>> GetCargoOrderDetailsForAdminAsync(string orderId);

        /// <summary>
        /// Get cargo order statistics for admin dashboard
        /// </summary>
        Task<ApiResponseModel<AdminCargoOrderStatisticsResponseModel>> GetCargoOrderStatisticsForAdminAsync(
                DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Flag or unflag a cargo order
        /// </summary>
        Task<ApiResponseModel<bool>> FlagCargoOrderAsync(string orderId, bool isFlagged, string? flagReason, string adminUserId);

        /// <summary>
        /// Update cargo order status by admin (override)
        /// </summary>
        Task<ApiResponseModel<bool>> UpdateCargoOrderStatusByAdminAsync(
                string orderId, CargoOrderStatus status, string? reason, string adminUserId);
        
        /// <summary>
        /// Get cargo orders summary by status
        /// </summary>
        Task<ApiResponseModel<List<CargoOrderStatusSummaryModel>>> GetCargoOrdersSummaryAsync();

        /// <summary>
        /// Delete a cargo order (admin only)
        /// </summary>
        Task<ApiResponseModel<bool>> DeleteCargoOrderAsync(string orderId, string adminUserId);

        #endregion

}