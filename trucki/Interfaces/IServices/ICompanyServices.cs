using trucki.DTOs;

namespace trucki.Interfaces.IServices
{
    public interface ICompanyServices
    {
        Task<GenericResponse<string>> CreateTruckiComapnyAsync(CreateCompanyDto createCompany);
        Task<GenericResponse<string>> UpdateTruckiComapnyAsync(CreateCompanyDto createCompany);
        Task<GenericResponse<IEnumerable<CompanyResponseDto>>> FetchAllTruckiDriversAsync(CompanyParameter companyParameter);
        Task<GenericResponse<CompanyResponseDto>> FetchTruckiDriverAsync(string companyId);

    }
}
