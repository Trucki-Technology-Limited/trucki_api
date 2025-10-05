using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using trucki.Models.ResponseModels;
using trucki.Entities;
using Microsoft.EntityFrameworkCore;
using trucki.Interfaces.IServices;
using trucki.DatabaseContext;
using trucki.Models.RequestModel;
using AutoMapper;
using trucki.Interfaces.IRepository; // If you're using EF Core

namespace trucki.Services
{
    public class CargoOrderService : ICargoOrderService
    {
        private readonly TruckiDBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IStripeService _stripeService;
        private readonly INotificationService _notificationService;
        private readonly NotificationEventService _notificationEventService;
        private readonly IEmailService _emailService;
        private readonly IWalletService _walletService;
        private readonly IDriverWalletService _driverWalletService;
        private readonly IDriverService _driverService;
        private readonly IDriverRatingRepository _ratingRepository;

        public CargoOrderService(TruckiDBContext dbContext, IMapper mapper, IStripeService stripeService, INotificationService notificationService,
    IEmailService emailService, NotificationEventService notificationEventService, IWalletService walletService, IDriverWalletService driverWalletService, // Add this
        IDriverService driverService, IDriverRatingRepository ratingRepository)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
            _mapper = mapper;
            _notificationService = notificationService;
            _emailService = emailService;
            _notificationEventService = notificationEventService;
            _walletService = walletService;
            _driverWalletService = driverWalletService;
            _driverService = driverService;
            _ratingRepository = ratingRepository;
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
                    PickupContactName = createOrderDto.PickupContactName,
                    PickupContactPhone = createOrderDto.PickupContactPhone,
                    DeliveryContactName = createOrderDto.DeliveryContactName,
                    DeliveryContactPhone = createOrderDto.DeliveryContactPhone,
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
                await _notificationEventService.NotifyOrderCreated(
           newOrder.CargoOwnerId,
           newOrder.Id,
           newOrder.PickupLocation,
           newOrder.DeliveryLocation);
                if (createOrderDto.OpenForBidding)
                {
                    // Send notification to truck owners and drivers
                    await _notificationService.SendNotificationToTopicAsync(
                        "driver",
                        "New Cargo Order Available",
                        $"A new cargo order is available from {newOrder.PickupLocation} to {newOrder.DeliveryLocation}",
                        new Dictionary<string, string> {
                { "orderId", newOrder.Id },
                { "type", "new_order" }
                        }
                    );
                    await _notificationEventService.NotifyOrderOpenForBidding(
              newOrder.Id,
              newOrder.PickupLocation,
              newOrder.DeliveryLocation);
                }
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
                existingOrder.PickupContactName = updateOrderDto.PickupContactName;
                existingOrder.PickupContactPhone = updateOrderDto.PickupContactPhone;
                existingOrder.DeliveryContactName = updateOrderDto.DeliveryContactName;
                existingOrder.DeliveryContactPhone = updateOrderDto.DeliveryContactPhone;

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

                // Send notification to truck owners and drivers
                await _notificationService.SendNotificationToTopicAsync(
                    "driver",
                    "New Cargo Order Available",
                    $"A new cargo order is available from {order.PickupLocation} to {order.DeliveryLocation}",
                    new Dictionary<string, string> {
                { "orderId", order.Id },
                { "type", "new_order" }
                    }
                );
                await _notificationEventService.NotifyOrderOpenForBidding(
         order.Id,
         order.PickupLocation,
         order.DeliveryLocation);

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
                    .Include(a => a.CargoOwner)
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

