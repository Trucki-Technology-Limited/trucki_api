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
}