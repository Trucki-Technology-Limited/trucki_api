using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository;

public interface ICustomerRepository
{
    Task<ApiResponseModel<string>> AddNewCustomer(AddCustomerRequestModel model);
    Task<ApiResponseModel<string>> EditCustomer(EditCustomerRequestModel model);
    Task<ApiResponseModel<AllCustomerResponseModel>> GetCustomerById(string customerId);
    Task<ApiResponseModel<List<AllCustomerResponseModel>>> GetAllCustomers();
    Task<ApiResponseModel<string>> DeleteCustomer(string customerId);
    Task<ApiResponseModel<IEnumerable<AllCustomerResponseModel>>> SearchCustomers(string searchWords);
}