                var hasActiveOrder = await DriverHasActiveOrderAsync(driver.Id);
                if (hasActiveOrder)
                {
                    return ApiResponseModel<bool>.Fail("Driver already has an active order and cannot place another bid until it's completed", 400);
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
                // Notify cargo owner about the new bid
                await _notificationService.SendNotificationAsync(
                    order.CargoOwner.UserId,
                    "New Bid Received",
                    $"You received a new bid for your cargo order to {order.DeliveryLocation}",
                    new Dictionary<string, string> {
                { "orderId", order.Id },
                { "bidId", existingBid?.Id },
                { "type", "new_bid" }
                    }
                );
                await _notificationEventService.NotifyBidSubmitted(
           order.CargoOwnerId,
           order.Id,
           driver.Name,
           createBidDto.Amount);
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

                // Calculate fee components for both shipper and broker
                var bidAmount = selectedBid.Amount;
                var systemFee = bidAmount * 0.15m; // 15% system fee
                var subtotal = bidAmount + systemFee;
                var totalAmount = subtotal;

                // Store these values in the order
                order.TotalAmount = bidAmount;
                order.SystemFee = systemFee;

                // Set the accepted bid
                order.AcceptedBidId = selectedBid.Id;
                selectedBid.Status = BidStatus.CargoOwnerSelected;

                // Generate invoice number - common for both shipper and broker
                string invoiceNumber = GenerateInvoiceNumber();
                order.InvoiceNumber = invoiceNumber;

                // Create invoice for both shipper and broker
                var invoice = new Invoice
                {
                    OrderId = order.Id,
                    InvoiceNumber = invoiceNumber,
                    SubTotal = bidAmount,
                    SystemFee = systemFee,
                    Tax = 0,
                    TotalAmount = totalAmount,
                    // Different due dates based on cargo owner type
                    DueDate = order.CargoOwner.OwnerType == CargoOwnerType.Broker
                        ? DateTime.UtcNow.AddDays(14) // 14 days for brokers
                        : DateTime.UtcNow.AddDays(2),  // 2 days for shippers
                    Status = InvoiceStatus.Pending
                };

                _dbContext.Set<Invoice>().Add(invoice);

                // Get the cargo owner's wallet balance
                var walletBalance = await _walletService.GetWalletBalance(order.CargoOwnerId);

                // Different flows based on cargo owner type
                if (order.CargoOwner.OwnerType == CargoOwnerType.Shipper)
                {
                    // Determine payment breakdown
                    var walletPaymentAmount = Math.Min(walletBalance, totalAmount);
                    var remainingPaymentAmount = totalAmount - walletPaymentAmount;

                    // Store wallet payment amount
                    if (walletPaymentAmount > 0)
                    {
                        order.WalletPaymentAmount = walletPaymentAmount;
                    }

                    // If payment is fully covered by wallet
                    if (remainingPaymentAmount <= 0)
                    {
                        // Process wallet payment
                        var walletResult = await _walletService.DeductFundsFromWallet(
                            order.CargoOwnerId,
                            walletPaymentAmount,
                            $"Payment for Order #{order.Id}",
                            WalletTransactionType.Payment,
                            order.Id);

                        if (!walletResult.IsSuccessful)
                        {
                            return ApiResponseModel<StripePaymentResponse>.Fail(
                                walletResult.Message,
                                walletResult.StatusCode);
                        }

                        // Update order status
                        order.Status = CargoOrderStatus.DriverSelected;
                        order.PaymentMethod = PaymentMethodType.Wallet;
                        order.IsPaid = true;
                        order.PaymentDate = DateTime.UtcNow;

                        // Mark other bids as expired
                        foreach (var bid in order.Bids.Where(b => b.Id != selectedBid.Id))
                        {
                            bid.Status = BidStatus.Expired;
                        }

                        // Save changes
                        await _dbContext.SaveChangesAsync();

                        // Return response without Stripe payment intent
                        var walletResponse = new StripePaymentResponse
                        {
                            OrderId = order.Id,
                            Amount = totalAmount,
                            Status = "succeeded",
                            PaymentBreakdown = new PaymentBreakdown
                            {
                                BidAmount = bidAmount,
                                SystemFee = systemFee,
                                Tax = 0,
                                TotalAmount = totalAmount,
                                WalletAmount = walletPaymentAmount,
                                RemainingAmount = 0
                            }
                        };

                        // Notify driver about selection
                        var driver = await _dbContext.Drivers
                            .FirstOrDefaultAsync(d => d.Truck.Id == selectedBid.TruckId);

                        if (driver?.UserId != null)
                        {
                            await _notificationService.SendNotificationAsync(
                                driver.UserId,
                                "Bid Accepted",
                                $"Your bid for the order to {order.DeliveryLocation} has been accepted",
                                new Dictionary<string, string> {
                            { "orderId", order.Id },
                            { "bidId", selectedBid.Id },
                            { "type", "bid_accepted" }
                                }
                            );
                            await _notificationEventService.NotifyBidAccepted(
                       driver.Id,
                       order.Id,
                       order.PickupLocation,
                       order.DeliveryLocation
                   );
                        }

                        return ApiResponseModel<StripePaymentResponse>.Success(
                            "Payment processed from wallet balance",
                            walletResponse,
                            200);
                    }
                    else
                    {
                        // Create Stripe payment for remaining amount
                        var paymentResponse = await _stripeService.CreatePaymentIntent(
                            order.Id,
                            remainingPaymentAmount,
                            "usd");

                        // Set partial payment info
                        order.PaymentMethod = PaymentMethodType.Mixed;
                        order.WalletPaymentAmount = walletPaymentAmount;
                        order.StripePaymentAmount = remainingPaymentAmount;
                        order.PaymentIntentId = paymentResponse.PaymentIntentId;

                        // Save changes but don't update order status yet
                        await _dbContext.SaveChangesAsync();

                        // Add payment breakdown
                        paymentResponse.PaymentBreakdown = new PaymentBreakdown
                        {
                            BidAmount = bidAmount,
                            SystemFee = systemFee,
                            Tax = 0,
                            TotalAmount = totalAmount,
                            WalletAmount = walletPaymentAmount,
                            RemainingAmount = remainingPaymentAmount
                        };

                        return ApiResponseModel<StripePaymentResponse>.Success(
                            walletPaymentAmount > 0
                                ? $"${walletPaymentAmount:F2} applied from wallet balance. Please complete remaining payment."
                                : "Please complete payment",
                            paymentResponse,
                            200);
                    }
                }
                else // Broker
                {
                    // For Brokers, update order status immediately
                    order.Status = CargoOrderStatus.DriverSelected;
                    order.PaymentMethod = PaymentMethodType.Invoice;

                    // Mark other bids as expired
                    foreach (var bid in order.Bids.Where(b => b.Id != selectedBid.Id))
                    {
                        bid.Status = BidStatus.Expired;
                    }

                    await _dbContext.SaveChangesAsync();

                    // Notify driver about selection
                    var driver = await _dbContext.Drivers
                        .FirstOrDefaultAsync(d => d.Truck.Id == selectedBid.TruckId);

                    if (driver?.UserId != null)
                    {
                        await _notificationService.SendNotificationAsync(
                            driver.UserId,
                            "Bid Accepted",
                            $"Your bid for cargo order to {order.DeliveryLocation} has been accepted",
                            new Dictionary<string, string> {
                        { "orderId", order.Id },
                        { "bidId", selectedBid.Id },
                        { "type", "bid_accepted" }
                            }
                        );
                        await _notificationEventService.NotifyBidAccepted(
           driver.Id,
           order.Id,
           order.PickupLocation,
           order.DeliveryLocation
       );
                    }

                    // Return empty payment response with breakdown info
                    var emptyPaymentResponse = new StripePaymentResponse
                    {
                        OrderId = order.Id,
                        PaymentBreakdown = new PaymentBreakdown
                        {
                            BidAmount = bidAmount,
                            SystemFee = systemFee,
                            Tax = 0,
                            TotalAmount = totalAmount,
                            WalletAmount = 0,
                            RemainingAmount = 0
                        }
                    };

                    return ApiResponseModel<StripePaymentResponse>.Success(
                        "Driver selected and invoice created successfully",
                        emptyPaymentResponse,
                        200);
                }
            }
            catch (Exception ex)
            {
                return ApiResponseModel<StripePaymentResponse>.Fail($"Error: {ex.Message}", 500);
            }
        }
        private string GenerateInvoiceNumber()
        {
            // Format: INV-YYYYMMDD-XXXX
            var dateString = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random();
            var randomPart = random.Next(1000, 9999).ToString();
            return $"INV-{dateString}-{randomPart}";
        }

        public async Task<ApiResponseModel<bool>> DriverAcknowledgeBidAsync(DriverAcknowledgementDto acknowledgementDto)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(a => a.CargoOwner)
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .FirstOrDefaultAsync(o => o.Id == acknowledgementDto.OrderId);

                if (order == null || order.AcceptedBid == null)
                {
                    return ApiResponseModel<bool>.Fail("Order or accepted bid not found", 404);
                }

                if (order.Status != CargoOrderStatus.DriverSelected)
                {
                    return ApiResponseModel<bool>.Fail("Order is not in driver selection state", 400);
                }

                // Get the driver info
                var truckId = order.AcceptedBid.TruckId;
                var driver = await _dbContext.Set<Driver>()
                    .FirstOrDefaultAsync(d => d.TruckId == truckId);
                string driverName = "Driver"; // Default fallback

                if (driver == null)
                {
                    return ApiResponseModel<bool>.Fail("Driver not found for this truck", 404);
                }

