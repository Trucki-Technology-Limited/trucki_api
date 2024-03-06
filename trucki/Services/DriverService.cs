
using AutoMapper;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;

namespace trucki.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driver;
        private readonly IMapper _mapper;
        public DriverService(IDriverRepository driver, IMapper mapper)
        {
            _driver = driver;
            _mapper = mapper;
            
        }

        public async Task<GenericResponse<string>> CreateTruckiDriverAsync(CreateDriverDto createDriver)
        {
            var driver = _mapper.Map<Driver>(createDriver); 
            if (driver == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Driver was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _driver.CreateTruckiDriver(driver);
            await _driver.SaveAsync();
           
            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Driver created successfully",
                IsSuccessful = true,
                Data = "Driver created successfully"
            };

        }

        public async Task<GenericResponse<string>> UpdateTruckiDriverAsync(CreateDriverDto createDriver)
        {
            var driver = await _driver.FetchDriverById(createDriver.Id, false);
            if (driver == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Driver was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _driver.UpdateTruckiDrivern(driver);
            await _driver.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Driver Updated successfully",
                IsSuccessful = true,
                Data = "Driver created successfully"
            };

        }


        public async Task<GenericResponse<DriverResponse>> FetchTruckiDriverAsync(string driverId)
        {
            var driver = await _driver.FetchDriverById(driverId, false);

             var  driverResponse = _mapper.Map<DriverResponse>(driver); 

            if (driver == null)
            {
                return new GenericResponse<DriverResponse>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Driver was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new GenericResponse<DriverResponse>
            {
                ResponseCode = "00",
                ResponseMessage = "Driver Gotten successfully",
                IsSuccessful = true,
                Data = driverResponse
            };

        }

        public async Task<GenericResponse<IEnumerable<DriversResponse>>> FetchAllTruckiDriversAsync(DriverParameter driverParameter)
        {
            var driver = await _driver.FetchTruckiDrivers(driverParameter);

            var driverResponse = _mapper.Map<IEnumerable<DriversResponse>>(driver);

            if (driver == null)
            {
                return new GenericResponse<IEnumerable<DriversResponse>>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Drivers was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new GenericResponse<IEnumerable<DriversResponse>>
            {
                ResponseCode = "00",
                ResponseMessage = "All Drivers Gotten successfully",
                IsSuccessful = true,
                Data = driverResponse
            };

        }


    }
}
