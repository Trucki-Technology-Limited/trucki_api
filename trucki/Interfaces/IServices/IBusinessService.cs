using trucki.DTOs;
using trucki.Entities;

namespace trucki.Interfaces.IServices
{
    public interface IBusinessService
    {
        Task<GenericResponse<string>> CreateTruckiBusinessAsync(CreateBusinessDto createBusiness);
        Task<GenericResponse<string>> UpdateTruckiBusinessAsync(CreateBusinessDto createBusiness);
        Task<GenericResponse<BusinessResponse>> FetchTruckiBusinessAsync(string driverId);
        Task<GenericResponse<IEnumerable<BusinessResponse>>> FetchAllTruckiBusinessesAsync(BusinessParameter driverParameter);
        Task<GenericResponse<string>> DeleteTruckiBusinessAsync(string businessId);
    }
}
