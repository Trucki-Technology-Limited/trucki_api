using AutoMapper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly TruckiDBContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;
    private readonly IEmailService _emailSender;

    public OrderRepository(TruckiDBContext appDbContext, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;

    }
    public async Task<ApiResponseModel<string>> CreateNewOrder(CreateOrderRequestModel model)
    {
        var orderId = await GenerateUniqueOrderIdAsync();
        var business = await _context.Businesses.Where(x => x.Id == model.CompanyId).FirstOrDefaultAsync();
        if (business == null)
            return new ApiResponseModel<string>
            { IsSuccessful = false, Message = "Business not found or Inactive", StatusCode = 400 };


        var routes = await _context.RoutesEnumerable.Where(x => x.Id == model.RouteId).FirstOrDefaultAsync();
        if (routes == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Route not found",
                StatusCode = 404
            };
        }

        var order = new Order
        {
            OrderId = orderId,
            CargoType = model.CargoType,
            Quantity = model.Quantity,
            ManagerId = business.managerId,
            BusinessId = business.Id,
            OrderStatus = OrderStatus.Pending,
            CreatedAt = DateTime.Now
        };
        string startDate = model.StartDate;
        order.RoutesId = routes.Id;

        order.Price = routes.Gtv;

        order.CustomerId = model.CustomerId;
        order.StartDate = DateTime.Parse(startDate);
        order.EndDate = DateTime.Parse(startDate).AddHours(24);
        order.DeliveryAddress = model.DeliveryAddress;
        order.DeliveryLocationLat = model.DeliveryLocationLat;
        order.DeliveryLocationLong = model.DeliveryLocationLong;
        order.Consignment = model.Consignment;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Order created successfully",
            StatusCode = 200,
            Data = order.OrderId
        };
    }

    public async Task<ApiResponseModel<string>> AssignTruckToOrder(AssignTruckRequestModel model)
    {
        var order = await _context.Orders.Where(x => x.OrderId == model.OrderId).FirstOrDefaultAsync();
        if (order == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }


        var truck = await _context.Trucks.Where(x => x.Id == model.TruckId).FirstOrDefaultAsync();
        if (truck == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Truck not found",
                StatusCode = 404
            };
        }
        order.TruckId = model.TruckId;
        order.Truck = truck;
        order.OrderStatus = OrderStatus.Assigned;
        // var emailSubject = "Order Created";
        // await _emailSender.SendOrderEmailAsync(newManager.EmailAddress, emailSubject, password);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Order updated successfully",
            StatusCode = 200,
            Data = order.Id
        };
    }

    public async Task<ApiResponseModel<string>> EditOrder(EditOrderRequestModel model)
    {
        // Find the order by ID
        var order = await _context.Orders.FindAsync(model.Id);
        if (order == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }
        //string startDate = model.StartDate;

        //order.TruckNo = model.TruckNo;
        order.Quantity = model.Quantity;
        order.OrderStatus = model.OrderStatus;
        //order.StartDate = DateTime.Parse(startDate);
        //order.EndDate = DateTime.Parse(startDate).AddHours(24);


        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Order updated successfully",
            StatusCode = 200,
            Data = order.OrderId
        };
    }

    public async Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> GetAllOrders(List<string> userRoles,
string userId)
    {
        try
        {
            // Determine if the user has high-level roles
            bool isManager = userRoles.Any(role => role.Equals("manager", StringComparison.OrdinalIgnoreCase));
            bool isFieldOfficer = userRoles.Any(role => role.Equals("field officer", StringComparison.OrdinalIgnoreCase));

            // Initialize the query for orders
            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.Business)
                .Include(o => o.Manager)
                .Include(o => o.Truck)
                .Include(o => o.Routes)
                .Include(o => o.Customer)
                .AsQueryable();

            if (isManager)
            {
                // Map userId to ManagerId
                var manager = await _context.Managers
                    .FirstOrDefaultAsync(m => m.UserId == userId && m.IsActive);

                if (manager == null)
                {
                    return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
                    {
                        Data = null,
                        IsSuccessful = false,
                        Message = "Manager not found for the given user.",
                        StatusCode = 404
                    };
                }

                // Retrieve Business IDs managed by this manager
                var managedBusinessIds = await _context.Businesses
                    .Where(b => b.managerId == manager.Id)
                    .Select(b => b.Id)
                    .ToListAsync();

                if (managedBusinessIds == null || !managedBusinessIds.Any())
                {
                    // No businesses managed by this manager
                    return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
                    {
                        Data = new List<AllOrderResponseModel>(),
                        IsSuccessful = true,
                        Message = "No businesses managed by this manager.",
                        StatusCode = 200
                    };
                }

                // Filter orders based on managed businesses
                ordersQuery = ordersQuery.Where(o => managedBusinessIds.Contains(o.BusinessId));
            }
            if (isFieldOfficer)
            {
                // Map userId to ManagerId
                var officer = await _context.Officers
                    .FirstOrDefaultAsync(m => m.UserId == userId);

                if (officer == null)
                {
                    return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
                    {
                        Data = null,
                        IsSuccessful = false,
                        Message = "officer not found for the given user.",
                        StatusCode = 404
                    };
                }

                // Retrieve Business IDs managed by this manager
                var managedBusiness = await _context.Businesses
                    .FirstOrDefaultAsync(b => b.Id == officer.CompanyId);

                if (managedBusiness == null)
                {
                    // No businesses managed by this manager
                    return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
                    {
                        Data = new List<AllOrderResponseModel>(),
                        IsSuccessful = true,
                        Message = "No businesses managed by this manager.",
                        StatusCode = 200
                    };
                }

                // Filter orders based on managed businesses
                ordersQuery = ordersQuery.Where(o => o.BusinessId == managedBusiness.Id);
            }
            // Else, if user is Chief Manager or Finance, retrieve all orders (no additional filter)

            // Execute the query
            List<Order> ordersWithDetails = await ordersQuery.ToListAsync();

            if (ordersWithDetails == null || !ordersWithDetails.Any())
            {
                return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "No orders found",
                    StatusCode = 404,
                    Data = new List<AllOrderResponseModel> { }
                };
            }

            var orderResponseList = ordersWithDetails.Select(order => new AllOrderResponseModel
            {
                Id = order.Id, // Assuming there's a property called Id in Order entity
                OrderId = order.OrderId,
                TruckNo = order.Truck?.PlateNumber ?? "",
                Quantity = order.Quantity,
                StartDate = order.StartDate,
                EndDate = order.EndDate,
                OrderStatus = order.OrderStatus,
                Routes = _mapper.Map<RouteResponseModel>(order.Routes),
                Business = _mapper.Map<AllBusinessResponseModel>(order.Business),
                Customer = _mapper.Map<AllCustomerResponseModel>(order.Customer),
                Consignment = order.Consignment,
                DeliveryLocationLat = order.DeliveryLocationLat,
                DeliveryLocationLong = order.DeliveryLocationLong,
                CreatedAt = order.CreatedAt,
            });

            return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
            {
                IsSuccessful = true,
                Message = "Orders retrieved successfully",
                StatusCode = 200,
                Data = orderResponseList
            };
        }
        catch (Exception ex)
        {
            // Log the exception
            return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
            {
                IsSuccessful = false,
                Message = "Failed to retrieve orders",
                StatusCode = 500,
                Data = null
            };
        }
    }

    public async Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> GetOrdersByStatus(OrderStatus orderStatus)
    {
        try
        {
            var ordersWithDetails = await _context.Orders
                .Include(o => o.Business)
                .Include(o => o.Manager)
                .Include(o => o.Truck)
                .Include(o => o.Routes)
                .Include(o => o.Customer)
                .Where(o => o.OrderStatus == orderStatus) // Filter by the provided status
                .ToListAsync();
            if (ordersWithDetails == null || !ordersWithDetails.Any())
            {
                return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "No orders found",
                    StatusCode = 404,
                    Data = new List<AllOrderResponseModel> { }
                };
            }

            var orderResponseList = ordersWithDetails.Select(order => new AllOrderResponseModel
            {
                Id = order.Id, // Assuming there's a property called Id in Order entity
                OrderId = order.OrderId,
                TruckNo = order.Truck?.PlateNumber ?? "",
                Quantity = order.Quantity,
                StartDate = order.StartDate,
                EndDate = order.EndDate,
                OrderStatus = order.OrderStatus,
                Routes = _mapper.Map<RouteResponseModel>(order.Routes),
                Business = _mapper.Map<AllBusinessResponseModel>(order.Business),
                Consignment = order.Consignment,
                DeliveryLocationLat = order.DeliveryLocationLat,
                DeliveryLocationLong = order.DeliveryLocationLong,
                CreatedAt = order.CreatedAt
            });

            return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
            {
                IsSuccessful = true,
                Message = "Orders retrieved successfully",
                StatusCode = 200,
                Data = orderResponseList
            };
        }
        catch (Exception ex)
        {
            // Log the exception
            return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
            {
                IsSuccessful = false,
                Message = "Failed to retrieve orders",
                StatusCode = 500,
                Data = null
            };
        }
    }

    public async Task<ApiResponseModel<OrderResponseModel>> GetOrderById(string orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Truck)
            .Include(o => o.Routes)
            .Include(o => o.Customer) // Add Customer include
            .Include(o => o.Truck.TruckOwner)
            .ThenInclude(to => to.BankDetails).Where(x => x.Id == orderId)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return new ApiResponseModel<OrderResponseModel>
            {
                IsSuccessful = false,
                Message = "Failed to retrieve orders",
                StatusCode = 500,
                Data = new OrderResponseModel()
            };
        }

        var orderResponseModel = new OrderResponseModel
        {
            Id = order.Id,
            OrderId = order.OrderId,
            TruckNo = order.Truck?.PlateNumber ?? "",
            Quantity = order.Quantity,
            businessId = order.BusinessId,
            CargoType = order.CargoType,
            OrderStatus = order.OrderStatus,
            RouteFrom = order.Routes?.FromRoute ?? "",
            RouteTo = order.Routes?.ToRoute ?? "",
            StartDate = order.StartDate.ToString(),
            EndDate = order.EndDate.ToString(),
            Price = order.Routes?.Price,
            Driver = order.Truck?.DriverId ?? "",
            Customer = order.Customer?.CustomerName ?? "", // Get customer name
            DeliveryLocation = order.DeliveryAddress ?? "", // Get delivery address
            Documents = order.Documents,
            DeliveryDocuments = order.DeliveryDocuments,
            TruckOwnerName = order.Truck?.TruckOwner?.Name ?? "",
            TruckOwnerBankName = order.Truck?.TruckOwner?.BankDetails?.BankName ?? "",
            TruckOwnerBankAccountNumber = order.Truck?.TruckOwner?.BankDetails?.BankAccountNumber ?? "",
            is60Paid = order.is60Paid,
            is40Paid = order.is40Paid,
            Consignment = order.Consignment,
            DeliveryLocationLat = order.DeliveryLocationLat,
            DeliveryLocationLong = order.DeliveryLocationLong,
        };

        return new ApiResponseModel<OrderResponseModel>
        {
            IsSuccessful = true,
            Message = "Order retrieved successfully",
            StatusCode = 200,
            Data = orderResponseModel
        };
    }

    public async Task<ApiResponseModel<OrderResponseModelForMobile>> GetOrderByIdForMobile(string orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Truck)
            .Include(o => o.Business)
            .Include(o => o.Routes)
            .Include(o => o.Customer) // Add Customer include
            .Include(o => o.Truck.TruckOwner)
            .ThenInclude(to => to.BankDetails).Where(x => x.Id == orderId)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return new ApiResponseModel<OrderResponseModelForMobile>
            {
                IsSuccessful = false,
                Message = "Failed to retrieve orders",
                StatusCode = 500,
                Data = new OrderResponseModelForMobile()
            };
        }

        var orderResponseModel = new OrderResponseModelForMobile
        {
            Id = order.Id,
            OrderId = order.OrderId,
            TruckNo = order.Truck?.PlateNumber ?? "",
            Quantity = order.Quantity,
            businessId = order.BusinessId,
            businessName = order.Business.Name,
            businessLocation = order.Business.Location,
            customerId = order.CustomerId,
            customerName = order.Customer.CustomerName,
            customerLocation = order.Customer.Location,
            CargoType = order.CargoType,
            OrderStatus = order.OrderStatus,
            RouteFrom = order.Routes?.FromRoute ?? "",
            RouteTo = order.Routes?.ToRoute ?? "",
            StartDate = order.StartDate.ToString(),
            EndDate = order.EndDate.ToString(),
            Price = order.Routes?.Price,
            Driver = order.Truck?.DriverId ?? "",
            Customer = order.Customer?.CustomerName ?? "", // Get customer name
            DeliveryLocation = order.DeliveryAddress ?? "", // Get delivery address
            Documents = order.Documents,
            DeliveryDocuments = order.DeliveryDocuments,
            TruckOwnerName = order.Truck?.TruckOwner?.Name ?? "",
            TruckOwnerBankName = order.Truck?.TruckOwner?.BankDetails?.BankName ?? "",
            TruckOwnerBankAccountNumber = order.Truck?.TruckOwner?.BankDetails?.BankAccountNumber ?? "",
            is60Paid = order.is60Paid,
            is40Paid = order.is40Paid,
            Consignment = order.Consignment,
            DeliveryLocationLat = order.DeliveryLocationLat,
            DeliveryLocationLong = order.DeliveryLocationLong,
        };

        return new ApiResponseModel<OrderResponseModelForMobile>
        {
            IsSuccessful = true,
            Message = "Order retrieved successfully",
            StatusCode = 200,
            Data = orderResponseModel
        };
    }

    private async Task<string> GenerateUniqueOrderIdAsync()
    {
        string orderId;
        bool exists;
        var random = new Random();

        do
        {
            var randomNumber = random.Next(1000, 9999).ToString();
            orderId = "TR" + randomNumber;

            exists = await _context.Orders.AnyAsync(o => o.OrderId == orderId);
        }
        while (exists);

        return orderId;
    }

    public async Task<ApiResponseModel<bool>> UploadOrderManifest(UploadOrderManifestRequestModel model)
    {
        var order = await _context.Orders.FindAsync(model.orderId);
        if (order == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }

        // 1. Validation
        if (!model.Documents.Any())
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "No files uploaded",
                StatusCode = 400
            };
        }

        // 4. Update Order
        if (order.Documents == null)
        {
            order.Documents = new List<string>();
        }

        order.Documents.AddRange(model.Documents);

        // Update order status if this is the first document being uploadedssss
        // If this is the first time *any* documents are being added:
        if (order.Documents.Count > 0 && order.OrderStatus == OrderStatus.Assigned)
        {
            order.OrderStatus = OrderStatus.Loaded;
        }

        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Documents uploaded and order updated",
            StatusCode = 200,
        };
    }
    public async Task<ApiResponseModel<bool>> Pay60Percent(string orderId)
    {
        var order = await _context.Orders.Include(e => e.Business).Include(a => a.Truck).Where(e => e.Id == orderId).FirstOrDefaultAsync();
        if (order == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }
        var transaction = new Transaction
        {
            OrderId = orderId,
            BusinessId = order.BusinessId,
            TruckId = order.TruckId,  // If you want to track truck
            Amount = order.Price.HasValue ? (decimal)order.Price.Value * 0.6M : 0, // Calculate 60% based on the order's total price
            TransactionDate = DateTime.Now,
            Type = TransactionType.SixtyPercentPayment
        };

        _context.Transactions.Add(transaction);

        order.is60Paid = true;
        order.OrderStatus = OrderStatus.InTransit;

        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "payment made",
            StatusCode = 200,
        };
    }
    public async Task<ApiResponseModel<bool>> Pay40Percent(string orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }

        var transaction = new Transaction
        {
            OrderId = orderId,
            BusinessId = order.BusinessId,
            TruckId = order.TruckId,  // If you want to track truck
            Amount = order.Price.HasValue ? (decimal)order.Price.Value * 0.4M : 0, // Calculate 60% based on the order's total price
            TransactionDate = DateTime.Now,
            Type = TransactionType.FortyPercentPayment
        };

        _context.Transactions.Add(transaction);
        order.is40Paid = true;
        order.OrderStatus = OrderStatus.Delivered;

        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "payment made",
            StatusCode = 200,
        };
    }
    public async Task<ApiResponseModel<bool>> UploadDeliveryManifest(UploadOrderManifestRequestModel model)
    {
        var order = await _context.Orders.FindAsync(model.orderId);
        if (order == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }

        // 1. Validation
        if (!model.Documents.Any())
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "No files uploaded",
                StatusCode = 400
            };
        }

        if (order.Documents == null)
        {
            order.DeliveryDocuments = new List<string>();
        }

        order.DeliveryDocuments.AddRange(model.Documents);

        // Update order status if this is the first document being uploaded
        // if (order.DeliveryDocuments.Count == model.Documents.Count)
        // {
        order.OrderStatus = OrderStatus.Destination;
        // }

        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Documents uploaded and order updated",
            StatusCode = 200,
        };
    }
    public async Task<ApiResponseModel<bool>> AcceptOrderRequest(AcceptOrderRequestModel model)
    {
        // Find the order by the provided orderId
        var order = await _context.Orders.Include(o => o.Truck).FirstOrDefaultAsync(o => o.Id == model.orderId);

        if (order == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Order not found",
                StatusCode = 404
            };
        }

        // Check if the order has a truck assigned
        if (order.Truck == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "No truck assigned to this order",
                StatusCode = 400
            };
        }

        // Check if the driverId matches the driver assigned to the truck (assuming Truck has a DriverId)
        if (order.Truck.DriverId != model.driverId)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "This driver is not assigned to the truck for this order",
                StatusCode = 400
            };
        }

        // If the driver is assigned to the truck, update the order status
        if (model.status == "accepted")
        {
            order.OrderStatus = OrderStatus.Accepted;
        }
        else if (model.status == "declined")
        {
            order.OrderStatus = OrderStatus.Declined;
        }
        else
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Invalid status update",
                StatusCode = 400
            };
        }

        // Save changes to the database
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Order status updated successfully",
            StatusCode = 200,
        };
    }

    public async Task<ApiResponseModel<IEnumerable<AllOrderResponseModel>>> SearchOrders(SearchOrderRequestModel filter)
    {
        // Start with a base query
        var query = _context.Orders.Include(o => o.Truck).AsQueryable();

        // Filter by start and end date if both are provided
        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
            query = query.Where(o => o.StartDate >= filter.StartDate && o.EndDate <= filter.EndDate);

        // Filter by truck plate number if provided
        if (!string.IsNullOrEmpty(filter.TruckNo))
            query = query.Where(o => o.Truck != null && o.Truck.PlateNumber == filter.TruckNo);

        // Filter by status if provided
        if (filter.Status.HasValue)
            query = query.Where(o => o.OrderStatus == filter.Status);

        // Filter by quantity if provided
        if (!string.IsNullOrEmpty(filter.Quantity))
            query = query.Where(o => o.Quantity == filter.Quantity);

        // Filter by created date if provided
        if (filter.CreatedAt.HasValue)
            query = query.Where(o => o.CreatedAt.Date == filter.CreatedAt.Value.Date);

        // Execute the query and get the results
        var orders = await query.ToListAsync();
        var orderResponseList = orders.Select(order => new AllOrderResponseModel
        {
            Id = order.Id, // Assuming there's a property called Id in Order entity
            OrderId = order.OrderId,
            TruckNo = order.Truck?.PlateNumber ?? "",
            Quantity = order.Quantity,
            StartDate = order.StartDate,
            EndDate = order.EndDate,
            OrderStatus = order.OrderStatus,
            Routes = _mapper.Map<RouteResponseModel>(order.Routes),
            Business = _mapper.Map<AllBusinessResponseModel>(order.Business),
            Customer = _mapper.Map<AllCustomerResponseModel>(order.Customer),
            Consignment = order.Consignment,
            DeliveryLocationLat = order.DeliveryLocationLat,
            DeliveryLocationLong = order.DeliveryLocationLong,
            CreatedAt = order.CreatedAt,
        });
        return new ApiResponseModel<IEnumerable<AllOrderResponseModel>>
        {
            IsSuccessful = true,
            Message = "Orders retrieved successfully",
            StatusCode = 200,
            Data = orderResponseList
        };
    }


}