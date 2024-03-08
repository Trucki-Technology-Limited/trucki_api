using trucki.DTOs;

namespace trucki.Interfaces.IServices
{
    public interface IManagerService
    {
        Task<GenericResponse<string>> CreateTruckiManagerAsync(CreateManagerDto createManager);
        Task<GenericResponse<string>> UpdateTruckiManagerAsync(CreateManagerDto createManager);
        Task<GenericResponse<ManagerResponseDto>> FetchTruckiManagerAsync(string managerId);
        Task<GenericResponse<IEnumerable<ManagerResponseDto>>> FetchAllTruckiManagersAsync(ManagerParameter managerParameter);
    }
}
