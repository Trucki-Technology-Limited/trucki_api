using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trucki.CustomExtension;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class CargoOwnerRepository : ICargoOwnerRepository
{
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailSender;

    public CargoOwnerRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _emailSender = emailSender;
        _userManager = userManager;
    }

    public async Task<ApiResponseModel<string>> CreateCargoOwnerAccount(CreateCargoOwnerRequestModel model)
    {
        var existingCargoOwner = await _context.CargoOwners
            .FirstOrDefaultAsync(m => m.EmailAddress == model.Email || m.Phone == model.Phone);

        if (existingCargoOwner != null)
        {
            if (existingCargoOwner.EmailAddress == model.Email)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Email address already exists",
                    StatusCode = 400
                };
            }
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Phone number already exists",
                StatusCode = 400
            };
        }

        var newCargoOwner = new CargoOwner
        {
            Name = model.Name,
            Phone = model.Phone,
            EmailAddress = model.Email,
            CompanyName = model.CompanyName,
            Address = model.Address,
            Country = model.Country,
            OwnerType = model.OwnerType
        };

        _context.CargoOwners.Add(newCargoOwner);
        var res = await _authService.AddNewUserAsync(newCargoOwner.Name, newCargoOwner.EmailAddress,
            newCargoOwner.Phone, "cargo owner", model.Password, true);

        if (res.StatusCode == 201)
        {
            var user = await _userManager.FindByEmailAsync(newCargoOwner.EmailAddress);
            newCargoOwner.UserId = user.Id;
            newCargoOwner.User = user;

            await _context.SaveChangesAsync();

            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string confirmationLink = $"https://trucki.co/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(confirmationToken)}";

            // Send welcome email
            await _emailSender.SendWelcomeEmailAsync(
                newCargoOwner.EmailAddress,
                newCargoOwner.Name,
                "cargo owner",
                confirmationLink);

            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                Message = "Cargo owner account created successfully",
                StatusCode = 201,
                Data = ""
            };
        }

        return new ApiResponseModel<string>
        {
            IsSuccessful = false,
            Message = "An error occurred while creating the cargo owner account",
            StatusCode = 400
        };
    }

    public async Task<ApiResponseModel<bool>> EditCargoOwnerProfile(EditCargoOwnerRequestModel model)
    {
        var cargoOwner = await _context.CargoOwners.FindAsync(model.Id);

        if (cargoOwner == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Cargo owner not found",
                StatusCode = 404
            };
        }

        cargoOwner.Name = model.Name;
        cargoOwner.Phone = model.Phone;
        cargoOwner.CompanyName = model.CompanyName;
        cargoOwner.Address = model.Address;

        _context.CargoOwners.Update(cargoOwner);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Profile updated successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<bool>> DeactivateCargoOwner(string cargoOwnerId)
    {
        var cargoOwner = await _context.CargoOwners
            .Include(co => co.User)
            .FirstOrDefaultAsync(co => co.Id == cargoOwnerId);

        if (cargoOwner == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Cargo owner not found",
                StatusCode = 404
            };
        }

        if (cargoOwner.User != null)
        {
            cargoOwner.User.IsActive = false;
        }

        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Cargo owner account deactivated successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<List<CargoOrderResponseModel>>> GetCargoOwnerOrders(GetCargoOwnerOrdersQueryDto query)
    {
        try
        {
            // Start with base query
            var ordersQuery = _context.Set<CargoOrders>()
                .Include(o => o.CargoOwner)
                .Include(o => o.Items)
                .Include(o => o.Bids)
                    .ThenInclude(b => b.Truck)
                        .ThenInclude(t => t.Driver)
                .Include(o => o.AcceptedBid)
                    .ThenInclude(b => b.Truck)
                        .ThenInclude(t => t.Driver)
                .Where(o => o.CargoOwnerId == query.CargoOwnerId)
                .AsSplitQuery();

            // Apply status filter if provided
            if (query.Status.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);
            }

            // Apply sorting
            ordersQuery = query.SortBy?.ToLower() switch
            {
                "status" when query.SortDescending => ordersQuery.OrderByDescending(o => o.Status)
                    .ThenByDescending(o => o.CreatedAt),

                "status" => ordersQuery.OrderBy(o => o.Status)
                    .ThenByDescending(o => o.CreatedAt),

                // Default sort by date
                _ when query.SortDescending => ordersQuery.OrderByDescending(o => o.CreatedAt),

                _ => ordersQuery.OrderBy(o => o.CreatedAt)
            };

            var orders = await ordersQuery.ToListAsync();

            if (!orders.Any())
            {
                return ApiResponseModel<List<CargoOrderResponseModel>>.Success(
                    "No orders found",
                    new List<CargoOrderResponseModel>(),
                    200);
            }

            // Initialize collections to prevent null reference
            foreach (var order in orders)
            {
                order.Items ??= new List<CargoOrderItem>();
                order.Bids ??= new List<Bid>();
                order.Documents ??= new List<string>();
                order.DeliveryDocuments ??= new List<string>();
            }

            var orderResponses = _mapper.Map<List<CargoOrderResponseModel>>(orders);

            // Add summary information for each order
            foreach (var orderResponse in orderResponses)
            {
                var order = orders.First(o => o.Id == orderResponse.Id);
                var summary = await GetOrderSummary(order);
                orderResponse.TotalWeight = summary.TotalWeight;
                orderResponse.TotalVolume = summary.TotalVolume;
                orderResponse.HasFragileItems = summary.HasFragileItems;
                orderResponse.ItemTypeBreakdown = summary.ItemTypeBreakdown;
                orderResponse.SpecialHandlingRequirements = summary.SpecialHandlingRequirements;
            }

            return ApiResponseModel<List<CargoOrderResponseModel>>.Success(
                "Orders retrieved successfully",
                orderResponses,
                200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<List<CargoOrderResponseModel>>.Fail(
                $"An error occurred while retrieving orders: {ex.Message}",
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
    public async Task<ApiResponseModel<CargoOwnerProfileResponseModel>> GetCargoOwnerProfile(string userId)
    {
        var cargoOwner = await _context.CargoOwners
            .Include(co => co.User)
            .FirstOrDefaultAsync(co => co.UserId == userId);

        if (cargoOwner == null)
        {
            return new ApiResponseModel<CargoOwnerProfileResponseModel>
            {
                IsSuccessful = false,
                Message = "Cargo owner not found",
                StatusCode = 404
            };
        }

        var profileResponse = _mapper.Map<CargoOwnerProfileResponseModel>(cargoOwner);

        return new ApiResponseModel<CargoOwnerProfileResponseModel>
        {
            IsSuccessful = true,
            Message = "Profile retrieved successfully",
            StatusCode = 200,
            Data = profileResponse
        };
    }
    public async Task<ApiResponseModel<PagedResponse<AdminCargoOwnerResponseModel>>> GetCargoOwnersWithPagination(
    int pageNumber, int pageSize, string? searchQuery = null)
    {
        try
        {
            var query = _context.CargoOwners
                .Include(co => co.User)
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchTerm = searchQuery.Trim().ToLower();
                query = query.Where(co =>
                    co.Name.ToLower().Contains(searchTerm) ||
                    co.EmailAddress.ToLower().Contains(searchTerm) ||
                    co.CompanyName.ToLower().Contains(searchTerm) ||
                    co.Phone.Contains(searchTerm));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var cargoOwners = await query
                .OrderByDescending(co => co.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var cargoOwnerResponses = _mapper.Map<List<AdminCargoOwnerResponseModel>>(cargoOwners);

            // Set IsActive from User entity
            foreach (var response in cargoOwnerResponses)
            {
                var cargoOwner = cargoOwners.First(co => co.Id == response.Id);
                response.IsActive = cargoOwner.User?.IsActive ?? false;
            }

            var pagedResponse = new PagedResponse<AdminCargoOwnerResponseModel>
            {
                Data = cargoOwnerResponses,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return ApiResponseModel<PagedResponse<AdminCargoOwnerResponseModel>>.Success(
                "Cargo owners retrieved successfully",
                pagedResponse,
                200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<PagedResponse<AdminCargoOwnerResponseModel>>.Fail(
                $"An error occurred while retrieving cargo owners: {ex.Message}",
                500);
        }
    }

    public async Task<ApiResponseModel<AdminCargoOwnerDetailsResponseModel>> GetCargoOwnerDetailsForAdmin(string cargoOwnerId)
    {
        try
        {
            var cargoOwner = await _context.CargoOwners
                .Include(co => co.User)
                .Include(co => co.Orders)
                .FirstOrDefaultAsync(co => co.Id == cargoOwnerId);

            if (cargoOwner == null)
            {
                return ApiResponseModel<AdminCargoOwnerDetailsResponseModel>.Fail(
                    "Cargo owner not found",
                    404);
            }

            var response = _mapper.Map<AdminCargoOwnerDetailsResponseModel>(cargoOwner);
            response.IsActive = cargoOwner.User?.IsActive ?? false;

            // Calculate order statistics
            var completedOrders = cargoOwner.Orders.Where(o => o.Status == CargoOrderStatus.Delivered).ToList();
            response.TotalCompletedOrdersCount = completedOrders.Count;

            var pendingOrderStatuses = new[]
            {
            CargoOrderStatus.Draft,
            CargoOrderStatus.OpenForBidding,
            CargoOrderStatus.BiddingInProgress,
            CargoOrderStatus.DriverSelected,
            CargoOrderStatus.DriverAcknowledged
        };
            response.PendingOrdersCount = cargoOwner.Orders.Count(o => pendingOrderStatuses.Contains(o.Status));

            response.InTransitOrdersCount = cargoOwner.Orders.Count(o => o.Status == CargoOrderStatus.InTransit);

            // Calculate total order value from accepted bids
            response.TotalOrderValue = cargoOwner.Orders
                .Where(o => o.AcceptedBid != null)
                .Sum(o => o.AcceptedBid.Amount);

            response.LastOrderDate = cargoOwner.Orders
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault()?.CreatedAt;

            return ApiResponseModel<AdminCargoOwnerDetailsResponseModel>.Success(
                "Cargo owner details retrieved successfully",
                response,
                200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<AdminCargoOwnerDetailsResponseModel>.Fail(
                $"An error occurred while retrieving cargo owner details: {ex.Message}",
                500);
        }
    }
}