using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trucki.DTOs;
using trucki.Entities;
using trucki.Enums;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Migrations;
using trucki.Models.ResponseModels;
using trucki.Repository;

namespace trucki.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IManagerRepository _managerRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManger;
        public ManagerService(IManagerRepository managerRepository, IMapper mapper, 
            ICompanyRepository companyRepository, UserManager<User> userManager)
        {
            _managerRepository = managerRepository;
            _userManger = userManager;
            _mapper = mapper;
        }

        public async Task<ApiResponseModel<string>> CreateTruckiManagerAsync(CreateManagerDto createManager)
        {
            // TODO get manager by userId from Session to get CompanyId

            //var userId = _httpContext.HttpContext?

            var userManager = _mapper.Map<User>(createManager);
            var user = await _userManger.FindByNameAsync(createManager.EmailAddress);
            var existingRoles = await _userManger.GetRolesAsync(user);

            if (existingRoles.Contains(createManager.Role))
            {
                return new ApiResponseModel<string>
                {
                    StatusCode = 400,
                    Message = $"Manager with the role '{createManager.Role}' already exists",
                    IsSuccessful = false,
                    Data = null
                };
            }

            // Check if the user already has the manager role

            if (user == null)
            {
                return new ApiResponseModel<string>
                {
                    StatusCode = 400,
                    Message = "Manager is not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

           /* string managerRole = string.Empty;
            if (Enum.TryParse<UserRoles>(createManager.Role, out var userRole))
            {
                managerRole = userRole.ToString();
            }*/


           /* var hasManagerRole = await _userManger.IsInRoleAsync(user, UserRoles.OperationManager.ToString())
                || await _userManger.IsInRoleAsync(user, UserRoles.FinancialManager.ToString());

            if (hasManagerRole)
            {
                return new ApiResponseModel<string>
                {
                    StatusCode = 400,
                    Message = "User is already a manager",
                    IsSuccessful = false,
                    Data = null
                };
            }*/

            var manager = _mapper.Map<Manager>(createManager);
            _managerRepository.CreateTruckiManager(manager);
            try
            {
                await _managerRepository.SaveAsync();
                return new ApiResponseModel<string>
                {
                    StatusCode = 201,
                    Message = "Manager created successfully",
                    IsSuccessful = true,
                    Data = "Manager created successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                // Handle specific database-related exceptions
                // You can log the exception details for debugging purposes
                return new ApiResponseModel<string>
                {
                    StatusCode = 500,
                    Message = "An error occurred while saving changes",
                    IsSuccessful = false,
                    Data = ex.Message // Include the exception message in the response
                };
            }

        }


      

        public async Task<ApiResponseModel<string>> UpdateTruckiManagerAsync(UpdateManagerDto updateManager)
        {
            var updateManagerDetails = await _userManger.FindByIdAsync(updateManager.UserId);
            var manager = _mapper.Map<Manager>(updateManager);

            if (updateManagerDetails == null)
            {
                return new ApiResponseModel<string>
                {
                    StatusCode = 404,
                    Message = " Manager was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            
            updateManagerDetails.PhoneNumber = updateManager.PhoneNumber;
            updateManagerDetails.Email = updateManager.EmailAddress;
            
            _managerRepository.UpdateTruckiManagern(manager);
            await _managerRepository.SaveAsync();

            return new ApiResponseModel<string>
            {
                StatusCode = 200,
                Message = "Manager updated successfully",
                IsSuccessful = true,
                Data = "Manager updated successfully"
            };

        }

        public async Task<ApiResponseModel<ManagerResponseDto>> FetchTruckiManagerAsync(string managerId)
        {
            var manager = await _managerRepository.FetchManagerById(managerId, false);

            var managerResponse = _mapper.Map<ManagerResponseDto>(manager);

            if (manager == null)
            {
                return new ApiResponseModel<ManagerResponseDto>
                {
                    StatusCode = 404,
                    Message = "Manager was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new ApiResponseModel<ManagerResponseDto>
            {
                StatusCode = 200,
                Message = "Manager Gotten successfully",
                IsSuccessful = true,
                Data = managerResponse
            };

        }

        public async Task<ApiResponseModel<IEnumerable<ManagerResponseDto>>> FetchAllTruckiManagersAsync(ManagerParameter managerParameter)
        {
            var managers = await _managerRepository.FetchTruckiManagers(managerParameter);

            var managerResponse = _mapper.Map<IEnumerable<ManagerResponseDto>>(managers);

            if (managers == null)
            {
                return new ApiResponseModel<IEnumerable<ManagerResponseDto>>
                {
                    StatusCode = 404,
                    Message = "Manager was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new ApiResponseModel<IEnumerable<ManagerResponseDto>>
            {
                StatusCode = 200,
                Message = "All Drivers Gotten successfully",
                IsSuccessful = true,
                Data = managerResponse
            };

        }
    }
}



/*public async Task<ApiResponseModel<string>> CreateTruckiManagerAsync(CreateManagerDto createManager)
      {
          // TODO get manager by userId from Session to get CompanyId
          var manager = _mapper.Map<Manager>(createManager);

          if (manager == null)
          {
              return new ApiResponseModel<string>
              {
                  StatusCode = "01",
                  Message = "Manager was not found",
                  IsSuccessful = false,
                  Data = null
              };
          }

          _managerRepository.CreateTruckiManager(manager);
          await _managerRepository.SaveAsync();

          return new ApiResponseModel<string>
          {
              StatusCode = "00",
              Message = "Manager created successfully",
              IsSuccessful = true,
              Data = "Manager created successfully"
          };

      }*/