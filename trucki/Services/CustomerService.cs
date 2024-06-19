using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class CustomerService: ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;

    }
    public async Task<ApiResponseModel<string>> AddNewCustomer(AddCustomerRequestModel model)
    {
        var res = await _customerRepository.AddNewCustomer(model);
        return res;
    }

    public async Task<ApiResponseModel<string>> EditCustomer(EditCustomerRequestModel model)
    {
        var res = await _customerRepository.EditCustomer(model);
        return res;
    }
    public async Task<ApiResponseModel<AllCustomerResponseModel>> GetCustomerById(string customerId)
    {
        var res = await _customerRepository.GetCustomerById(customerId);
        return res;
    }
    public async Task<ApiResponseModel<List<AllCustomerResponseModel>>> GetAllCustomers()
    {
        var res = await _customerRepository.GetAllCustomers();
        return res;
    }
    public async Task<ApiResponseModel<string>> DeleteCustomer(string customerId)
    {
        var res = await _customerRepository.DeleteCustomer(customerId);
        return res;
    }
    public async Task<ApiResponseModel<IEnumerable<AllCustomerResponseModel>>> SearchCustomers(string searchWords)
    {
        var res = await _customerRepository.SearchCustomers(searchWords);
        return res;
    }
}