                if (acknowledgementDto.IsAcknowledged)
                {
                    // Check if the driver already has an active order
                    var hasActiveOrder = await DriverHasActiveOrderAsync(driver.Id, order.Id);
                    if (hasActiveOrder)
                    {
                        return ApiResponseModel<bool>.Fail("Driver already has an active order and cannot accept another one until it's completed", 400);
                    }

                    order.Status = CargoOrderStatus.DriverAcknowledged;
                    order.AcceptedBid.Status = BidStatus.DriverAcknowledged;
                    order.AcceptedBid.DriverAcknowledgedAt = DateTime.UtcNow;
                }
                else
                {
                    // Driver declined, reopen for bidding
                    order.Status = CargoOrderStatus.OpenForBidding;
                    // Store the driver name before nulling the accepted bid

                    if (order.AcceptedBid?.Truck?.Driver != null &&
                        !string.IsNullOrEmpty(order.AcceptedBid.Truck.Driver.Name))
                    {
                        driverName = order.AcceptedBid.Truck.Driver.Name;
                    }
                    if (order.AcceptedBid != null)
                    {
                        order.AcceptedBid.Status = BidStatus.DriverDeclined;
                    }
                    order.AcceptedBidId = null;
                    order.PickupDateTime = null;

                    // If the order was paid, refund the payment to the cargo owner's wallet
                    if (order.IsPaid)
                    {
                        decimal refundAmount = 0;

                        // Calculate refund amount based on payment method
                        if (order.PaymentMethod == PaymentMethodType.Stripe)
                        {
                            refundAmount = order.TotalAmount;
                        }
                        else if (order.PaymentMethod == PaymentMethodType.Mixed)
                        {
                            refundAmount = order.WalletPaymentAmount.GetValueOrDefault() + order.StripePaymentAmount.GetValueOrDefault();
                        }
                        else if (order.PaymentMethod == PaymentMethodType.Wallet)
                        {
                            refundAmount = order.WalletPaymentAmount.GetValueOrDefault();
                        }

                        if (refundAmount > 0)
                        {
                            // Add funds to wallet
                            await _walletService.AddFundsToWallet(
                                order.CargoOwnerId,
                                refundAmount,
                                $"Refund for Order #{order.Id} - Driver declined",
                                WalletTransactionType.Refund,
                                order.Id);

                            // Reset payment fields
                            order.IsPaid = false;
                            order.PaymentIntentId = null;
                            order.PaymentDate = null;
                            order.WalletPaymentAmount = null;
                            order.StripePaymentAmount = null;
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();

                if (acknowledgementDto.IsAcknowledged)
                {
                    // Notify cargo owner that driver acknowledged the order
                    await _notificationService.SendNotificationAsync(
                        order.CargoOwner.UserId,
                        "Driver Accepted Order",
                        $"The driver has accepted your order to {order.DeliveryLocation}",
                        new Dictionary<string, string> {
                { "orderId", order.Id },
                { "type", "driver_accepted" }
                        }
                    );

                    await _notificationEventService.NotifyDriverAcknowledged(
                        order.CargoOwner.Id,
                        order.Id,
                        order.AcceptedBid.Truck.Driver.Name);

                    return ApiResponseModel<bool>.Success(
                        "Driver acknowledged successfully",
                        true,
                        200);
                }
                else
                {
                    // Notify cargo owner that driver declined
                    await _notificationService.SendNotificationAsync(
                        order.CargoOwner.UserId,
                        "Driver Declined Order",
                        $"The driver has declined your order to {order.DeliveryLocation}. The order is now reopened for bidding.",
                        new Dictionary<string, string> {
                { "orderId", order.Id },
                { "type", "driver_declined" }
                        }
                    );

                    await _notificationEventService.NotifyDriverDeclined(
                          order.CargoOwner.Id,
                        order.Id,
                       driverName);

                    // If payment was refunded to wallet, include that information
                    string message = "Order reopened for bidding";
                    if (order.IsPaid)
                    {
                        message = $"Order reopened for bidding. Payment refunded to your wallet.";
                    }

                    return ApiResponseModel<bool>.Success(
                        message,
                        true,
                        200);
                }
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
                // Include bids with driver ratings ONLY when order is in bidding progress
                if (cargoOrder.Status == CargoOrderStatus.BiddingInProgress && orderResponse.Bids != null && orderResponse.Bids.Any())
                {
                    foreach (var bid in orderResponse.Bids)
                    {
                        if (bid.Driver != null)
                        {
                            // Get driver rating summary for each bidding driver
                            var ratingResult = await _ratingRepository.GetDriverRatingSummaryAsync(bid.Driver.Id);
                            if (ratingResult.IsSuccessful && ratingResult.Data != null)
                            {
                                bid.DriverRating = ratingResult.Data;
                                bid.Driver.Rating = ratingResult.Data;
                            }
                            else
                            {
                                // Default rating for drivers with no ratings
                                var defaultRating = new DriverRatingSummaryModel
                                {
                                    AverageRating = 0,
                                    TotalRatings = 0,
                                    RatingBreakdown = new Dictionary<int, int>
                            {
                                { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                            }
                                };
                                bid.DriverRating = defaultRating;
                                bid.Driver.Rating = defaultRating;
                            }
                        }
                    }
                }
                else if (cargoOrder.Status != CargoOrderStatus.BiddingInProgress)
                {
                    // Clear bids for orders not in bidding progress (existing behavior)
                    orderResponse.Bids = null;
                }
                // Set the Driver property if an accepted bid exists and has a driver
                if (cargoOrder.AcceptedBid != null &&
                    cargoOrder.AcceptedBid.Truck != null &&
                    cargoOrder.AcceptedBid.Truck.Driver != null)
                {
                    orderResponse.Driver = _mapper.Map<DriverProfileResponseModel>(cargoOrder.AcceptedBid.Truck.Driver);
                }

                // Set contact information based on order status
                if (cargoOrder.Status == CargoOrderStatus.ReadyForPickup)
                {
                    // Return pickup contact info only
                    orderResponse.PickupContactName = cargoOrder.PickupContactName;
                    orderResponse.PickupContactPhone = cargoOrder.PickupContactPhone;
                    orderResponse.DeliveryContactName = null;
                    orderResponse.DeliveryContactPhone = null;
                }
                else if (cargoOrder.Status == CargoOrderStatus.InTransit)
                {
                    // Return delivery contact info only
                    orderResponse.PickupContactName = null;
                    orderResponse.PickupContactPhone = null;
                    orderResponse.DeliveryContactName = cargoOrder.DeliveryContactName;
                    orderResponse.DeliveryContactPhone = cargoOrder.DeliveryContactPhone;
                }
                else
                {
                    // For other statuses, don't return contact info to drivers
                    orderResponse.PickupContactName = null;
                    orderResponse.PickupContactPhone = null;
                    orderResponse.DeliveryContactName = null;
                    orderResponse.DeliveryContactPhone = null;
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

                    // Set contact information based on order status
                    if (order.Status == CargoOrderStatus.ReadyForPickup)
                    {
                        // Return pickup contact info only
                        orderResponse.PickupContactName = order.PickupContactName;
                        orderResponse.PickupContactPhone = order.PickupContactPhone;
                        orderResponse.DeliveryContactName = null;
                        orderResponse.DeliveryContactPhone = null;
                    }
                    else if (order.Status == CargoOrderStatus.InTransit)
                    {
                        // Return delivery contact info only
                        orderResponse.PickupContactName = null;
                        orderResponse.PickupContactPhone = null;
                        orderResponse.DeliveryContactName = order.DeliveryContactName;
                        orderResponse.DeliveryContactPhone = order.DeliveryContactPhone;
                    }
                    else
                    {
                        // For other statuses, don't return contact info to drivers
                        orderResponse.PickupContactName = null;
                        orderResponse.PickupContactPhone = null;
                        orderResponse.DeliveryContactName = null;
                        orderResponse.DeliveryContactPhone = null;
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
                    .Include(o => o.CargoOwner) // Add this line
                    .Include(o => o.AcceptedBid.Truck.Driver) // Add this line to get driver info
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

                // Add notification to cargo owner about pickup
                await _notificationEventService.NotifyOrderPickedUp(
                    order.CargoOwnerId,
                    order.Id,
                    order.AcceptedBid.Truck.Driver.Name);

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
                    .Include(o => o.CargoOwner) // Add this line
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

                // Add notification to cargo owner about location update
                await _notificationEventService.NotifyLocationUpdated(
                    order.CargoOwnerId,
                    order.Id,
                    updateLocationDto.CurrentLocation,
                    updateLocationDto.EstimatedTimeOfArrival ?? DateTime.UtcNow);

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
                    .Include(o => o.CargoOwner) // Add this line
                    .Include(o => o.AcceptedBid.Truck.Driver) // Add this line to get driver info
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

                // Credit the driver's wallet with the bid amount
                if (order.AcceptedBid != null)
                {
                    await _driverWalletService.CreditDriverForDeliveryAsync(
                        _driverService,
                        order.Id,
                        order.AcceptedBid.Truck.Driver.Id,
                        order.AcceptedBid.Amount,
                        $"Payment for delivery from {order.PickupLocation} to {order.DeliveryLocation}");
                }

                // Add notification to cargo owner about completed delivery
                await _notificationEventService.NotifyOrderDelivered(
                    order.CargoOwnerId,
                    order.Id,
                    order.AcceptedBid.Truck.Driver.Name);

                // Add notification to driver about delivery confirmation
                await _notificationEventService.NotifyDeliveryConfirmed(
                    order.AcceptedBid.Truck.Driver.Id,
                    order.Id,
                    order.PickupLocation,
                    order.DeliveryLocation);

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
                    .Include(o => o.CargoOwner) // Add this line
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

                // Add notification to cargo owner about the order being started
                await _notificationEventService.NotifyOrderStarted(
                    order.CargoOwnerId,
                    order.Id,
                    driver.Name);

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
                        o.AcceptedBid.TruckId == driver.Truck.Id 
                        &&
                        (o.Status == CargoOrderStatus.Delivered)
                        )
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
                        .Sum(o => (double)o.AcceptedBid.Amount), // Cast to double
                    DailyTrips = completedOrders
                        .Where(o => o.DeliveryDateTime >= startOfWeek)
                        .GroupBy(o => o.DeliveryDateTime.Value.Date)
                        .Select(g => new DailyTrip
                        {
                            Date = g.Key,
                            TripCount = g.Count(),
                            Earnings = g.Sum(o => (double)o.AcceptedBid.Amount) // Cast to double
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
                        .Sum(o => (double)o.AcceptedBid.Amount), // Cast to double
                    WeeklyTrips = GetWeeklyTrips(completedOrders, startOfMonth)
                };

                // Calculate total stats
                var summary = new DriverSummaryResponseModel
                {
                    WeeklyStats = weeklyStats,
                    MonthlyStats = monthlyStats,
                    TotalEarnings = completedOrders.Sum(o => (double)o.AcceptedBid.Amount), // Cast to double
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
                    Earnings = g.Sum(o => (double)o.AcceptedBid.Amount)
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

        // Main method that routes to appropriate handler
        public async Task<ApiResponseModel<bool>> UpdateOrderPaymentStatusAsync(string orderId, string paymentIntentId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(w => w.CargoOwner)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                // Route to appropriate payment handler based on owner type
                return order.CargoOwner.OwnerType switch
                {
                    CargoOwnerType.Shipper => await ProcessShipperPaymentAsync(orderId, paymentIntentId),
                    CargoOwnerType.Broker => await ProcessBrokerPaymentAsync(orderId, paymentIntentId),
                    _ => ApiResponseModel<bool>.Fail("Unknown cargo owner type", 400)
                };
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

        // Shipper payment processing (upfront payment during bid selection)
        private async Task<ApiResponseModel<bool>> ProcessShipperPaymentAsync(string orderId, string paymentIntentId)
        {
            var order = await _dbContext.Set<CargoOrders>()
                .Include(o => o.Bids)
                .Include(w => w.CargoOwner)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order.Status != CargoOrderStatus.BiddingInProgress || order.AcceptedBidId == null)
            {
                return ApiResponseModel<bool>.Fail("Order is not in the correct state for payment", 400);
            }

            var invoice = await _dbContext.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.OrderId == orderId);

            if (invoice == null)
            {
                return ApiResponseModel<bool>.Fail("Invoice not found for this order", 404);
            }

            // Process wallet portion for mixed payments
            if (order.PaymentMethod == PaymentMethodType.Mixed && order.WalletPaymentAmount > 0)
            {
                var walletResult = await _walletService.DeductFundsFromWallet(
                    order.CargoOwnerId,
                    order.WalletPaymentAmount.Value,
                    $"Partial payment for Order #{order.Id}",
                    WalletTransactionType.Payment,
                    order.Id);

                if (!walletResult.IsSuccessful)
                {
                    return ApiResponseModel<bool>.Fail(
                        $"Error processing wallet payment: {walletResult.Message}",
                        walletResult.StatusCode);
                }
            }

            // Update payment details
            order.PaymentIntentId = paymentIntentId;
            order.PaymentDate = DateTime.UtcNow;
            order.IsPaid = true;
            order.Status = CargoOrderStatus.DriverSelected;

            // Update invoice
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaymentApprovedAt = DateTime.UtcNow;

            // Mark other bids as expired
            foreach (var bid in order.Bids.Where(b => b.Id != order.AcceptedBidId))
            {
                bid.Status = BidStatus.Expired;
            }

            await _dbContext.SaveChangesAsync();
            await SendPaymentConfirmationAsync(order, invoice);

            return ApiResponseModel<bool>.Success("Shipper payment confirmed and order updated", true, 200);
        }

        // Broker payment processing (payment after delivery via invoice)
        private async Task<ApiResponseModel<bool>> ProcessBrokerPaymentAsync(string orderId, string paymentIntentId)
        {
            var order = await _dbContext.Set<CargoOrders>()
                .Include(w => w.CargoOwner)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            // Brokers can pay for delivered, completed, or in-transit orders
            var allowedStatuses = new[]
            {
        CargoOrderStatus.Delivered,
        CargoOrderStatus.InTransit
    };

            if (!allowedStatuses.Contains(order.Status))
            {
                return ApiResponseModel<bool>.Fail(
                    "Broker payments can only be processed for delivered, completed, or in-transit orders", 400);
            }

            if (order.AcceptedBidId == null)
            {
                return ApiResponseModel<bool>.Fail("No accepted bid found for this order", 400);
            }

            var invoice = await _dbContext.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.OrderId == orderId);

            if (invoice == null)
            {
                return ApiResponseModel<bool>.Fail("Invoice not found for this order", 404);
            }

            // Update payment details
            order.PaymentIntentId = paymentIntentId;
            order.PaymentDate = DateTime.UtcNow;
            order.IsPaid = true;
            order.PaymentMethod = PaymentMethodType.Stripe;

            // Update invoice
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaymentApprovedAt = DateTime.UtcNow;

            // Order remains in Delivered status after payment approval

            await _dbContext.SaveChangesAsync();
            await SendPaymentConfirmationAsync(order, invoice);

            return ApiResponseModel<bool>.Success("Broker payment confirmed and order updated", true, 200);
        }

        // Common method for sending payment confirmations
        private async Task SendPaymentConfirmationAsync(CargoOrders order, Invoice invoice)
        {
            // Send notification
            await _notificationService.SendNotificationAsync(
                order.CargoOwner.UserId,
                "Payment Successful",
                order.CargoOwner.OwnerType == CargoOwnerType.Broker
                    ? $"Your payment for Invoice #{invoice.InvoiceNumber} was successful"
                    : $"Your payment for cargo order to {order.DeliveryLocation} was successful",
                new Dictionary<string, string> {
            { "orderId", order.Id },
            { "invoiceId", invoice.Id },
            { "type", "payment_success" }
                }
            );

            // Send email if available
            if (!string.IsNullOrEmpty(order.CargoOwner.EmailAddress))
            {
                await _emailService.SendPaymentReceiptEmailAsync(
                    order.CargoOwner.EmailAddress,
                    order.Id,
                    order.AcceptedBid?.Amount ?? order.TotalAmount,
                    order.SystemFee,
                    order.Tax,
                    invoice.TotalAmount,
                    "USD",
                    order.PickupLocation,
                    order.DeliveryLocation
                );
            }
        }
        private async Task<bool> DriverHasActiveOrderAsync(string driverId, string excludeOrderId = null)
        {
            try
            {
                // Get the driver's truck
                var driver = await _dbContext.Set<Driver>()
                    .Include(d => d.Truck)
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver?.Truck == null)
                    return false;

                // Get all active orders for this truck
                var query = _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                    .Where(o =>
                        o.AcceptedBid != null &&
                        o.AcceptedBid.TruckId == driver.Truck.Id &&
                        (
                         // o.Status == CargoOrderStatus.DriverSelected ||
                         o.Status == CargoOrderStatus.DriverAcknowledged ||
                         o.Status == CargoOrderStatus.ReadyForPickup ||
                         o.Status == CargoOrderStatus.InTransit));

                // Exclude the current order if specified
                if (!string.IsNullOrEmpty(excludeOrderId))
                {
                    query = query.Where(o => o.Id != excludeOrderId);
                }

                // Check if any active orders exist
                return await query.AnyAsync();
            }
            catch
            {
                // Error on the safe side - consider it as having an active order
                return true;
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

        public async Task<ApiResponseModel<PagedResponse<CargoOrderResponseModel>>> GetAllOrdersForDriverAsync(string driverId, GetDriverOrdersQueryDto query)
        {
            try
            {
                // First get the driver with their truck information
                var driver = await _dbContext.Set<Driver>()
                    .Include(d => d.Truck)
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<PagedResponse<CargoOrderResponseModel>>.Fail(
                        "Driver not found",
                        404);
                }

                if (driver.Truck == null)
                {
                    return ApiResponseModel<PagedResponse<CargoOrderResponseModel>>.Fail(
                        "No truck assigned to this driver",
                        404);
                }

                // Build the query for all orders for this driver
                var ordersQuery = _dbContext.Set<CargoOrders>()
                    .Include(o => o.CargoOwner)
                    .Include(o => o.Items)
                    .Include(o => o.AcceptedBid)
                    .Include(o => o.Bids.Where(b => b.TruckId == driver.Truck.Id))
                      .Where(o =>
                // 1. Orders where this driver has an accepted bid
                (o.AcceptedBid != null && o.AcceptedBid.TruckId == driver.Truck.Id) ||
                // 2. Orders where this driver has placed a bid (pending, accepted, or expired)
                o.Bids.Any(b => b.TruckId == driver.Truck.Id) ||
                // 3. Open orders that the driver hasn't bid on yet (NEW ADDITION)
                (o.Status == CargoOrderStatus.OpenForBidding ||
                 o.Status == CargoOrderStatus.BiddingInProgress) &&
                o.AcceptedBidId == null &&
                !o.Bids.Any(b => b.TruckId == driver.Truck.Id));

                // Apply status filter if provided
                if (query.Status.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);
                }

                // Apply date filters if provided
                if (query.StartDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt >= query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt <= query.EndDate.Value);
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    ordersQuery = ordersQuery.Where(o =>
                        o.PickupLocation.Contains(query.SearchTerm) ||
                        o.DeliveryLocation.Contains(query.SearchTerm) ||
                        o.CargoOwner.Name.Contains(query.SearchTerm));
                }

                // Apply sorting
                ordersQuery = query.SortBy?.ToLower() switch
                {
                    "pickuplocation" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.PickupLocation)
                        : ordersQuery.OrderBy(o => o.PickupLocation),
                    "deliverylocation" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.DeliveryLocation)
                        : ordersQuery.OrderBy(o => o.DeliveryLocation),
                    "status" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.Status)
                        : ordersQuery.OrderBy(o => o.Status),
                    "deliverydatetime" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.DeliveryDateTime)
                        : ordersQuery.OrderBy(o => o.DeliveryDateTime),
                    _ => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.CreatedAt)
                        : ordersQuery.OrderBy(o => o.CreatedAt)
                };

