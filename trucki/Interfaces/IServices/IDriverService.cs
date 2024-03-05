using trucki.DTOs;

namespace trucki.Interfaces.IServices
{
    public interface IDriverService
    {
        Task<GenericResponse<string>> CreateTruckiDriverAsync(CreateDriverDto createDriver);
        Task<GenericResponse<string>> UpdateTruckiDriverAsync(CreateDriverDto createDriver);
        Task<GenericResponse<DriverResponse>> FetchTruckiDriverAsync(string driverId);
        Task<GenericResponse<IEnumerable<DriversResponse>>> FetchAllTruckiDriversAsync(DriverParameter driverParameter);
    }
}
