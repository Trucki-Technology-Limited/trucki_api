using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services
{
    public class CargoOwnerService : ICargoOwnerService
    {
        private readonly ICargoOwnerRepository _cargoOwnerRepository;

        public CargoOwnerService(ICargoOwnerRepository cargoOwnerRepository)
        {
            _cargoOwnerRepository = cargoOwnerRepository;
        }

        public async Task<ApiResponseModel<string>> CreateCargoOwnerAccount(CreateCargoOwnerRequestModel model)
        {
            try
            {
                var result = await _cargoOwnerRepository.CreateCargoOwnerAccount(model);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "An error occurred while creating the cargo owner account: " + ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponseModel<bool>> EditCargoOwnerProfile(EditCargoOwnerRequestModel model)
        {
            try
            {
                var result = await _cargoOwnerRepository.EditCargoOwnerProfile(model);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "An error occurred while updating the profile: " + ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponseModel<bool>> DeactivateCargoOwner(string cargoOwnerId)
        {
            try
            {
                var result = await _cargoOwnerRepository.DeactivateCargoOwner(cargoOwnerId);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "An error occurred while deactivating the account: " + ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponseModel<List<CargoOrderResponseModel>>> GetCargoOwnerOrders(GetCargoOwnerOrdersQueryDto query)
        {
            try
            {
                var result = await _cargoOwnerRepository.GetCargoOwnerOrders(query);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<List<CargoOrderResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "An error occurred while retrieving orders: " + ex.Message,
                    StatusCode = 500,
                    Data = new List<CargoOrderResponseModel>()
                };
            }
        }

        public async Task<ApiResponseModel<CargoOwnerProfileResponseModel>> GetCargoOwnerProfile(string userId)
        {
            try
            {
                var result = await _cargoOwnerRepository.GetCargoOwnerProfile(userId);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<CargoOwnerProfileResponseModel>
                {
                    IsSuccessful = false,
                    Message = "An error occurred while retrieving the profile: " + ex.Message,
                    StatusCode = 500
                };
            }
        }
        public async Task<ApiResponseModel<PagedResponse<AdminCargoOwnerResponseModel>>> GetCargoOwnersWithPagination(
    int pageNumber, int pageSize, string? searchQuery = null)
        {
            try
            {
                var result = await _cargoOwnerRepository.GetCargoOwnersWithPagination(pageNumber, pageSize, searchQuery);
                return result;
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
                var result = await _cargoOwnerRepository.GetCargoOwnerDetailsForAdmin(cargoOwnerId);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponseModel<AdminCargoOwnerDetailsResponseModel>.Fail(
                    $"An error occurred while retrieving cargo owner details: {ex.Message}",
                    500);
            }
        }
    }

}