                // Get total count
                var totalCount = await ordersQuery.CountAsync();

                // Apply pagination
                var orders = await ordersQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var orderResponses = new List<CargoOrderResponseModel>();

                foreach (var order in orders)
                {
                    // Initialize collections to prevent null reference
                    order.Items ??= new List<CargoOrderItem>();
                    order.Documents ??= new List<string>();
                    order.DeliveryDocuments ??= new List<string>();

                    var orderResponse = _mapper.Map<CargoOrderResponseModel>(order);

                    // Calculate order summary
                    var summary = await GetOrderSummary(order);
                    orderResponse.TotalWeight = summary.TotalWeight;
                    orderResponse.TotalVolume = summary.TotalVolume;
                    orderResponse.HasFragileItems = summary.HasFragileItems;
                    orderResponse.ItemTypeBreakdown = summary.ItemTypeBreakdown;

                    // Set driver bid information for this specific driver
                    var driverBid = order.Bids?.FirstOrDefault(b => b.TruckId == driver.Truck.Id);
                    if (driverBid != null)
                    {
                        orderResponse.DriverBidInfo = new DriverBidInfo
                        {
                            BidId = driverBid.Id,
                            Amount = driverBid.Amount,
                            Status = driverBid.Status,
                            CreatedAt = driverBid.CreatedAt,
                            UpdatedAt = driverBid.UpdatedAt
                        };
                    }

                    // Clear the bids list as it shouldn't be exposed to drivers
                    orderResponse.Bids = null;

                    orderResponses.Add(orderResponse);
                }

