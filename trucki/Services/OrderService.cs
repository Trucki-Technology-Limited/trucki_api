using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;

    }

    public async Task<ApiResponseModel<string>> CreateNewOrder(CreateOrderRequestModel model)
    {
        var res = await _orderRepository.CreateNewOrder(model);
        return res;
    }
    public async Task<ApiResponseModel<string>> EditOrder(EditOrderRequestModel model)
    {
        var res = await _orderRepository.EditOrder(model);
        return res;
    }
    public async Task<ApiResponseModel<string>> AssignTruckToOrder(AssignTruckRequestModel model)
    {
        var res = await _orderRepository.AssignTruckToOrder(model);
        return res;
    }

    public async Task<ApiResponseModel<PaginatedListDto<AllOrderResponseModel>>> GetAllOrders(
        List<string> userRoles, 
        string userId, 
        GetAllOrdersRequestModel request)
    {
        return await _orderRepository.GetAllOrders(userRoles, userId, request);
    }
    public async Task<ApiResponseModel<OrderResponseModel>> GetOrderById(string orderId)
    {
        var res = await _orderRepository.GetOrderById(orderId);
        return res;
    }
    public async Task<ApiResponseModel<bool>> UploadOrderManifest(UploadOrderManifestRequestModel model)
    {
        var res = await _orderRepository.UploadOrderManifest(model);
        return res;
    }
    public async Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> GetOrdersByStatus(OrderStatus orderStatus)
    {
        var res = await _orderRepository.GetOrdersByStatus(orderStatus);
        return res;
    }
    public async Task<ApiResponseModel<bool>> UploadDeliveryManifest(UploadOrderManifestRequestModel model)
    {
        var res = await _orderRepository.UploadDeliveryManifest(model);
        return res;
    }

    public async Task<ApiResponseModel<bool>> Pay60Percent(string model)
    {
        var res = await _orderRepository.Pay60Percent(model);
        return res;
    }
    public async Task<ApiResponseModel<bool>> Pay40Percent(string model)
    {
        var res = await _orderRepository.Pay40Percent(model);
        return res;
    }
    public async Task<ApiResponseModel<OrderResponseModelForMobile>> GetOrderByIdForMobile(string orderId)
    {
        var res = await _orderRepository.GetOrderByIdForMobile(orderId);
        return res;
    }
    public async Task<ApiResponseModel<bool>> AcceptOrderRequest(AcceptOrderRequestModel model)
    {
        var res = await _orderRepository.AcceptOrderRequest(model);
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> SearchOrders(SearchOrderRequestModel filter)
    {
        return await _orderRepository.SearchOrders(filter);
    }

    public async Task<ApiResponseModel<List<AllOrderResponseModel>>> GetPendingOrders()
    {
        var res = await _orderRepository.GetPendingOrders();
        return res;
    }

    public async Task<ApiResponseModel<bool>> AssignOrderToTruckAsTransporter(AssignOrderToTruckAsTransporter model)
    {
        var res = await _orderRepository.AssignOrderToTruckAsTransporter(model);
        return res;
    }
    public async Task<ApiResponseModel<List<AllOrderResponseModel>>> GetTransporterOrdersAsync(string truckOwnerId)
    {
        var res = await _orderRepository.GetTransporterOrdersAsync(truckOwnerId);
        return res;
    }
    public async Task<ApiResponseModel<List<AllOrderResponseModel>>> GetTransporterCompletedOrdersAsync(string truckOwnerId)
    {
        var res = await _orderRepository.GetTransporterCompletedOrdersAsync(truckOwnerId);
        return res;
    }
    public async Task<ApiResponseModel<List<AllOrderResponseModel>>> GetDriverOrdersAsync(string driverId)
    {
        var res = await _orderRepository.GetDriverOrdersAsync(driverId);
        return res;
    }
}