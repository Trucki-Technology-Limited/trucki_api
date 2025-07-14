using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace trucki.Services
{
    public partial class TruckService : ITruckService
    {
        private readonly ITruckRepository _truckRepository;

        public TruckService(ITruckRepository truckRepository)
        {
            _truckRepository = truckRepository;
        }
        public async Task<ApiResponseModel<string>> AddNewTruck(AddTruckRequestModel model)
        {
            return await _truckRepository.AddNewTruck(model);
        }

        public async Task<ApiResponseModel<string>> AddDriverOwnedTruck(DriverAddTruckRequestModel model)
        {
            return await _truckRepository.AddDriverOwnedTruck(model);
        }

        public async Task<ApiResponseModel<bool>> EditTruck(EditTruckRequestModel model)
        {
            return await _truckRepository.EditTruck(model);
        }

        public async Task<ApiResponseModel<string>> DeleteTruck(string truckId)
        {
            return await _truckRepository.DeleteTruck(truckId);
        }

        public async Task<ApiResponseModel<AllTruckResponseModel>> GetTruckById(string truckId)
        {
            return await _truckRepository.GetTruckById(truckId);
        }

        public async Task<ApiResponseModel<List<AllTruckResponseModel>>> GetAllTrucks()
        {
            return await _truckRepository.GetAllTrucks();
        }

        public async Task<ApiResponseModel<IEnumerable<AllTruckResponseModel>>> SearchTruck(string? searchWords)
        {
            return await _truckRepository.SearchTruck(searchWords);
        }

        public async Task<ApiResponseModel<IEnumerable<string>>> GetTruckDocuments(string truckId)
        {
            return await _truckRepository.GetTruckDocuments(truckId);
        }

        public async Task<ApiResponseModel<bool>> AssignDriverToTruck(AssignDriverToTruckRequestModel model)
        {
            return await _truckRepository.AssignDriverToTruck(model);
        }

        public async Task<ApiResponseModel<string>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model)
        {
            return await _truckRepository.UpdateTruckStatus(truckId, model);
        }

        public async Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByOwnersId(string ownersId)
        {
            return await _truckRepository.GetTrucksByOwnersId(ownersId);
        }

        public async Task<ApiResponseModel<List<AllTruckResponseModel>>> GetTrucksByDriverId(string driverId)
        {
            return await _truckRepository.GetTrucksByDriverId(driverId);
        }

        public async Task<ApiResponseModel<TruckStatusCountResponseModel>> GetTruckStatusCountByOwnerId(string ownerId)
        {
            return await _truckRepository.GetTruckStatusCountByOwnerId(ownerId);
        }

        public async Task<ApiResponseModel<string>> UpdateApprovalStatusAsync(string truckId, ApprovalStatus approvalStatus)
        {
            return await _truckRepository.UpdateApprovalStatusAsync(truckId, approvalStatus);
        }

        public async Task<ApiResponseModel<bool>> UpdateTruckPhotos(UpdateTruckPhotosRequestModel model)
        {
            return await _truckRepository.UpdateTruckPhotos(model);
        }

        // New enhanced methods
        /// <summary>
        /// Get all trucks with enhanced filtering, pagination, and search
        /// </summary>
        public async Task<ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>> GetAllTrucksEnhancedAsync(GetTrucksQueryDto query)
        {
            try
            {
                // Validate query parameters
                if (query == null)
                {
                    query = new GetTrucksQueryDto();
                }

                query.ValidateAndNormalize();

                return await _truckRepository.GetAllTrucksEnhancedAsync(query);
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<PagedResponse<EnhancedTruckResponseModel>>
                {
                    IsSuccessful = false,
                    Message = $"Error in truck service: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get comprehensive truck status and approval status counts
        /// </summary>
        public async Task<ApiResponseModel<EnhancedTruckStatusCountResponseModel>> GetTruckStatusCountsEnhancedAsync()
        {
            try
            {
                return await _truckRepository.GetTruckStatusCountsEnhancedAsync();
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
                {
                    IsSuccessful = false,
                    Message = $"Error retrieving truck status counts: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get detailed truck information by ID with driver details and trip statistics
        /// </summary>
        public async Task<ApiResponseModel<TruckDetailResponseModel>> GetTruckByIdEnhancedAsync(string truckId)
        {
            try
            {
                if (string.IsNullOrEmpty(truckId))
                {
                    return new ApiResponseModel<TruckDetailResponseModel>
                    {
                        IsSuccessful = false,
                        Message = "Truck ID is required",
                        StatusCode = 400,
                        Data = null
                    };
                }

                return await _truckRepository.GetTruckByIdEnhancedAsync(truckId);
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<TruckDetailResponseModel>
                {
                    IsSuccessful = false,
                    Message = $"Error retrieving truck details: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                };
            }
        }

        /// <summary>
        /// Export trucks as CSV file
        /// </summary>
        public async Task<IActionResult> ExportTrucksAsCsvAsync(GetTrucksQueryDto query)
        {
            try
            {
                if (query == null)
                {
                    query = new GetTrucksQueryDto();
                }

                query.ValidateAndNormalize();

                var csvResponse = await _truckRepository.ExportTrucksAsCsvAsync(query);

                if (!csvResponse.IsSuccessful || csvResponse.Data == null)
                {
                    return new BadRequestObjectResult(new
                    {
                        message = csvResponse.Message,
                        success = false
                    });
                }

                var fileName = $"trucks_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                return new FileContentResult(csvResponse.Data, "text/csv")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new
                {
                    message = $"Error exporting trucks: {ex.Message}",
                    success = false
                });
            }
        }

        /// <summary>
        /// Update truck approval status with enhanced functionality
        /// </summary>
        public async Task<ApiResponseModel<string>> UpdateApprovalStatusEnhancedAsync(string truckId, UpdateApprovalStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(truckId))
                {
                    return new ApiResponseModel<string>
                    {
                        IsSuccessful = false,
                        Message = "Truck ID is required",
                        StatusCode = 400,
                        Data = null
                    };
                }

                if (request == null)
                {
                    return new ApiResponseModel<string>
                    {
                        IsSuccessful = false,
                        Message = "Update request is required",
                        StatusCode = 400,
                        Data = null
                    };
                }

                return await _truckRepository.UpdateApprovalStatusEnhancedAsync(truckId, request);
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = $"Error updating approval status: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get truck status counts by owner with enhanced details
        /// </summary>
        public async Task<ApiResponseModel<EnhancedTruckStatusCountResponseModel>> GetTruckStatusCountsByOwnerIdAsync(string ownerId)
        {
            try
            {
                if (string.IsNullOrEmpty(ownerId))
                {
                    return new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
                    {
                        IsSuccessful = false,
                        Message = "Owner ID is required",
                        StatusCode = 400,
                        Data = null
                    };
                }

                return await _truckRepository.GetTruckStatusCountsByOwnerIdAsync(ownerId);
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<EnhancedTruckStatusCountResponseModel>
                {
                    IsSuccessful = false,
                    Message = $"Error retrieving owner truck counts: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                };
            }
        }
    }
}