                // Create paged response
                var pagedResponse = new PagedResponse<CargoOrderResponseModel>
                {
                    Data = orderResponses,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return ApiResponseModel<PagedResponse<CargoOrderResponseModel>>.Success(
                    $"All cargo orders retrieved successfully. Found {totalCount} orders.",
                    pagedResponse,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<PagedResponse<CargoOrderResponseModel>>.Fail(
                    $"Error retrieving all cargo orders: {ex.Message}",
                    500);
            }
        }
               #region Admin Methods

        /// <summary>
        /// Get cargo orders for admin with advanced filtering, pagination, and search
        /// </summary>
        public async Task<ApiResponseModel<PagedResponse<AdminCargoOrderResponseModel>>> GetCargoOrdersForAdminAsync(
            AdminGetCargoOrdersQueryDto query)
        {
            try
            {
                // Start with base query including all necessary relationships
                var ordersQuery = _dbContext.Set<CargoOrders>()
                    .Include(o => o.CargoOwner)
                        .ThenInclude(co => co.User)
                    .Include(o => o.Items)
                    .Include(o => o.Bids)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .AsSplitQuery()
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchTerm = query.SearchTerm.ToLower();
                    ordersQuery = ordersQuery.Where(o =>
                        o.CargoOwner.Name.ToLower().Contains(searchTerm) ||
                        o.CargoOwner.CompanyName.ToLower().Contains(searchTerm) ||
                        o.CargoOwner.EmailAddress.ToLower().Contains(searchTerm) ||
                        o.PickupLocation.ToLower().Contains(searchTerm) ||
                        o.DeliveryLocation.ToLower().Contains(searchTerm) ||
                        (o.Consignment != null && o.Consignment.ToLower().Contains(searchTerm)) ||
                        (o.InvoiceNumber != null && o.InvoiceNumber.ToLower().Contains(searchTerm)));
                }

                // Apply filters
                if (query.Status.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);
                }

