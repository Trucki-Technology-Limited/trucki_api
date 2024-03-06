using trucki.DTOs;

namespace trucki.Interfaces.IServices
{
    public interface ICompanyServices
    {
        Task<GenericResponse<string>> CreateTruckiComapnyAsync(CreateCompanyDto createCompany);
        Task<GenericResponse<string>> UpdateTruckiComapnyAsync(CreateCompanyDto createCompany);
    }
}
