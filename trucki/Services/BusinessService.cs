using AutoMapper;
using System.Security.Claims;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;

namespace trucki.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        public BusinessService(IBusinessRepository businessRepository, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _businessRepository = businessRepository;
            _mapper = mapper;
            _httpContext = httpContext;

        }

        public async Task<GenericResponse<string>> CreateTruckiBusinessAsync(CreateBusinessDto createBusiness)
        {
            var business = _mapper.Map<Business>(createBusiness);
            if (business == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Business was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _businessRepository.CreateTruckiBusiness(business);
            await _businessRepository.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Business created successfully",
                IsSuccessful = true,
                Data = "Business created successfully"
            };

        }

        public async Task<GenericResponse<string>> UpdateTruckiBusinessAsync(CreateBusinessDto createBusiness)
        {
            var userId = _httpContext.HttpContext.User.Claims.Where(c => c.Type == "Id").First().Value;

            var business = await _businessRepository.FetchBusinessById(userId, false);
            if  (business == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Business was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _businessRepository.UpdateTruckiBusiness(business);
            await _businessRepository.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Business Updated successfully",
                IsSuccessful = true,
                Data = "Business created successfully"
            };

        }


        public async Task<GenericResponse<BusinessResponse>> FetchTruckiBusinessAsync(string businessId)
        {
            var business = await _businessRepository.FetchBusinessById(businessId, false);

            var businessResponse = _mapper.Map<BusinessResponse>(business);

            if  (business == null)
            {
                return new GenericResponse<BusinessResponse>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Business was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new GenericResponse<BusinessResponse>
            {
                ResponseCode = "00",
                ResponseMessage = "Business Gotten successfully",
                IsSuccessful = true,
                Data = businessResponse
            };

        }

        public async Task<GenericResponse<IEnumerable<BusinessResponse>>> FetchAllTruckiBusinessesAsync(BusinessParameter businessParameter)
        {
            var business = await _businessRepository.FetchAllTruckiBusinesses(businessParameter);

            var businessResponse = _mapper.Map<IEnumerable<BusinessResponse>>(business);

            if  (business == null)
            {
                return new GenericResponse<IEnumerable<BusinessResponse>>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Businesss was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new GenericResponse<IEnumerable<BusinessResponse>>
            {
                ResponseCode = "00",
                ResponseMessage = "All Businessses Gotten successfully",
                IsSuccessful = true,
                Data = businessResponse
            };

        }



        public async Task<GenericResponse<string>> DeleteTruckiBusinessAsync(string businessId)
        {
            var business = await _businessRepository.FetchBusinessById(businessId, false);
            if (business == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Business was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _businessRepository.DeleteTruckiBusiness(business);
            await _businessRepository.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Business Deleted successfully",
                IsSuccessful = true,
                Data = "Business Deleted successfully"
            };

        }


    }

}