                if (query.PaymentStatus.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.PaymentStatus == query.PaymentStatus.Value);
                }

                
                if (!string.IsNullOrWhiteSpace(query.CargoOwnerName))
                {
                    var ownerName = query.CargoOwnerName.ToLower();
                    ordersQuery = ordersQuery.Where(o => o.CargoOwner.Name.ToLower().Contains(ownerName));
                }
                
                // Date filters
                if (query.CreatedFrom.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt >= query.CreatedFrom.Value);
                }

                if (query.CreatedTo.HasValue)
                {
                    var endDate = query.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt <= endDate);
                }
                
                if (query.PaymentMethod.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.PaymentMethod == query.PaymentMethod.Value);
                }

                if (query.IsFlagged.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.IsFlagged == query.IsFlagged.Value);
                }

                if (query.IsPaid.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.IsPaid == query.IsPaid.Value);
                }

                if (query.HasAcceptedBid.HasValue)
                {
                    if (query.HasAcceptedBid.Value)
                    {
                        ordersQuery = ordersQuery.Where(o => o.AcceptedBidId != null);
                    }
                    else
                    {
                        ordersQuery = ordersQuery.Where(o => o.AcceptedBidId == null);
                    }
                }

                // Apply sorting
                ordersQuery = query.SortBy?.ToLower() switch
                {
                    "totalamount" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.TotalAmount)
                        : ordersQuery.OrderBy(o => o.TotalAmount),
                    "status" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.Status)
                        : ordersQuery.OrderBy(o => o.Status),
                    "paymentstatus" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.PaymentStatus)
                        : ordersQuery.OrderBy(o => o.PaymentStatus),
                    "pickupdatetime" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.PickupDateTime)
                        : ordersQuery.OrderBy(o => o.PickupDateTime),
                    "deliverydatetime" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.DeliveryDateTime)
                        : ordersQuery.OrderBy(o => o.DeliveryDateTime),
                    "cargoownername" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.CargoOwner.Name)
                        : ordersQuery.OrderBy(o => o.CargoOwner.Name),
                    _ => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.CreatedAt)
                        : ordersQuery.OrderBy(o => o.CreatedAt)
                };

                // Get total count
                var totalCount = await ordersQuery.CountAsync();

                // Apply pagination
                var orders = await ordersQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var orderResponses = new List<AdminCargoOrderResponseModel>();

                foreach (var order in orders)
                {
                    // Initialize collections to prevent null reference
                    order.Items ??= new List<CargoOrderItem>();
                    order.Bids ??= new List<Bid>();

                    var orderResponse = new AdminCargoOrderResponseModel
                    {
                        Id = order.Id,
                        CargoOwnerId = order.CargoOwnerId,
                        CargoOwnerName = order.CargoOwner?.Name ?? "N/A",
                        CargoOwnerEmail = order.CargoOwner?.EmailAddress ?? "N/A",
                        CargoOwnerCompany = order.CargoOwner?.CompanyName ?? "N/A",
                        PickupLocation = order.PickupLocation,
                        DeliveryLocation = order.DeliveryLocation,
                        Status = order.Status,
                        StatusDisplay = order.Status.ToString(),
                        PaymentStatus = order.PaymentStatus,
                        PaymentStatusDisplay = order.PaymentStatus.ToString(),
                        ItemCount = order.Items?.Count ?? 0,
                        BidCount = order.Bids?.Count ?? 0,
                        HasAcceptedBid = !string.IsNullOrEmpty(order.AcceptedBidId),
                        AcceptedBidId = order.AcceptedBidId,
                        AcceptedBidAmount = order.AcceptedBid?.Amount,
                        AcceptedDriverName = order.AcceptedBid?.Truck?.Driver?.Name,
                        AcceptedTruckPlateNumber = order.AcceptedBid?.Truck?.PlateNumber,
                        TotalAmount = order.TotalAmount,
                        SystemFee = order.SystemFee,
                        Tax = order.Tax,
                        Consignment = order.Consignment,
                        PickupDateTime = order.PickupDateTime,
                        DeliveryDateTime = order.DeliveryDateTime,
                        InvoiceNumber = order.InvoiceNumber,
                        PaymentDueDate = order.PaymentDueDate,
                        IsPaid = order.IsPaid,
                        PaymentMethod = order.PaymentMethod,
                        WalletPaymentAmount = order.WalletPaymentAmount,
                        StripePaymentAmount = order.StripePaymentAmount,
                        IsFlagged = order.IsFlagged,
                        FlagReason = order.FlagReason,
                        FlaggedAt = order.FlaggedAt,
                        FlaggedBy = order.FlaggedBy,
                        CreatedAt = order.CreatedAt,
                        UpdatedAt = order.UpdatedAt
                    };

                    orderResponses.Add(orderResponse);
                }

                // Create paged response
                var pagedResponse = new PagedResponse<AdminCargoOrderResponseModel>
                {
                    Data = orderResponses,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return ApiResponseModel<PagedResponse<AdminCargoOrderResponseModel>>.Success(
                    $"Cargo orders retrieved successfully. Found {totalCount} orders.",
                    pagedResponse,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<PagedResponse<AdminCargoOrderResponseModel>>.Fail(
                    $"Error retrieving cargo orders: {ex.Message}",
                    500);
            }
        }

        /// <summary>
        /// Get detailed cargo order information for admin
        /// </summary>
        public async Task<ApiResponseModel<AdminCargoOrderDetailsResponseModel>> GetCargoOrderDetailsForAdminAsync(string orderId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.CargoOwner)
                        .ThenInclude(co => co.User)
                    .Include(o => o.Items)
                    .Include(o => o.Bids)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .Include(o => o.Bids)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.TruckOwner)
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<AdminCargoOrderDetailsResponseModel>.Fail(
                        "Cargo order not found",
                        404);
                }

                // Initialize collections
                order.Items ??= new List<CargoOrderItem>();
                order.Bids ??= new List<Bid>();
                order.Documents ??= new List<string>();
                order.DeliveryDocuments ??= new List<string>();

                // Get cargo owner summary with additional stats
                var cargoOwnerStats = await GetCargoOwnerStats(order.CargoOwnerId);

                var response = new AdminCargoOrderDetailsResponseModel
                {
                    Id = order.Id,
                    CargoOwnerId = order.CargoOwnerId,
                    CargoOwnerName = order.CargoOwner?.Name ?? "N/A",
                    CargoOwnerEmail = order.CargoOwner?.EmailAddress ?? "N/A",
                    CargoOwnerCompany = order.CargoOwner?.CompanyName ?? "N/A",
                    PickupLocation = order.PickupLocation,
                    PickupLocationLat = order.PickupLocationLat,
                    PickupLocationLong = order.PickupLocationLong,
                    DeliveryLocation = order.DeliveryLocation,
                    DeliveryLocationLat = order.DeliveryLocationLat,
                    DeliveryLocationLong = order.DeliveryLocationLong,
                    Status = order.Status,
                    StatusDisplay = order.Status.ToString(),
                    PaymentStatus = order.PaymentStatus,
                    PaymentStatusDisplay = order.PaymentStatus.ToString(),
                    ItemCount = order.Items.Count,
                    BidCount = order.Bids.Count,
                    HasAcceptedBid = !string.IsNullOrEmpty(order.AcceptedBidId),
                    AcceptedBidId = order.AcceptedBidId,
                    AcceptedBidAmount = order.AcceptedBid?.Amount,
                    AcceptedDriverName = order.AcceptedBid?.Truck?.Driver?.Name,
                    AcceptedTruckPlateNumber = order.AcceptedBid?.Truck?.PlateNumber,
                    TotalAmount = order.TotalAmount,
                    SystemFee = order.SystemFee,
                    Tax = order.Tax,
                    Consignment = order.Consignment,
                    PickupDateTime = order.PickupDateTime,
                    ActualPickupDateTime = order.ActualPickupDateTime,
                    DeliveryDateTime = order.DeliveryDateTime,
                    InvoiceNumber = order.InvoiceNumber,
                    PaymentDueDate = order.PaymentDueDate,
                    PaymentIntentId = order.PaymentIntentId,
                    PaymentDate = order.PaymentDate,
                    IsPaid = order.IsPaid,
                    PaymentMethod = order.PaymentMethod,
                    WalletPaymentAmount = order.WalletPaymentAmount,
                    StripePaymentAmount = order.StripePaymentAmount,
                    DriverEarnings = order.DriverEarnings,
                    IsFlagged = order.IsFlagged,
                    FlagReason = order.FlagReason,
                    FlaggedAt = order.FlaggedAt,
                    FlaggedBy = order.FlaggedBy,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    Documents = order.Documents,
                    DeliveryDocuments = order.DeliveryDocuments,

                    // Related entities
                    CargoOwner = new AdminCargoOwnerSummaryModel
                    {
                        Id = order.CargoOwner.Id,
                        Name = order.CargoOwner.Name,
                        Email = order.CargoOwner.EmailAddress,
                        Phone = order.CargoOwner.Phone,
                        CompanyName = order.CargoOwner.CompanyName,
                        Address = order.CargoOwner.Address,
                        OwnerType = order.CargoOwner.OwnerType,
                        IsActive = order.CargoOwner.User?.IsActive ?? false,
                        CreatedAt = order.CargoOwner.CreatedAt,
                        TotalOrders = cargoOwnerStats.TotalOrders,
                        CompletedOrders = cargoOwnerStats.CompletedOrders,
                        TotalSpent = cargoOwnerStats.TotalSpent
                    },

                    Items = order.Items.Select(item => new CargoOrderItemDetailsModel
                    {
                        Id = item.Id,
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
                    }).ToList(),

                    Bids = order.Bids.Select(bid => new AdminBidDetailsModel
                    {
                        Id = bid.Id,
                        TruckId = bid.TruckId,
                        TruckPlateNumber = bid.Truck?.PlateNumber ?? "N/A",
                        TruckType = bid.Truck?.TruckType ?? "N/A",
                        DriverId = bid.Truck?.DriverId ?? "N/A",
                        DriverName = bid.Truck?.Driver?.Name ?? "N/A",
                        DriverPhone = bid.Truck?.Driver?.Phone ?? "N/A",
                        TruckOwnerId = bid.Truck?.TruckOwnerId,
                        TruckOwnerName = bid.Truck?.TruckOwner?.Name,
                        Amount = bid.Amount,
                        Status = bid.Status,
                        CreatedAt = bid.CreatedAt,
                        UpdatedAt = bid.UpdatedAt,
                        IsAccepted = bid.Id == order.AcceptedBidId
                    }).ToList(),

                    AcceptedBid = order.AcceptedBid != null ? new AdminBidDetailsModel
                    {
                        Id = order.AcceptedBid.Id,
                        TruckId = order.AcceptedBid.TruckId,
                        TruckPlateNumber = order.AcceptedBid.Truck?.PlateNumber ?? "N/A",
                        TruckType = order.AcceptedBid.Truck?.TruckType ?? "N/A",
                        DriverId = order.AcceptedBid.Truck?.DriverId ?? "N/A",
                        DriverName = order.AcceptedBid.Truck?.Driver?.Name ?? "N/A",
                        DriverPhone = order.AcceptedBid.Truck?.Driver?.Phone ?? "N/A",
                        TruckOwnerId = order.AcceptedBid.Truck?.TruckOwnerId,
                        TruckOwnerName = order.AcceptedBid.Truck?.TruckOwner?.Name,
                        Amount = order.AcceptedBid.Amount,
                        Status = order.AcceptedBid.Status,
                        CreatedAt = order.AcceptedBid.CreatedAt,
                        UpdatedAt = order.AcceptedBid.UpdatedAt,
                        IsAccepted = true
                    } : null,

                    // Audit trail
                    AuditTrail = new AdminCargoOrderAuditTrail
                    {
                        CreatedAt = order.CreatedAt,
                        LastStatusChange = order.UpdatedAt,
                        LastPaymentUpdate = order.PaymentDate,
                        FlaggedAt = order.FlaggedAt,
                        FlaggedBy = order.FlaggedBy,
                        StatusHistory = new List<CargoOrderAuditEntry>() // This would need to be implemented with audit table
                    }
                };

                return ApiResponseModel<AdminCargoOrderDetailsResponseModel>.Success(
                    "Cargo order details retrieved successfully",
                    response,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<AdminCargoOrderDetailsResponseModel>.Fail(
                    $"Error retrieving cargo order details: {ex.Message}",
                    500);
            }
        }

        /// <summary>
        /// Get cargo order statistics for admin dashboard
        /// </summary>
        public async Task<ApiResponseModel<AdminCargoOrderStatisticsResponseModel>> GetCargoOrderStatisticsForAdminAsync(
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var lastMonth = currentMonth.AddMonths(-1);
                var nextMonth = currentMonth.AddMonths(1);

                var baseQuery = _dbContext.Set<CargoOrders>().AsQueryable();

                // Apply date filter if provided
                if (fromDate.HasValue)
                {
                    baseQuery = baseQuery.Where(o => o.CreatedAt >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                    baseQuery = baseQuery.Where(o => o.CreatedAt <= endDate);
                }

                var totalOrders = await baseQuery.CountAsync();
                var totalOrdersThisMonth = await baseQuery.CountAsync(o => o.CreatedAt >= currentMonth && o.CreatedAt < nextMonth);
                var totalOrdersLastMonth = await baseQuery.CountAsync(o => o.CreatedAt >= lastMonth && o.CreatedAt < currentMonth);

                var totalRevenue = await baseQuery.SumAsync(o => o.TotalAmount);
                var totalRevenueThisMonth = await baseQuery
                    .Where(o => o.CreatedAt >= currentMonth && o.CreatedAt < nextMonth)
                    .SumAsync(o => o.TotalAmount);
                var totalRevenueLastMonth = await baseQuery
                    .Where(o => o.CreatedAt >= lastMonth && o.CreatedAt < currentMonth)
                    .SumAsync(o => o.TotalAmount);

                var totalSystemFees = await baseQuery.SumAsync(o => o.SystemFee);
                var totalTaxes = await baseQuery.SumAsync(o => o.Tax);

                var activeStatuses = new[]
                {
                    CargoOrderStatus.OpenForBidding,
                    CargoOrderStatus.BiddingInProgress,
                    CargoOrderStatus.DriverSelected,
                    CargoOrderStatus.DriverAcknowledged,
                    CargoOrderStatus.ReadyForPickup,
                    CargoOrderStatus.InTransit
                };

                var activeOrders = await baseQuery.CountAsync(o => activeStatuses.Contains(o.Status));
                var completedOrders = await baseQuery.CountAsync(o => o.Status == CargoOrderStatus.Delivered);
                var cancelledOrders = await baseQuery.CountAsync(o => o.Status == CargoOrderStatus.Cancelled);
                var flaggedOrders = await baseQuery.CountAsync(o => o.IsFlagged);
                var overduePayments = await baseQuery.CountAsync(o => 
                    o.PaymentDueDate.HasValue && 
                    o.PaymentDueDate < DateTime.UtcNow && 
                    !o.IsPaid);

                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                // Status breakdown
                var statusBreakdown = await baseQuery
                    .GroupBy(o => o.Status)
                    .Select(g => new CargoOrderStatusCount
                    {
                        Status = g.Key,
                        StatusDisplay = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = totalOrders > 0 ? Math.Round((decimal)g.Count() / totalOrders * 100, 2) : 0
                    })
                    .ToListAsync();

                // Payment status breakdown
                var paymentStatusBreakdown = await baseQuery
                    .GroupBy(o => o.PaymentStatus)
                    .Select(g => new PaymentStatusCount
                    {
                        Status = g.Key,
                        StatusDisplay = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = totalOrders > 0 ? Math.Round((decimal)g.Count() / totalOrders * 100, 2) : 0
                    })
                    .ToListAsync();

                // Monthly trends (last 12 months)
                var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
                var monthlyTrends = await baseQuery
                    .Where(o => o.CreatedAt >= twelveMonthsAgo)
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new MonthlyOrderTrend
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                        OrderCount = g.Count(),
                        Revenue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

                var statistics = new AdminCargoOrderStatisticsResponseModel
                {
                    TotalOrders = totalOrders,
                    TotalOrdersThisMonth = totalOrdersThisMonth,
                    TotalOrdersLastMonth = totalOrdersLastMonth,
                    TotalRevenue = totalRevenue,
                    TotalRevenueThisMonth = totalRevenueThisMonth,
                    TotalRevenueLastMonth = totalRevenueLastMonth,
                    TotalSystemFees = totalSystemFees,
                    TotalTaxes = totalTaxes,
                    ActiveOrders = activeOrders,
                    CompletedOrders = completedOrders,
                    CancelledOrders = cancelledOrders,
                    FlaggedOrders = flaggedOrders,
                    OverduePayments = overduePayments,
                    AverageOrderValue = averageOrderValue,
                    StatusBreakdown = statusBreakdown,
                    PaymentStatusBreakdown = paymentStatusBreakdown,
                    MonthlyTrends = monthlyTrends,
                    LastUpdated = DateTime.UtcNow
                };

                return ApiResponseModel<AdminCargoOrderStatisticsResponseModel>.Success(
                    "Statistics retrieved successfully",
                    statistics,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<AdminCargoOrderStatisticsResponseModel>.Fail(
                    $"Error retrieving statistics: {ex.Message}",
                    500);
            }
        }

        /// <summary>
        /// Flag or unflag a cargo order
        /// </summary>
        public async Task<ApiResponseModel<bool>> FlagCargoOrderAsync(string orderId, bool isFlagged, string? flagReason, string adminUserId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Cargo order not found", 404);
                }

                order.IsFlagged = isFlagged;
                order.FlagReason = isFlagged ? flagReason : null;
                order.FlaggedAt = isFlagged ? DateTime.UtcNow : null;
                order.FlaggedBy = isFlagged ? adminUserId : null;
                order.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                var action = isFlagged ? "flagged" : "unflagged";
                return ApiResponseModel<bool>.Success(
                    $"Cargo order {action} successfully",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error updating flag status: {ex.Message}",
                    500);
            }
        }

        /// <summary>
        /// Update cargo order status by admin
        /// </summary>
        public async Task<ApiResponseModel<bool>> UpdateCargoOrderStatusByAdminAsync(
            string orderId, CargoOrderStatus status, string? reason, string adminUserId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Cargo order not found", 404);
                }

                var oldStatus = order.Status;
                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                // Log status change (you might want to implement an audit table)
                // await LogStatusChange(orderId, oldStatus, status, adminUserId, reason);

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success(
                    $"Cargo order status updated from {oldStatus} to {status}",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error updating status: {ex.Message}",
                    500);
            }
        }
        /// <summary>
        /// Get cargo orders summary by status
        /// </summary>
        public async Task<ApiResponseModel<List<CargoOrderStatusSummaryModel>>> GetCargoOrdersSummaryAsync()
        {
            try
            {
                var summary = await _dbContext.Set<CargoOrders>()
                    .GroupBy(o => o.Status)
                    .Select(g => new CargoOrderStatusSummaryModel
                    {
                        Status = g.Key,
                        StatusDisplay = g.Key.ToString(),
                        Count = g.Count(),
                        TotalValue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(s => s.Status)
                    .ToListAsync();

                return ApiResponseModel<List<CargoOrderStatusSummaryModel>>.Success(
                    "Summary retrieved successfully",
                    summary,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<List<CargoOrderStatusSummaryModel>>.Fail(
                    $"Error retrieving summary: {ex.Message}",
                    500);
            }
        }

        /// <summary>
        /// Delete a cargo order (admin only) - For cleaning test data
        /// </summary>
        public async Task<ApiResponseModel<bool>> DeleteCargoOrderAsync(string orderId, string adminUserId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.Items)
                    .Include(o => o.Bids)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Cargo order not found", 404);
                }

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // First, break the circular reference by clearing AcceptedBidId
                        if (!string.IsNullOrEmpty(order.AcceptedBidId))
                        {
                            order.AcceptedBidId = null;
                            order.AcceptedBid = null;
                            await _dbContext.SaveChangesAsync();
                        }

                        // Delete related data first
                        if (order.Items != null && order.Items.Any())
                        {
                            _dbContext.Set<CargoOrderItem>().RemoveRange(order.Items);
                        }

                        if (order.Bids != null && order.Bids.Any())
                        {
                            _dbContext.Set<Bid>().RemoveRange(order.Bids);
                        }

                        // Delete any delivery location updates for this order
                        var deliveryUpdates = await _dbContext.Set<DeliveryLocationUpdate>()
                            .Where(d => d.OrderId == orderId)
                            .ToListAsync();
                        if (deliveryUpdates.Any())
                        {
                            _dbContext.Set<DeliveryLocationUpdate>().RemoveRange(deliveryUpdates);
                        }

                        // Delete any driver wallet transactions related to this order
                        var driverWalletTransactions = await _dbContext.Set<DriverWalletTransaction>()
                            .Where(t => t.RelatedOrderId == orderId)
                            .ToListAsync();
                        if (driverWalletTransactions.Any())
                        {
                            _dbContext.Set<DriverWalletTransaction>().RemoveRange(driverWalletTransactions);
                        }

                        // Delete any wallet transactions related to this order
                        var walletTransactions = await _dbContext.Set<WalletTransaction>()
                            .Where(t => t.RelatedOrderId == orderId)
                            .ToListAsync();
                        if (walletTransactions.Any())
                        {
                            _dbContext.Set<WalletTransaction>().RemoveRange(walletTransactions);
                        }

                        // Delete the order itself
                        _dbContext.Set<CargoOrders>().Remove(order);

                        await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return ApiResponseModel<bool>.Success("Cargo order deleted successfully", true, 200);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponseModel<bool>.Fail($"Error deleting cargo order: {ex.Message}", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail($"Error deleting cargo order: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Helper method to get cargo owner statistics
        /// </summary>
        private async Task<(int TotalOrders, int CompletedOrders, decimal TotalSpent)> GetCargoOwnerStats(string cargoOwnerId)
        {
            var orders = await _dbContext.Set<CargoOrders>()
                .Where(o => o.CargoOwnerId == cargoOwnerId)
                .ToListAsync();

            var totalOrders = orders.Count;
            var completedOrders = orders.Count(o => o.Status == CargoOrderStatus.Delivered);
            var totalSpent = orders.Where(o => o.IsPaid).Sum(o => o.TotalAmount);

            return (totalOrders, completedOrders, totalSpent);
        }

        #endregion
    }

}
