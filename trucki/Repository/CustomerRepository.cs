using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;
    private readonly IEmailService _emailSender;

    public CustomerRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;
        _userManager = userManager;
    }

    public async Task<ApiResponseModel<string>> AddNewCustomer(AddCustomerRequestModel model)
    {
        var business = await _context.Businesses
            .Include(b => b.Customers)
            .FirstOrDefaultAsync(b => b.Id == model.BusinessId);

        if (business == null)
        {
            // If business does not exist, return appropriate response
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Business does not exist",
                StatusCode = 404,
                Data = "false"
            };
        }

        var existingCustomer = await _context.Customers.FirstOrDefaultAsync(m =>
            m.EmailAddress == model.EmailAddress || m.PhoneNumber == model.PhoneNumber);
        if (existingCustomer != null)
        {
            if (existingCustomer.EmailAddress == model.EmailAddress)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Email address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
            else if (existingCustomer.PhoneNumber == model.PhoneNumber)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Phone Number address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
        }

        var newCustomer = new Customer
        {
            CustomerName = model.CustomerName,
            PhoneNumber = model.PhoneNumber,
            EmailAddress = model.EmailAddress,
            Location = model.Location,
            BusinessId = model.BusinessId // Set the BusinessId
        };

        _context.Customers.Add(newCustomer);
        await _context.SaveChangesAsync();

        business.Customers ??= new List<Customer>();
        business.Customers.Add(newCustomer);

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Customer added successfully",
            StatusCode = 201
        };
    }

    public async Task<ApiResponseModel<string>> EditCustomer(EditCustomerRequestModel model)
    {
        var customer = await _context.Customers.FindAsync(model.CustomerId);
        if (customer == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Customer not found",
                StatusCode = 404
            };
        }

        customer.CustomerName = model.CustomerName;
        customer.PhoneNumber = model.PhoneNumber;
        customer.EmailAddress = model.EmailAddress;
        customer.Location = model.Location;
        customer.RCNo = model.RCNo;
        customer.UpdatedAt = DateTime.Now.ToUniversalTime();

        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Customer updated successfully",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<AllCustomerResponseModel>> GetCustomerById(string customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
        {
            return new ApiResponseModel<AllCustomerResponseModel>
            {
                Data = new AllCustomerResponseModel { },
                IsSuccessful = false,
                StatusCode = 404,
                Message = "Customer not found"
            };
        }

        //var managerToReturn = _mapper.Map<AllManagerResponseModel>(manager);
        var customerToReturn = _mapper.Map<AllCustomerResponseModel>(customer);

        return new ApiResponseModel<AllCustomerResponseModel>
        {
            Data = customerToReturn,
            IsSuccessful = true,
            StatusCode = 200,
            Message = "Customer successfully retrived"
        };
    }

    public async Task<ApiResponseModel<List<AllCustomerResponseModel>>> GetAllCustomers()
    {
        var customers = await _context.Customers.ToListAsync();
        if (customers == null)
        {
            return new ApiResponseModel<List<AllCustomerResponseModel>>
            {
                Data = new List<AllCustomerResponseModel> { },
                IsSuccessful = false,
                StatusCode = 404,
                Message = "No customers found"
            };
        }

        var customersToReturn = _mapper.Map<List<AllCustomerResponseModel>>(customers);

        return new ApiResponseModel<List<AllCustomerResponseModel>>
        {
            Data = customersToReturn,
            IsSuccessful = true,
            StatusCode = 200,
            Message = "Customers retrived successfully"
        };
    }

    public async Task<ApiResponseModel<string>> DeleteCustomer(string customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
        {
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                Message = "Customer not found",
                StatusCode = 404
            };
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            IsSuccessful = true,
            Message = "Customer deleted",
            StatusCode = 200
        };
    }
    public async Task<ApiResponseModel<IEnumerable<AllCustomerResponseModel>>> SearchCustomers(string searchWords)
    {
        IQueryable<Customer> query = _context.Customers;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " &&
            searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.CustomerName.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var customers = await query.ToListAsync();

        if (!customers.Any())
        {
            return new ApiResponseModel<IEnumerable<AllCustomerResponseModel>>
            {
                Data = new List<AllCustomerResponseModel> { },
                IsSuccessful = false,
                Message = "No Customer found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllCustomerResponseModel>>(customers);

        return new ApiResponseModel<IEnumerable<AllCustomerResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Customers successfully retrieved",
            StatusCode = 200,
        };
    }
    public async Task<ApiResponseModel<List<AllCustomerResponseModel>>> GetCustomersByBusinessId(string businessId)
    {
        // Check if the business exists
        var businessExists = await _context.Businesses.AnyAsync(b => b.Id == businessId);
        if (!businessExists)
        {
            return new ApiResponseModel<List<AllCustomerResponseModel>>
            {
                IsSuccessful = false,
                Message = "Business not found",
                StatusCode = 404,
                Data = new List<AllCustomerResponseModel>()
            };
        }

        // Retrieve customers associated with the given business ID
        var customers = await _context.Customers
            .Where(c => c.BusinessId == businessId)
            .ToListAsync();

        if (!customers.Any())
        {
            return new ApiResponseModel<List<AllCustomerResponseModel>>
            {
                IsSuccessful = false,
                Message = "No customers found for the specified business",
                StatusCode = 404,
                Data = new List<AllCustomerResponseModel>()
            };
        }

        // Map the customers to the response model
        var customersToReturn = _mapper.Map<List<AllCustomerResponseModel>>(customers);

        return new ApiResponseModel<List<AllCustomerResponseModel>>
        {
            IsSuccessful = true,
            Message = "Customers retrieved successfully",
            StatusCode = 200,
            Data = customersToReturn
        };
    }


}