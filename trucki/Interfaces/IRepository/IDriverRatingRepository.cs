using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository
{
    public interface IDriverRatingRepository
    {
        Task<ApiResponseModel<bool>> SubmitRatingAsync(SubmitRatingDto model, string cargoOwnerId);
        Task<ApiResponseModel<DriverRatingSummaryModel>> GetDriverRatingSummaryAsync(string driverId);
        Task<ApiResponseModel<IEnumerable<DriverRatingResponseModel>>> GetDriverRatingsAsync(string driverId);
        Task<ApiResponseModel<DriverRatingResponseModel>> GetRatingByOrderIdAsync(string orderId);
        Task<ApiResponseModel<bool>> HasCargoOwnerRatedOrderAsync(string orderId, string cargoOwnerId);
    
    }
}