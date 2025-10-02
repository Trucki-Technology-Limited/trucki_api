using AutoMapper;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services
{
    public class AccountDeletionService : IAccountDeletionService
    {
        private readonly IAccountDeletionRepository _accountDeletionRepository;
        private readonly TruckiDBContext _context;
        private readonly IMapper _mapper;

        public AccountDeletionService(
            IAccountDeletionRepository accountDeletionRepository,
            TruckiDBContext context,
            IMapper mapper)
        {
            _accountDeletionRepository = accountDeletionRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponseModel<bool>> RequestAccountDeletion(string userId, AccountDeletionRequestModel model)
        {
            try
            {
                // Check if user already has a pending deletion request
                var hasExistingRequest = await _accountDeletionRepository.HasPendingRequestAsync(userId);
                if (hasExistingRequest)
                {
                    return ApiResponseModel<bool>.Fail(
                        "You already have a pending account deletion request",
                        StatusCodes.Status400BadRequest);
                }

                // Check if user is a driver or cargo owner and if they're from US
                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                var cargoOwner = await _context.CargoOwners
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                string userType;
                string userCountry;

                if (driver != null)
                {
                    userType = "Driver";
                    userCountry = driver.Country;
                }
                else if (cargoOwner != null)
                {
                    userType = "CargoOwner";
                    userCountry = cargoOwner.Country;
                }
                else
                {
                    return ApiResponseModel<bool>.Fail(
                        "Only drivers and cargo owners can request account deletion",
                        StatusCodes.Status403Forbidden);
                }

                // Validate that user is from US
                if (userCountry?.ToUpper() != "US")
                {
                    return ApiResponseModel<bool>.Fail(
                        "Account deletion requests are only available for US users",
                        StatusCodes.Status403Forbidden);
                }

                // Create the deletion request
                var deletionRequest = new AccountDeletionRequest
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    UserType = userType,
                    DeletionReason = model.DeletionReason,
                    Country = userCountry,
                    Status = AccountDeletionStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                };

                await _accountDeletionRepository.CreateAsync(deletionRequest);

                return ApiResponseModel<bool>.Success(
                    "Account deletion request submitted successfully. Our team will review your request.",
                    true,
                    StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    "An error occurred while processing your request",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponseModel<AccountDeletionResponseModel>> GetAccountDeletionRequest(string userId)
        {
            try
            {
                var deletionRequest = await _accountDeletionRepository.GetByUserIdAsync(userId);

                if (deletionRequest == null)
                {
                    return ApiResponseModel<AccountDeletionResponseModel>.Fail(
                        "No account deletion request found",
                        StatusCodes.Status404NotFound);
                }

                var responseModel = _mapper.Map<AccountDeletionResponseModel>(deletionRequest);

                return ApiResponseModel<AccountDeletionResponseModel>.Success(
                    "Account deletion request retrieved successfully",
                    responseModel,
                    StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<AccountDeletionResponseModel>.Fail(
                    "An error occurred while retrieving your request",
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}