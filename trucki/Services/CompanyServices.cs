using AutoMapper;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;

namespace trucki.Services
{
    public class CompanyServices : ICompanyServices
    {

        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository; 
        public CompanyServices(ICompanyRepository companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
            
        }

        public async Task<GenericResponse<string>> CreateTruckiComapnyAsync(CreateCompanyDto createCompany)
        {
            var company = _mapper.Map<Company>(createCompany);

            if (company == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Company was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _companyRepository.CreateTruckiCompanies(company); 
            await _companyRepository.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Company created successfully",
                IsSuccessful = true,
                Data = "Company created successfully"
            };

        }

        public async Task<GenericResponse<string>> UpdateTruckiComapnyAsync(CreateCompanyDto createCompany)
        {

            var company = _mapper.Map<Company>(createCompany);
    
            if (company == null)
            {
                return new GenericResponse<string>
                {
                    ResponseCode = "01",
                    ResponseMessage = " Company was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }
            _companyRepository.UpdateTruckiCompanies(company);
            await _companyRepository.SaveAsync();

            return new GenericResponse<string>
            {
                ResponseCode = "00",
                ResponseMessage = "Company updated successfully",
                IsSuccessful = true,
                Data = "Company updated successfully"
            };

        }

        public async Task<GenericResponse<CompanyResponseDto>> FetchTruckiDriverAsync(string companyId)
        {
            var company = await _companyRepository.FetchComapnyById(companyId, false);

            var companyResponse = _mapper.Map<CompanyResponseDto>(company);

            if (company == null)
            {
                return new GenericResponse<CompanyResponseDto>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Driver was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new GenericResponse<CompanyResponseDto>
            {
                ResponseCode = "00",
                ResponseMessage = "Driver Gotten successfully",
                IsSuccessful = true,
                Data = companyResponse
            };

        }

        public async Task<GenericResponse<IEnumerable<CompanyResponseDto>>> FetchAllTruckiDriversAsync(CompanyParameter companyParameter)
        {
            var companies = await _companyRepository.FetchAllTruckiCompanies(companyParameter);

            var companyResponse = _mapper.Map<IEnumerable<CompanyResponseDto>>(companies);

            if (companies == null)
            {
                return new GenericResponse<IEnumerable<CompanyResponseDto>>
                {
                    ResponseCode = "01",
                    ResponseMessage = "Drivers was not found",
                    IsSuccessful = false,
                    Data = null
                };
            }

            return new GenericResponse<IEnumerable<CompanyResponseDto>>
            {
                ResponseCode = "00",
                ResponseMessage = "All Drivers Gotten successfully",
                IsSuccessful = true,
                Data = companyResponse
            };

        }
    }
}
