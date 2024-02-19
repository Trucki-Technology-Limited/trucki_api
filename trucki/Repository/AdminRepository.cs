using AutoMapper;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class AdminRepository: IAdminRepository
{
    
    private readonly TruckiDBContext _context;
    private readonly IMapper _mapper;
    public AdminRepository(TruckiDBContext appDbContext, IMapper mapper) 
    {
        _context = appDbContext;
        _mapper = mapper;
    }

    public async Task<ApiResponseModel<bool>> CreateNewBusiness(CreateNewBusinessRequestModel model)
    {
        var newBusiness = new Business
        {
            Name = model.Name,
            Ntons = model.Ntons,
            Address = model.Address,
            isActive = true
        };
        _context.Businesses.Add(newBusiness);
        await _context.SaveChangesAsync();
        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Business created successfully",
            StatusCode = 201,
            Data = true
        };
    }
    public async Task<ApiResponseModel<List<AllBusinessResponseModel>>> GetAllBusiness()
    {
        var businesses = await _context.Businesses.ToListAsync();
        
        var businessResponseModels = _mapper.Map<List<AllBusinessResponseModel>>(businesses);

        return new ApiResponseModel<List<AllBusinessResponseModel>>
        {
            IsSuccessful = true,
            Message = "Businesses retrieved successfully",
            StatusCode = 200,
            Data = businessResponseModels
        };
    }
    public async Task<ApiResponseModel<bool>> AddRouteToBusiness(AddRouteToBusinessRequestModel model)
    {
        // Check if the business with the provided BusinessId exists
        var business = await _context.Businesses
            .Include(b => b.Routes)
            .FirstOrDefaultAsync(b => b.Id == model.BusinessId);

        if (business == null)
        {
            // If business does not exist, return appropriate response
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Business does not exist",
                StatusCode = 404,
                Data = false
            };
        }

        // Business exists, proceed to add the route
        var newRoute = new Routes
        {
            Name = model.Name,
            FromRoute = model.FromRoute,
            ToRoute = model.ToRoute,
            Price = model.Price,
            IsActive = model.IsActive
        };

        // Add the route to the business
        business.Routes ??= new List<Routes>();
        business.Routes.Add(newRoute);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Route added to business successfully",
            StatusCode = 201,
            Data = true
        };
    }
    public async Task<ApiResponseModel<BusinessResponseModel>> GetBusinessById(string id)
    {
        // Retrieve the business by ID including its routes
        var business = await _context.Businesses
            .Include(b => b.Routes)
            .FirstOrDefaultAsync(b => b.Id == id);

        // Check if business with the provided ID exists
        if (business == null)
        {
            return new ApiResponseModel<BusinessResponseModel>
            {
                IsSuccessful = false,
                Message = "Business not found",
                StatusCode = 404,
                Data = null
            };
        }

        // Map the business entity to a response model
        var businessResponseModel = _mapper.Map<BusinessResponseModel>(business);

        return new ApiResponseModel<BusinessResponseModel>
        {
            IsSuccessful = true,
            Message = "Business retrieved successfully",
            StatusCode = 200,
            Data = businessResponseModel
        };
    }
}