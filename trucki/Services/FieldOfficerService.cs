using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class FieldOfficerService : IFieldOfficerService
{
    private readonly IFieldOfficerRepository _fieldOfficerRepository;
    public FieldOfficerService(IFieldOfficerRepository fieldOfficerRepository)
    {
        _fieldOfficerRepository = fieldOfficerRepository;

    }
    public async Task<ApiResponseModel<string>> AddOfficer(AddOfficerRequestModel model)
    {
        var res = await _fieldOfficerRepository.AddOfficer(model);
        return res;
    }
    public async Task<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>> GetAllFieldOfficers(int page, int size)
    {
        var res = await _fieldOfficerRepository.GetAllFieldOfficers(page, size);
        return res;
    }
    public async Task<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>> GetAllSafetyOfficers(int page, int size)
{
    var res = await _fieldOfficerRepository.GetAllSafetyOfficers(page, size);
    return res;
}

    public async Task<ApiResponseModel<bool>> EditOfficer(EditOfficerRequestModel model)
    {
        var res = await _fieldOfficerRepository.EditOfficer(model);
        return res;
    }
    public async Task<ApiResponseModel<AllOfficerResponseModel>> GetOfficerById(string officerId)
    {
        var res = await _fieldOfficerRepository.GetOfficerById(officerId);
        return res;
    }

    public async Task<ApiResponseModel<string>> DeleteOfficers(string officerId)
    {
        var res = await _fieldOfficerRepository.DeleteOfficers(officerId);
        return res;
    }

    public async Task<ApiResponseModel<IEnumerable<AllOfficerResponseModel>>> SearchOfficer(string? searchWords)
    {
        var res = await _fieldOfficerRepository.SearchOfficer(searchWords);
        return res;
    }
    public async Task<ApiResponseModel<string>> ReassignOfficerCompany(string officerId, string newCompanyId)
    {
        var res = await _fieldOfficerRepository.ReassignOfficerCompany(officerId, newCompanyId);
        return res;
    }
}