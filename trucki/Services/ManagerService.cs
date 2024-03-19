using AutoMapper;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Repository;

namespace trucki.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IManagerRepository _managerRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        public ManagerService(IManagerRepository managerRepository, IMapper mapper, ICompanyRepository companyRepository)
        {
            _managerRepository = managerRepository;
            _mapper = mapper;
            _companyRepository = companyRepository;
        }

        public async Task<GenericResponse<string>> CreateTruckiManagerAsync(CreateManagerDto createManager)
        {
            // TODO get manager by userId from Session to get CompanyId
            var manager = _mapper.Map<Manager>(createManager);

            if (manager == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Manager was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            _managerRepository.CreateTruckiManager(manager);
            await _managerRepository.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Manager created successfully",
                IsSuccessful = true,
                Data = "Manager created successfully"
            };

        }

        public async Task<GenericResponse<string>> UpdateTruckiManagerAsync(CreateManagerDto createManager)
        {

            var manager = _mapper.Map<Manager>(createManager);

            if (manager == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = " Manager was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _managerRepository.UpdateTruckiManagern(manager);
            await _managerRepository.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Manager updated successfully",
                IsSuccessful = true,
                Data = "Manager updated successfully"
            };

        }

        public async Task<GenericResponse<ManagerResponseDto>> FetchTruckiManagerAsync(string managerId)
        {
            var manager = await _managerRepository.FetchManagerById(managerId, false);

            var managerResponse = _mapper.Map<ManagerResponseDto>(manager);

            if (manager == null)
            {
                return new GenericResponse<ManagerResponseDto>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Driver was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new GenericResponse<ManagerResponseDto>
            {
                ResponseCode = "00",
                ResponseMessage = "Driver Gotten successfully",
                IsSuccessful = true,
                Data = managerResponse
            };

        }

        public async Task<GenericResponse<IEnumerable<ManagerResponseDto>>> FetchAllTruckiManagersAsync(ManagerParameter managerParameter)
        {
            var companies = await _managerRepository.FetchTruckiManagers(managerParameter);

            var managerResponse = _mapper.Map<IEnumerable<ManagerResponseDto>>(companies);

            if (companies == null)
            {
                return new GenericResponse<IEnumerable<ManagerResponseDto>>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Drivers was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new GenericResponse<IEnumerable<ManagerResponseDto>>
            {
                ResponseCode = "00",
                ResponseMessage = "All Drivers Gotten successfully",
                IsSuccessful = true,
                Data = managerResponse
            };

        }
    }
}

