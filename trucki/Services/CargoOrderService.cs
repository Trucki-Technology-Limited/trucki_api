using System.Threading.Tasks;
using trucki.Models.ResponseModels;
using trucki.Entities;
using Microsoft.EntityFrameworkCore;
using trucki.Interfaces.IServices;
using trucki.DatabaseContext;
using trucki.Models.RequestModel;
using AutoMapper; // If you're using EF Core

namespace trucki.Services
{
    public class CargoOrderService : ICargoOrderService
    {
        private readonly TruckiDBContext _dbContext; // Example EF DbContext
        private readonly IMapper _mapper;
        private readonly IStripeService _stripeService;

        public CargoOrderService(TruckiDBContext dbContext, IMapper mapper, IStripeService stripeService)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
            _mapper = mapper;
        }

        public async Task<ApiResponseModel<bool>> CreateOrderAsync(CreateCargoOrderDto createOrderDto)
        {
            try
            {
                // Validate items
                if (createOrderDto.Items == null || !createOrderDto.Items.Any())
                {
                    return ApiResponseModel<bool>.Fail("At least one cargo item is required", 400);
                }

                // Validate each item
                foreach (var item in createOrderDto.Items)
                {
                    if (item.ItemImages == null || !item.ItemImages.Any())
                    {
                        return ApiResponseModel<bool>.Fail("Each cargo item must have at least one image", 400);
                    }

                    if (item.Weight <= 0 || item.Length <= 0 || item.Width <= 0 || item.Height <= 0)
                    {
                        return ApiResponseModel<bool>.Fail("Invalid dimensions or weight for cargo item", 400);
                    }

                    if (item.Quantity <= 0)
                    {
                        return ApiResponseModel<bool>.Fail("Quantity must be greater than 0", 400);
                    }
                }

                // Set pickup date and time
                if (createOrderDto.PickupDateTime <= DateTime.UtcNow)
                {
                    return ApiResponseModel<bool>.Fail("Pickup time must be in the future", 400);
                }

                // Create new order
                var newOrder = new CargoOrders
                {
                    CargoOwnerId = createOrderDto.CargoOwnerId,
                    PickupLocation = createOrderDto.PickupLocation,
                    PickupLocationLat = createOrderDto.PickupLocationLat,
                    PickupLocationLong = createOrderDto.PickupLocationLong,
                    DeliveryLocation = createOrderDto.DeliveryLocation,
                    DeliveryLocationLat = createOrderDto.DeliveryLocationLat,
                    DeliveryLocationLong = createOrderDto.DeliveryLocationLong,
                    CountryCode = createOrderDto.CountryCode,
                    PickupDateTime = createOrderDto.PickupDateTime,
                    RequiredTruckType = createOrderDto.RequiredTruckType,
                    Status = createOrderDto.OpenForBidding ? CargoOrderStatus.OpenForBidding : CargoOrderStatus.Draft,
                    Items = createOrderDto.Items.Select(item => new CargoOrderItem
                    {
                        Description = item.Description,
                        Weight = item.Weight,
                        Length = item.Length,
                        Width = item.Width,
                        Height = item.Height,
                        IsFragile = item.IsFragile,
                        SpecialHandlingInstructions = item.SpecialHandlingInstructions,
                        Type = item.Type,
                        Quantity = item.Quantity,
                        ItemImages = item.ItemImages
                    }).ToList()
                };

                _dbContext.Set<CargoOrders>().Add(newOrder);
                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Order created successfully", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<bool>> UpdateOrderAsync(string orderId, CreateCargoOrderDto updateOrderDto)
        {
            try
            {
                var existingOrder = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (existingOrder == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                // Can only update if order is in Draft status
                if (existingOrder.Status != CargoOrderStatus.Draft)
                {
                    return ApiResponseModel<bool>.Fail("Order can only be updated when in Draft status", 400);
                }

                // Validate items
                if (updateOrderDto.Items == null || !updateOrderDto.Items.Any())
                {
                    return ApiResponseModel<bool>.Fail("At least one cargo item is required", 400);
                }

                // Validate each item
                foreach (var item in updateOrderDto.Items)
                {
                    if (item.ItemImages == null || !item.ItemImages.Any())
                    {
                        return ApiResponseModel<bool>.Fail("Each cargo item must have at least one image", 400);
                    }

                    if (item.Weight <= 0 || item.Length <= 0 || item.Width <= 0 || item.Height <= 0)
                    {
                        return ApiResponseModel<bool>.Fail("Invalid dimensions or weight for cargo item", 400);
                    }

                    if (item.Quantity <= 0)
                    {
                        return ApiResponseModel<bool>.Fail("Quantity must be greater than 0", 400);
                    }
                }

                // Update order details
                existingOrder.PickupLocation = updateOrderDto.PickupLocation;
                existingOrder.PickupLocationLat = updateOrderDto.PickupLocationLat;
                existingOrder.PickupLocationLong = updateOrderDto.PickupLocationLong;
                existingOrder.DeliveryLocation = updateOrderDto.DeliveryLocation;
                existingOrder.DeliveryLocationLat = updateOrderDto.DeliveryLocationLat;
                existingOrder.DeliveryLocationLong = updateOrderDto.DeliveryLocationLong;
                existingOrder.CountryCode = updateOrderDto.CountryCode;
                existingOrder.RequiredTruckType = updateOrderDto.RequiredTruckType;

                // If OpenForBidding is true, update the status
                if (updateOrderDto.OpenForBidding)
                {
                    existingOrder.Status = CargoOrderStatus.OpenForBidding;
                }

                // Remove existing items and add new ones
                _dbContext.Set<CargoOrderItem>().RemoveRange(existingOrder.Items);
                existingOrder.Items = updateOrderDto.Items.Select(item => new CargoOrderItem
                {
                    Description = item.Description,
                    Weight = item.Weight,
                    Length = item.Length,
                    Width = item.Width,
                    Height = item.Height,
                    IsFragile = item.IsFragile,
                    SpecialHandlingInstructions = item.SpecialHandlingInstructions,
                    Type = item.Type,
                    Quantity = item.Quantity,
                    ItemImages = item.ItemImages
                }).ToList();

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Order updated successfully", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }


        public async Task<ApiResponseModel<bool>> OpenOrderForBiddingAsync(string orderId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                if (order.Status != CargoOrderStatus.Draft)
                {
                    return ApiResponseModel<bool>.Fail("Only draft orders can be opened for bidding", 400);
                }

                order.Status = CargoOrderStatus.OpenForBidding;
                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Order is now open for bidding", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }



        public async Task<ApiResponseModel<bool>> CreateBidAsync(CreateBidDto createBidDto)
        {
            try
            {
                // Check if the order exists and is open for bidding
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.Bids)
                    .FirstOrDefaultAsync(o => o.Id == createBidDto.OrderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                if (order.Status != CargoOrderStatus.OpenForBidding &&
                    order.Status != CargoOrderStatus.BiddingInProgress)
                {
                    return ApiResponseModel<bool>.Fail("Order is not open for bidding", 400);
                }

                // Check if the driver/truck exists and is available
                var driver = await _dbContext.Set<Driver>()
                    .Include(d => d.Truck)
                    .FirstOrDefaultAsync(d => d.Id == createBidDto.DriverId);

                if (driver == null || driver.Truck == null)
                {
                    return ApiResponseModel<bool>.Fail("Driver or truck not found", 404);
                }

                if (driver.Truck.TruckStatus != TruckStatus.Available)
                {
                    return ApiResponseModel<bool>.Fail("Truck is not available for bidding", 400);
                }

                // Check if driver already has a pending bid for this order
                var existingBid = order.Bids.FirstOrDefault(b =>
                    b.TruckId == driver.TruckId &&
                    b.Status == BidStatus.Pending);

                if (existingBid != null)
                {
                    // Update existing bid
                    existingBid.Amount = createBidDto.Amount;
                }
                else
                {
                    // Create new bid
                    var newBid = new Bid
                    {
                        OrderId = order.Id,
                        TruckId = driver.TruckId,
                        Amount = createBidDto.Amount,
                        Status = BidStatus.Pending
                    };

                    _dbContext.Set<Bid>().Add(newBid);
                }

                // Update order status if this is the first bid
                if (order.Status == CargoOrderStatus.OpenForBidding)
                {
                    order.Status = CargoOrderStatus.BiddingInProgress;
                }

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Bid submitted successfully", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }


        public async Task<ApiResponseModel<StripePaymentResponse>> SelectDriverBidAsync(SelectDriverDto selectDriverDto)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.Bids)
                    .Include(o => o.CargoOwner)
                    .FirstOrDefaultAsync(o => o.Id == selectDriverDto.OrderId);

                if (order == null)
                {
                    return ApiResponseModel<StripePaymentResponse>.Fail("Order not found", 404);
                }

                if (order.Status != CargoOrderStatus.BiddingInProgress)
                {
                    return ApiResponseModel<StripePaymentResponse>.Fail("Order is not in bidding state", 400);
                }

                var selectedBid = order.Bids.FirstOrDefault(b => b.Id == selectDriverDto.BidId);
                if (selectedBid == null)
                {
                    return ApiResponseModel<StripePaymentResponse>.Fail("Bid not found", 404);
                }

                // Check the cargo owner type and handle payment flow accordingly
                if (order.CargoOwner.OwnerType == CargoOwnerType.Shipper)
                {

                    var bidAmount = selectedBid.Amount;
                    var systemFee = bidAmount * 0.20m; // 20% of the bid amount
                    var subtotal = bidAmount + systemFee;
                    var tax = subtotal * 0.10m; // Assuming a 10% tax rate (adjust as needed)
                    var totalAmount = subtotal + tax;

                    // Store these values in the order
                    order.TotalAmount = bidAmount;
                    order.SystemFee = systemFee;
                    order.Tax = tax;
                    // For Shippers, we just mark the bid as selected but don't update the order status yet
                    var paymentResponse = await _stripeService.CreatePaymentIntent(order.Id, totalAmount, "usd");
                    // We'll wait for payment confirmation
                    order.AcceptedBidId = selectedBid.Id;
                    selectedBid.Status = BidStatus.CargoOwnerSelected;

                    // Set the payment method to Stripe for Shippers
                    order.PaymentMethod = PaymentMethodType.Stripe;
                    order.PaymentIntentId = paymentResponse.PaymentIntentId;
                    paymentResponse.PaymentBreakdown = new PaymentBreakdown
                    {
                        BidAmount = bidAmount,
                        SystemFee = systemFee,
                        Tax = tax,
                        TotalAmount = totalAmount
                    };
                    // The order status remains as BiddingInProgress until payment is confirmed
                    // We'll update other bids only after payment

                    await _dbContext.SaveChangesAsync();

                    return ApiResponseModel<StripePaymentResponse>.Success(
                                  "Driver selected, please complete payment",
                                  paymentResponse,
                                  200);
                }
                else // Broker
                {
                    // For Brokers, follow the existing flow, updating the order status
                    var bidAmount = selectedBid.Amount;
                    var systemFee = bidAmount * 0.20m;
                    var tax = (bidAmount + systemFee) * 0.10m;

                    order.TotalAmount = bidAmount;
                    order.SystemFee = systemFee;
                    order.Tax = tax;
                    order.Status = CargoOrderStatus.DriverSelected;
                    order.AcceptedBidId = selectedBid.Id;
                    selectedBid.Status = BidStatus.CargoOwnerSelected;
                    order.PaymentMethod = PaymentMethodType.Invoice;

                    // Mark other bids as expired
                    foreach (var bid in order.Bids.Where(b => b.Id != selectedBid.Id))
                    {
                        bid.Status = BidStatus.Expired;
                    }

                    await _dbContext.SaveChangesAsync();

                    return ApiResponseModel<StripePaymentResponse>.Success(
                                  "Driver selected successfully",
                                  new StripePaymentResponse(),
                                  200);
                }
            }
            catch (Exception ex)
            {
                return ApiResponseModel<StripePaymentResponse>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<bool>> DriverAcknowledgeBidAsync(DriverAcknowledgementDto acknowledgementDto)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                    .FirstOrDefaultAsync(o => o.Id == acknowledgementDto.OrderId);

                if (order == null || order.AcceptedBid == null)
                {
                    return ApiResponseModel<bool>.Fail("Order or accepted bid not found", 404);
                }

                if (order.Status != CargoOrderStatus.DriverSelected)
                {
                    return ApiResponseModel<bool>.Fail("Order is not in driver selection state", 400);
                }

                if (acknowledgementDto.IsAcknowledged)
                {
                    order.Status = CargoOrderStatus.DriverAcknowledged;
                    order.AcceptedBid.Status = BidStatus.DriverAcknowledged;
                    order.AcceptedBid.DriverAcknowledgedAt = DateTime.UtcNow;
                }
                else
                {
                    // Driver declined, reopen for bidding
                    order.Status = CargoOrderStatus.OpenForBidding;
                    order.AcceptedBid.Status = BidStatus.DriverDeclined;
                    order.AcceptedBidId = null;
                    order.PickupDateTime = null;
                }

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success(
                    acknowledgementDto.IsAcknowledged ?
                        "Driver acknowledged successfully" :
                        "Order reopened for bidding",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }


        public async Task<ApiResponseModel<CargoOrderResponseModel>> GetCargoOrderByIdAsync(string orderId)
        {
            try
            {
                var cargoOrder = await _dbContext.Set<CargoOrders>()
     .Include(o => o.CargoOwner)
     .Include(o => o.Items)
     .Include(o => o.Bids)
         .ThenInclude(b => b.Truck)
             .ThenInclude(t => t.Driver)
     .Include(o => o.AcceptedBid)
         .ThenInclude(b => b.Truck)
             .ThenInclude(t => t.Driver)
     .AsSplitQuery()
     .FirstOrDefaultAsync(o => o.Id == orderId);

                if (cargoOrder == null)
                {
                    return ApiResponseModel<CargoOrderResponseModel>.Fail("Cargo order not found", 404);
                }

                var orderResponse = _mapper.Map<CargoOrderResponseModel>(cargoOrder);

                // Calculate order summary
                var summary = await GetOrderSummary(cargoOrder);
                orderResponse.TotalWeight = summary.TotalWeight;
                orderResponse.TotalVolume = summary.TotalVolume;
                orderResponse.HasFragileItems = summary.HasFragileItems;
                orderResponse.ItemTypeBreakdown = summary.ItemTypeBreakdown;

                // Set the Driver property if an accepted bid exists and has a driver
                if (cargoOrder.AcceptedBid != null &&
                    cargoOrder.AcceptedBid.Truck != null &&
                    cargoOrder.AcceptedBid.Truck.Driver != null)
                {
                    orderResponse.Driver = _mapper.Map<DriverProfileResponseModel>(cargoOrder.AcceptedBid.Truck.Driver);
                }
                return ApiResponseModel<CargoOrderResponseModel>.Success(
                    "Cargo order retrieved successfully",
                    orderResponse,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<CargoOrderResponseModel>.Fail($"Error: {ex.Message}", 500);
            }
        }
        public async Task<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>> GetOpenCargoOrdersAsync(string? driverId = null)
        {
            try
            {
                // Get orders that are open for bidding or in bidding progress
                var openOrders = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.CargoOwner)
                    .Include(o => o.Items)
                    .Include(o => o.Bids.Where(b =>
                        driverId != null &&
                        b.Truck.DriverId == driverId)) // Only include the driver's own bids
                    .Where(o =>
                        (o.Status == CargoOrderStatus.OpenForBidding ||
                         o.Status == CargoOrderStatus.BiddingInProgress) &&
                        o.AcceptedBidId == null)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                if (!openOrders.Any())
                {
                    return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Success(
                        "No open cargo orders found",
                        Enumerable.Empty<CargoOrderResponseModel>(),
                        200);
                }

                var orderResponses = new List<CargoOrderResponseModel>();

                foreach (var order in openOrders)
                {
                    // Initialize collections to prevent null reference
                    order.Items ??= new List<CargoOrderItem>();
                    order.Documents ??= new List<string>();

                    var orderResponse = _mapper.Map<CargoOrderResponseModel>(order);

                    // Calculate and add summary information
                    var summary = await GetOrderSummary(order);
                    orderResponse.TotalWeight = summary.TotalWeight;
                    orderResponse.TotalVolume = summary.TotalVolume;
                    orderResponse.HasFragileItems = summary.HasFragileItems;
                    orderResponse.ItemTypeBreakdown = summary.ItemTypeBreakdown;
                    orderResponse.SpecialHandlingRequirements = summary.SpecialHandlingRequirements;

                    // If driverId is provided, only show their own bid information
                    if (!string.IsNullOrEmpty(driverId))
                    {
                        var driver = await _dbContext.Set<Driver>()
                            .Include(d => d.Truck)
                            .FirstOrDefaultAsync(d => d.Id == driverId);

                        if (driver?.Truck != null)
                        {
                            var driverBid = order.Bids.FirstOrDefault(b =>
                                b.TruckId == driver.Truck.Id &&
                                (b.Status == BidStatus.Pending || b.Status == BidStatus.AdminApproved));

                            // Only include the driver's own bid information
                            orderResponse.DriverBidInfo = driverBid != null ? new DriverBidInfo
                            {
                                BidId = driverBid.Id,
                                Amount = driverBid.Amount,
                                Status = driverBid.Status,
                                CreatedAt = driverBid.CreatedAt,
                                UpdatedAt = driverBid.UpdatedAt
                            } : null;
                        }
                    }

                    // Clear the bids list as it shouldn't be exposed to drivers
                    orderResponse.Bids = null;

                    orderResponses.Add(orderResponse);
                }

                return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Success(
                    "Open cargo orders retrieved successfully",
                    orderResponses,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Fail(
                    $"Error retrieving open cargo orders: {ex.Message}",
                    500);
            }
        }
        public async Task<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>> GetAcceptedOrdersForDriverAsync(string driverId)
        {
            try
            {
                // First get the driver with their truck information
                var driver = await _dbContext.Set<Driver>()
                    .Include(d => d.Truck)
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Fail(
                        "Driver not found",
                        404);
                }

                if (driver.Truck == null)
                {
                    return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Fail(
                        "No truck assigned to this driver",
                        404);
                }

                // Get active orders for the driver
                var activeOrders = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.CargoOwner)
                    .Include(o => o.Items)
                    .Include(o => o.AcceptedBid)
                    .Where(o =>
                        o.AcceptedBid != null &&
                        o.AcceptedBid.TruckId == driver.Truck.Id &&
                        (o.Status == CargoOrderStatus.DriverSelected ||
                         o.Status == CargoOrderStatus.DriverAcknowledged ||
                         o.Status == CargoOrderStatus.ReadyForPickup ||
                         o.Status == CargoOrderStatus.InTransit))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                if (!activeOrders.Any())
                {
                    return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Success(
                        "No active orders found for this driver",
                        Enumerable.Empty<CargoOrderResponseModel>(),
                        200);
                }

                var orderResponses = new List<CargoOrderResponseModel>();

                foreach (var order in activeOrders)
                {
                    // Initialize collections to prevent null reference
                    order.Items ??= new List<CargoOrderItem>();
                    order.Documents ??= new List<string>();
                    order.DeliveryDocuments ??= new List<string>();

                    var orderResponse = _mapper.Map<CargoOrderResponseModel>(order);

                    // Calculate and add summary information
                    var summary = await GetOrderSummary(order);
                    orderResponse.TotalWeight = summary.TotalWeight;
                    orderResponse.TotalVolume = summary.TotalVolume;
                    orderResponse.HasFragileItems = summary.HasFragileItems;
                    orderResponse.ItemTypeBreakdown = summary.ItemTypeBreakdown;
                    orderResponse.SpecialHandlingRequirements = summary.SpecialHandlingRequirements;

                    // Add next required action based on status
                    orderResponse.NextAction = GetNextRequiredAction(order.Status);

                    // Add bid/payment information
                    // Add bid/payment information
                    if (order.AcceptedBid != null)
                    {
                        orderResponse.AcceptedAmount = order.AcceptedBid.Amount;

                        // Set the Driver property if the bid has a driver
                        if (order.AcceptedBid.Truck != null && order.AcceptedBid.Truck.Driver != null)
                        {
                            orderResponse.Driver = _mapper.Map<DriverProfileResponseModel>(order.AcceptedBid.Truck.Driver);
                        }
                    }

                    orderResponses.Add(orderResponse);
                }

                return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Success(
                    "Active orders retrieved successfully",
                    orderResponses,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Fail(
                    $"Error retrieving active orders: {ex.Message}",
                    500);
            }
        }

        private async Task<CargoOrderSummaryModel> GetOrderSummary(CargoOrders order)
        {
            // Get all special handling instructions across items
            var specialHandlingInstructions = order.Items
                .Where(i => !string.IsNullOrEmpty(i.SpecialHandlingInstructions))
                .Select(i => i.SpecialHandlingInstructions)
                .Distinct()
                .ToList();

            var summary = new CargoOrderSummaryModel
            {
                TotalWeight = order.Items.Sum(i => i.Weight * i.Quantity),
                TotalVolume = order.Items.Sum(i => i.Length * i.Width * i.Height * i.Quantity),
                HasFragileItems = order.Items.Any(i => i.IsFragile),
                ItemTypeBreakdown = order.Items
                    .GroupBy(i => i.Type)
                    .ToDictionary(g => g.Key, g => g.Count()),
                SpecialHandlingRequirements = specialHandlingInstructions
            };

            return summary;
        }

        private string GetNextRequiredAction(CargoOrderStatus status)
        {
            return status switch
            {
                CargoOrderStatus.DriverSelected => "Acknowledge order selection and confirm availability",
                CargoOrderStatus.DriverAcknowledged => "Wait for pickup time",
                CargoOrderStatus.ReadyForPickup => "Proceed to pickup location and upload manifest after loading",
                CargoOrderStatus.InTransit => "Deliver cargo and upload delivery documents",
                _ => string.Empty
            };
        }

        public async Task<ApiResponseModel<bool>> UploadManifestAsync(UploadManifestDto uploadManifestDto)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                    .FirstOrDefaultAsync(o => o.Id == uploadManifestDto.OrderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                // Verify order status
                if (order.Status != CargoOrderStatus.ReadyForPickup)
                {
                    return ApiResponseModel<bool>.Fail("Order is not ready for pickup", 400);
                }

                // Validate delivery documents
                if (uploadManifestDto.ManifestUrl == null || !uploadManifestDto.ManifestUrl.Any())
                {
                    return ApiResponseModel<bool>.Fail("Delivery documents are required", 400);
                }

                // Update order with manifest and change status
                order.Documents = uploadManifestDto.ManifestUrl;
                order.Status = CargoOrderStatus.InTransit;
                order.ActualPickupDateTime = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Manifest uploaded and order is now in transit", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<bool>> UpdateLocationAsync(UpdateLocationDto updateLocationDto)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                    .FirstOrDefaultAsync(o => o.Id == updateLocationDto.OrderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                if (order.Status != CargoOrderStatus.InTransit)
                {
                    return ApiResponseModel<bool>.Fail("Order is not in transit", 400);
                }

                // Create new location update
                var locationUpdate = new DeliveryLocationUpdate
                {
                    OrderId = order.Id,
                    Latitude = updateLocationDto.Latitude,
                    Longitude = updateLocationDto.Longitude,
                    Timestamp = DateTime.UtcNow,
                    EstimatedTimeOfArrival = updateLocationDto.EstimatedTimeOfArrival,
                    CurrentLocation = updateLocationDto.CurrentLocation
                };

                _dbContext.Set<DeliveryLocationUpdate>().Add(locationUpdate);
                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Location updated successfully", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<bool>> CompleteDeliveryAsync(CompleteDeliveryDto completeDeliveryDto)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                    .FirstOrDefaultAsync(o => o.Id == completeDeliveryDto.OrderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                if (order.Status != CargoOrderStatus.InTransit)
                {
                    return ApiResponseModel<bool>.Fail("Order is not in transit", 400);
                }

                // Validate delivery documents
                if (completeDeliveryDto.DeliveryDocuments == null || !completeDeliveryDto.DeliveryDocuments.Any())
                {
                    return ApiResponseModel<bool>.Fail("Delivery documents are required", 400);
                }

                // Update order with delivery documents and change status
                order.DeliveryDocuments = completeDeliveryDto.DeliveryDocuments;
                order.Status = CargoOrderStatus.Delivered;
                order.DeliveryDateTime = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Delivery completed successfully", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<List<DeliveryLocationUpdate>>> GetDeliveryUpdatesAsync(string orderId)
        {
            try
            {
                var locationUpdates = await _dbContext.Set<DeliveryLocationUpdate>()
                    .Where(u => u.OrderId == orderId)
                    .OrderByDescending(u => u.Timestamp)
                    .ToListAsync();

                return ApiResponseModel<List<DeliveryLocationUpdate>>.Success(
                    "Location updates retrieved successfully",
                    locationUpdates,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<List<DeliveryLocationUpdate>>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<bool>> StartOrderAsync(StartOrderDto startOrderDto)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                    .FirstOrDefaultAsync(o => o.Id == startOrderDto.OrderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                // Verify the order is in DriverAcknowledged status
                if (order.Status != CargoOrderStatus.DriverAcknowledged)
                {
                    return ApiResponseModel<bool>.Fail("Order must be in acknowledged state to start", 400);
                }

                // Verify this is the assigned driver
                var driver = await _dbContext.Set<Driver>()
                    .Include(d => d.Truck)
                    .FirstOrDefaultAsync(d => d.Id == startOrderDto.DriverId);

                if (driver?.Truck == null || order.AcceptedBid?.TruckId != driver.Truck.Id)
                {
                    return ApiResponseModel<bool>.Fail("Driver is not assigned to this order", 400);
                }

                // Update order status to ReadyForPickup
                order.Status = CargoOrderStatus.ReadyForPickup;
                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Order is now ready for pickup", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }
        public async Task<ApiResponseModel<IEnumerable<CargoOrderResponseModel>>> GetCompletedOrdersForDriverAsync(string driverId)
        {
            try
            {
                // First get the driver with their truck information
                var driver = await _dbContext.Set<Driver>()
                    .Include(d => d.Truck)
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Fail(
                        "Driver not found",
                        404);
                }

                if (driver.Truck == null)
                {
                    return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Fail(
                        "No truck assigned to this driver",
                        404);
                }

                // Get completed orders for the driver
                var completedOrders = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.CargoOwner)
                    .Include(o => o.Items)
                    .Include(o => o.AcceptedBid)
                    .Where(o =>
                        o.AcceptedBid != null &&
                        o.AcceptedBid.TruckId == driver.Truck.Id &&
                        (o.Status == CargoOrderStatus.Delivered))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                if (!completedOrders.Any())
                {
                    return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Success(
                        "No completed orders found for this driver",
                        Enumerable.Empty<CargoOrderResponseModel>(),
                        200);
                }

                var orderResponses = new List<CargoOrderResponseModel>();

                foreach (var order in completedOrders)
                {
                    // Initialize collections to prevent null reference
                    order.Items ??= new List<CargoOrderItem>();
                    order.Documents ??= new List<string>();
                    order.DeliveryDocuments ??= new List<string>();

                    var orderResponse = _mapper.Map<CargoOrderResponseModel>(order);

                    // Calculate and add summary information
                    var summary = await GetOrderSummary(order);
                    orderResponse.TotalWeight = summary.TotalWeight;
                    orderResponse.TotalVolume = summary.TotalVolume;
                    orderResponse.HasFragileItems = summary.HasFragileItems;
                    orderResponse.ItemTypeBreakdown = summary.ItemTypeBreakdown;
                    orderResponse.SpecialHandlingRequirements = summary.SpecialHandlingRequirements;

                    // Add bid/payment information
                    if (order.AcceptedBid != null)
                    {
                        orderResponse.AcceptedAmount = order.AcceptedBid.Amount;

                        // Set the Driver property if the bid has a driver
                        if (order.AcceptedBid.Truck != null && order.AcceptedBid.Truck.Driver != null)
                        {
                            orderResponse.Driver = _mapper.Map<DriverProfileResponseModel>(order.AcceptedBid.Truck.Driver);
                        }
                    }

                    orderResponses.Add(orderResponse);
                }

                return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Success(
                    "Completed orders retrieved successfully",
                    orderResponses,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<IEnumerable<CargoOrderResponseModel>>.Fail(
                    $"Error retrieving completed orders: {ex.Message}",
                    500);
            }
        }
        // Add to CargoOrderService
        public async Task<ApiResponseModel<DriverSummaryResponseModel>> GetDriverSummaryAsync(string driverId)
        {
            try
            {
                // Get driver and truck info
                var driver = await _dbContext.Set<Driver>()
                    .Include(d => d.Truck)
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<DriverSummaryResponseModel>.Fail(
                        "Driver not found",
                        404);
                }

                if (driver.Truck == null)
                {
                    return ApiResponseModel<DriverSummaryResponseModel>.Fail(
                        "No truck assigned to this driver",
                        404);
                }

                // Get all completed orders for calculations
                var completedOrders = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                    .Where(o =>
                        o.AcceptedBid != null &&
                        o.AcceptedBid.TruckId == driver.Truck.Id &&
                        o.Status == CargoOrderStatus.Delivered)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                // Calculate weekly stats
                var weeklyStats = new WeeklyStats
                {
                    CompletedTrips = completedOrders.Count(o =>
                        o.DeliveryDateTime >= startOfWeek),
                    Earnings = completedOrders
                        .Where(o => o.DeliveryDateTime >= startOfWeek)
                        .Sum(o => o.AcceptedBid.Amount),
                    DailyTrips = completedOrders
                        .Where(o => o.DeliveryDateTime >= startOfWeek)
                        .GroupBy(o => o.DeliveryDateTime.Value.Date)
                        .Select(g => new DailyTrip
                        {
                            Date = g.Key,
                            TripCount = g.Count(),
                            Earnings = g.Sum(o => o.AcceptedBid.Amount)
                        })
                        .OrderBy(d => d.Date)
                        .ToList()
                };

                // Calculate monthly stats
                var monthlyStats = new MonthlyStats
                {
                    CompletedTrips = completedOrders.Count(o =>
                        o.DeliveryDateTime >= startOfMonth),
                    Earnings = completedOrders
                        .Where(o => o.DeliveryDateTime >= startOfMonth)
                        .Sum(o => o.AcceptedBid.Amount),
                    WeeklyTrips = GetWeeklyTrips(completedOrders, startOfMonth)
                };

                // Calculate total stats
                var summary = new DriverSummaryResponseModel
                {
                    WeeklyStats = weeklyStats,
                    MonthlyStats = monthlyStats,
                    TotalEarnings = completedOrders.Sum(o => o.AcceptedBid.Amount),
                    TotalTripsCompleted = completedOrders.Count,
                    AverageRating = 0
                    // AverageRating = await CalculateDriverAverageRating(driverId)
                };

                return ApiResponseModel<DriverSummaryResponseModel>.Success(
                    "Driver summary retrieved successfully",
                    summary,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<DriverSummaryResponseModel>.Fail(
                    $"Error retrieving driver summary: {ex.Message}",
                    500);
            }
        }

        private List<WeeklyTrip> GetWeeklyTrips(List<CargoOrders> orders, DateTime startOfMonth)
        {
            return orders
                .Where(o => o.DeliveryDateTime >= startOfMonth)
                .GroupBy(o => GetWeekNumberOfMonth(o.DeliveryDateTime.Value))
                .Select(g => new WeeklyTrip
                {
                    WeekNumber = g.Key,
                    StartDate = GetStartDateOfWeek(startOfMonth, g.Key),
                    EndDate = GetEndDateOfWeek(startOfMonth, g.Key),
                    TripCount = g.Count(),
                    Earnings = g.Sum(o => o.AcceptedBid.Amount)
                })
                .OrderBy(w => w.WeekNumber)
                .ToList();
        }

        private int GetWeekNumberOfMonth(DateTime date)
        {
            DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            return (date.Day - 1) / 7 + 1;
        }

        private DateTime GetStartDateOfWeek(DateTime startOfMonth, int weekNumber)
        {
            return startOfMonth.AddDays((weekNumber - 1) * 7);
        }

        private DateTime GetEndDateOfWeek(DateTime startOfMonth, int weekNumber)
        {
            return startOfMonth.AddDays(weekNumber * 7 - 1);
        }

        public async Task<ApiResponseModel<bool>> UpdateBidAsync(UpdateBidDto updateBidDto)
        {
            try
            {
                // Find the bid to update
                var bid = await _dbContext.Set<Bid>()
                    .Include(b => b.Order)
                    .FirstOrDefaultAsync(b => b.Id == updateBidDto.BidId && b.OrderId == updateBidDto.OrderId);

                if (bid == null)
                {
                    return ApiResponseModel<bool>.Fail("Bid not found", 404);
                }

                // Check if the order is still in bidding process
                if (bid.Order.Status != CargoOrderStatus.OpenForBidding &&
                    bid.Order.Status != CargoOrderStatus.BiddingInProgress)
                {
                    return ApiResponseModel<bool>.Fail("Order is no longer accepting bid updates", 400);
                }

                // Check if the bid is still pending (not selected, expired, etc.)
                if (bid.Status != BidStatus.Pending)
                {
                    return ApiResponseModel<bool>.Fail("This bid can no longer be updated", 400);
                }

                // Check if the new amount is lower than the current amount
                if (updateBidDto.Amount >= bid.Amount)
                {
                    return ApiResponseModel<bool>.Fail("New bid amount must be lower than the current amount", 400);
                }

                // Update the bid
                bid.Amount = updateBidDto.Amount;

                // Update notes if provided
                // if (!string.IsNullOrEmpty(updateBidDto.Notes))
                // {
                //     bid.Notes = updateBidDto.Notes;
                // }

                bid.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Bid updated successfully", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<bool>> UpdateOrderPaymentStatusAsync(string orderId, string paymentIntentId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.Bids)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                // Verify this is a Shipper order with a pending payment
                if (order.Status != CargoOrderStatus.BiddingInProgress || order.AcceptedBidId == null)
                {
                    return ApiResponseModel<bool>.Fail("Order is not in the correct state for payment", 400);
                }

                // Update payment details
                order.PaymentIntentId = paymentIntentId;
                order.PaymentDate = DateTime.UtcNow;
                order.IsPaid = true;

                // Now that payment is confirmed, update the order status
                order.Status = CargoOrderStatus.DriverSelected;

                // Mark other bids as expired
                foreach (var bid in order.Bids.Where(b => b.Id != order.AcceptedBidId))
                {
                    bid.Status = BidStatus.Expired;
                }

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success("Payment confirmed and order updated", true, 200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

        // private async Task<decimal> CalculateDriverAverageRating(string driverId)
        // {
        //     var ratings = await _dbContext.Set<DriverRating>()
        //         .Where(r => r.DriverId == driverId)
        //         .Select(r => r.Rating)
        //         .ToListAsync();

        //     if (!ratings.Any())
        //         return 0;

        //     return ratings.Average();
        // }
    }

}
