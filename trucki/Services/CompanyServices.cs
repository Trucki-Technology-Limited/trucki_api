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
        public CompanyServices(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
            
        }

        public async Task<GenericResponse<string>> CreateTruckiComapnyAsync(CreateCompanyDto createCompany)
        {
            // var company = _mapper.Map<Company>(createCompany);

            var company = new Company
            {
                Name = createCompany.Name,
                Address = createCompany.Address,
                EmailAddress = createCompany.EmailAddress,
                PhoneNumber = createCompany.PhoneNumber,
                ManageName = createCompany.ManagerName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
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
           //Create a  Company put manager Id and Manager create 


            //var company = _mapper.Map<Company>(createCompany);
            var company = new Company
            {
                Name = createCompany.Name,
                Address = createCompany.Address,
                ManageName = createCompany.ManagerName,
                EmailAddress = createCompany.EmailAddress,
                PhoneNumber = createCompany.PhoneNumber,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
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
    }
}
