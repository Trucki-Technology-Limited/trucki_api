using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface IFieldOfficerRepository
{
    Task<ApiResponseModel<string>> AddOfficer(AddOfficerRequestModel model);
    Task<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>> GetAllFieldOfficers(int page, int size);
    Task<ApiResponseModel<bool>> EditOfficer(EditOfficerRequestModel model);
    Task<ApiResponseModel<AllOfficerResponseModel>> GetOfficerById(string officerId);
    Task<ApiResponseModel<string>> DeleteOfficers(string officerId);
    Task<ApiResponseModel<IEnumerable<AllOfficerResponseModel>>> SearchOfficer(string? searchWords);
    Task<ApiResponseModel<string>> ReassignOfficerCompany(string officerId, string newCompanyId);
}