using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IOrderService
{
    Task<ApiResponseModel<string>> CreateNewOrder(CreateOrderRequestModel model);
    Task<ApiResponseModel<string>> EditOrder(EditOrderRequestModel model);
    Task<ApiResponseModel<string>> AssignTruckToOrder(AssignTruckRequestModel model);
    Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> GetAllOrders(List<string> userRoles,
   string userId);
    Task<ApiResponseModel<OrderResponseModel>> GetOrderById(string orderId);
    Task<ApiResponseModel<bool>> UploadOrderManifest(UploadOrderManifestRequestModel model);
    Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> GetOrdersByStatus(OrderStatus orderStatus);
    Task<ApiResponseModel<bool>> UploadDeliveryManifest(UploadOrderManifestRequestModel model);
    Task<ApiResponseModel<bool>> Pay40Percent(string orderId);
    Task<ApiResponseModel<bool>> Pay60Percent(string orderId);
    Task<ApiResponseModel<OrderResponseModelForMobile>> GetOrderByIdForMobile(string orderId);
    Task<ApiResponseModel<bool>> AcceptOrderRequest(AcceptOrderRequestModel model);
    Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> SearchOrders(SearchOrderRequestModel filter);
    Task<ApiResponseModel<List<AllOrderResponseModel>>> GetPendingOrders();
    Task<ApiResponseModel<bool>> AssignOrderToTruckAsTransporter(AssignOrderToTruckAsTransporter model);
    Task<ApiResponseModel<List<AllOrderResponseModel>>> GetTransporterOrdersAsync(string truckOwnerId);
     Task<ApiResponseModel<List<AllOrderResponseModel>>> GetTransporterCompletedOrdersAsync(string truckOwnerId);
     Task<ApiResponseModel<List<AllOrderResponseModel>>> GetDriverOrdersAsync(string driverId);
}