using trucki.DTOs;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices
{
    public interface IManagerService
    {
        //Task<GenericResponse<string>> CreateTruckiManagerAsync(CreateManagerDto createManager);

        Task<ApiResponseModel<string>> CreateTruckiManagerAsync(CreateManagerDto createManager);
        Task<ApiResponseModel<string>> UpdateTruckiManagerAsync(UpdateManagerDto updateManager);
        Task<ApiResponseModel<ManagerResponseDto>> FetchTruckiManagerAsync(string managerId);
        Task<ApiResponseModel<IEnumerable<ManagerResponseDto>>> FetchAllTruckiManagersAsync(ManagerParameter managerParameter);
    }
}
