using AutoMapper;
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
        public BusinessService(IBusinessRepository businessRepository, IMapper mapper)
        {
            _businessRepository = businessRepository;
            _mapper = mapper;
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
            var driver = await _businessRepository.FetchBusinessById(createBusiness.Id, false);
            if (driver == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Business was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _businessRepository.UpdateTruckiBusiness(driver);
            await _businessRepository.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Business Updated successfully",
                IsSuccessful = true,
                Data = "Business created successfully"
            };

        }


        public async Task<GenericResponse<BusinessResponse>> FetchTruckiBusinessAsync(string driverId)
        {
            var driver = await _businessRepository.FetchBusinessById(driverId, false);

            var driverResponse = _mapper.Map<BusinessResponse>(driver);

            if (driver == null)
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
                Data = driverResponse
            };

        }

        public async Task<GenericResponse<IEnumerable<BusinessResponse>>> FetchAllTruckiBusinessesAsync(BusinessParameter driverParameter)
        {
            var driver = await _businessRepository.FetchAllTruckiBusinesses(driverParameter);

            var driverResponse = _mapper.Map<IEnumerable<BusinessResponse>>(driver);

            if (driver == null)
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
                ResponseMessage = "All Businesss Deleted successfully",
                IsSuccessful = true,
                Data = driverResponse
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

