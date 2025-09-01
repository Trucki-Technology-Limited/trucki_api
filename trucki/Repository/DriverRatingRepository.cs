using Microsoft.EntityFrameworkCore;
using AutoMapper;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository
{
    public class DriverRatingRepository : IDriverRatingRepository
    {
        private readonly TruckiDBContext _context;
        private readonly IMapper _mapper;

        public DriverRatingRepository(TruckiDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponseModel<bool>> SubmitRatingAsync(SubmitRatingDto model, string cargoOwnerId)
        {
            try
            {
                // Get the order and verify it's completed and belongs to the cargo owner
                var order = await _context.CargoOrders
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .FirstOrDefaultAsync(o => o.Id == model.CargoOrderId && 
                                            o.CargoOwnerId == cargoOwnerId &&
                                            o.Status == CargoOrderStatus.Delivered);

                if (order == null)
                {
                    return new ApiResponseModel<bool>
                    {
                        IsSuccessful = false,
                        Message = "Order not found or not completed",
                        StatusCode = 404
                    };
                }

                if (order.AcceptedBid?.Truck?.Driver == null)
                {
                    return new ApiResponseModel<bool>
                    {
                        IsSuccessful = false,
                        Message = "No driver found for this order",
                        StatusCode = 400
                    };
                }

                // Check if rating already exists
                var existingRating = await _context.DriverRatings
                    .FirstOrDefaultAsync(r => r.CargoOrderId == model.CargoOrderId);

                if (existingRating != null)
                {
                    return new ApiResponseModel<bool>
                    {
                        IsSuccessful = false,
                        Message = "Rating already submitted for this order",
                        StatusCode = 400
                    };
                }

                // Create new rating
                var rating = new DriverRating
                {
                    CargoOrderId = model.CargoOrderId,
                    DriverId = order.AcceptedBid.Truck.Driver.Id,
                    CargoOwnerId = cargoOwnerId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    RatedAt = DateTime.UtcNow
                };

                _context.DriverRatings.Add(rating);
                await _context.SaveChangesAsync();

                return new ApiResponseModel<bool>
                {
                    IsSuccessful = true,
                    Message = "Rating submitted successfully",
                    StatusCode = 201,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = $"Error submitting rating: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponseModel<DriverRatingSummaryModel>> GetDriverRatingSummaryAsync(string driverId)
        {
            try
            {
                var ratings = await _context.DriverRatings
                    .Where(r => r.DriverId == driverId)
                    .ToListAsync();

                if (!ratings.Any())
                {
                    return new ApiResponseModel<DriverRatingSummaryModel>
                    {
                        IsSuccessful = true,
                        Message = "No ratings found",
                        StatusCode = 200,
                        Data = new DriverRatingSummaryModel
                        {
                            AverageRating = 0,
                            TotalRatings = 0
                        }
                    };
                }

                var averageRating = ratings.Average(r => r.Rating);
                var ratingBreakdown = ratings
                    .GroupBy(r => r.Rating)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Ensure all rating values (1-5) are represented
                for (int i = 1; i <= 5; i++)
                {
                    if (!ratingBreakdown.ContainsKey(i))
                        ratingBreakdown[i] = 0;
                }

                var summary = new DriverRatingSummaryModel
                {
                    AverageRating = Math.Round((decimal)averageRating, 2),
                    TotalRatings = ratings.Count,
                    RatingBreakdown = ratingBreakdown
                };

                return new ApiResponseModel<DriverRatingSummaryModel>
                {
                    IsSuccessful = true,
                    Message = "Driver rating summary retrieved successfully",
                    StatusCode = 200,
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<DriverRatingSummaryModel>
                {
                    IsSuccessful = false,
                    Message = $"Error retrieving rating summary: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponseModel<IEnumerable<DriverRatingResponseModel>>> GetDriverRatingsAsync(string driverId)
        {
            try
            {
                var ratings = await _context.DriverRatings
                    .Include(r => r.CargoOwner)
                    .Include(r => r.CargoOrder)
                    .Where(r => r.DriverId == driverId)
                    .OrderByDescending(r => r.RatedAt)
                    .ToListAsync();

                var ratingModels = ratings.Select(r => new DriverRatingResponseModel
                {
                    Id = r.Id,
                    CargoOrderId = r.CargoOrderId,
                    CargoOwnerId = r.CargoOwnerId,
                    CargoOwnerName = r.CargoOwner.Name,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    RatedAt = r.RatedAt,
                    PickupLocation = r.CargoOrder.PickupLocation,
                    DeliveryLocation = r.CargoOrder.DeliveryLocation
                });

                return new ApiResponseModel<IEnumerable<DriverRatingResponseModel>>
                {
                    IsSuccessful = true,
                    Message = "Driver ratings retrieved successfully",
                    StatusCode = 200,
                    Data = ratingModels
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<IEnumerable<DriverRatingResponseModel>>
                {
                    IsSuccessful = false,
                    Message = $"Error retrieving ratings: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponseModel<DriverRatingResponseModel>> GetRatingByOrderIdAsync(string orderId)
        {
            try
            {
                var rating = await _context.DriverRatings
                    .Include(r => r.CargoOwner)
                    .Include(r => r.CargoOrder)
                    .FirstOrDefaultAsync(r => r.CargoOrderId == orderId);

                if (rating == null)
                {
                    return new ApiResponseModel<DriverRatingResponseModel>
                    {
                        IsSuccessful = false,
                        Message = "Rating not found for this order",
                        StatusCode = 404
                    };
                }

                var ratingModel = new DriverRatingResponseModel
                {
                    Id = rating.Id,
                    CargoOrderId = rating.CargoOrderId,
                    CargoOwnerId = rating.CargoOwnerId,
                    CargoOwnerName = rating.CargoOwner.Name,
                    Rating = rating.Rating,
                    Comment = rating.Comment,
                    RatedAt = rating.RatedAt,
                    PickupLocation = rating.CargoOrder.PickupLocation,
                    DeliveryLocation = rating.CargoOrder.DeliveryLocation
                };

                return new ApiResponseModel<DriverRatingResponseModel>
                {
                    IsSuccessful = true,
                    Message = "Rating retrieved successfully",
                    StatusCode = 200,
                    Data = ratingModel
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<DriverRatingResponseModel>
                {
                    IsSuccessful = false,
                    Message = $"Error retrieving rating: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponseModel<bool>> HasCargoOwnerRatedOrderAsync(string orderId, string cargoOwnerId)
        {
            try
            {
                var hasRated = await _context.DriverRatings
                    .AnyAsync(r => r.CargoOrderId == orderId && r.CargoOwnerId == cargoOwnerId);

                return new ApiResponseModel<bool>
                {
                    IsSuccessful = true,
                    Message = hasRated ? "Cargo owner has already rated this order" : "Cargo owner has not rated this order",
                    StatusCode = 200,
                    Data = hasRated
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = $"Error checking rating status: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
