using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using trucki.CustomExtension;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository;

public class FieldOfficerRepository : IFieldOfficerRepository
{
    private readonly TruckiDBContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;
    private readonly IEmailService _emailSender;

    public FieldOfficerRepository(TruckiDBContext appDbContext, UserManager<User> userManager, IMapper mapper,
        IAuthService authService, IUploadService uploadService, IEmailService emailSender)
    {
        _context = appDbContext;
        _mapper = mapper;
        _authService = authService;
        _uploadService = uploadService;
        _emailSender = emailSender;
        _userManager = userManager;
    }


    public async Task<ApiResponseModel<string>> AddOfficer(AddOfficerRequestModel model)
    {
        var existingOfficer = await _context.Officers.FirstOrDefaultAsync(m =>
            m.EmailAddress == model.EmailAddress || m.PhoneNumber == model.PhoneNumber);
        if (existingOfficer != null)
        {
            if (existingOfficer.EmailAddress == model.EmailAddress)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Email address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
            else if (existingOfficer.PhoneNumber == model.PhoneNumber)
            {
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Phone Number address already exists",
                    StatusCode = 400 // Bad Request
                };
            }
        }

        var newOfficer = new Officer
        {
            OfficerName = model.OfficerName,
            PhoneNumber = model.PhoneNumber,
            EmailAddress = model.EmailAddress,
            OfficerType = model.OfficerType,
            CompanyId = model.CompanyId
        };

        _context.Officers.Add(newOfficer);

        var password = HelperClass.GenerateRandomPassword();
        var res = await _authService.AddNewUserAsync(newOfficer.OfficerName,
            newOfficer.EmailAddress, newOfficer.PhoneNumber, newOfficer.OfficerType == 0 ? "field officer" : "safety officer", password, false);

        if (res.StatusCode == 201)
        {
            var user = await _userManager.FindByEmailAsync(newOfficer.EmailAddress);
            newOfficer.UserId = user.Id;
            await _context.SaveChangesAsync();
            var emailSubject = "Account Created";
            await _emailSender.SendEmailAsync(newOfficer.EmailAddress, emailSubject, password);
            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                Message = "Officer Succesfully created",
                StatusCode = 201,
                Data = password
            };
        }

        return new ApiResponseModel<string>
        {
            IsSuccessful = false,
            Message = "An error occurred while creating officer",
            StatusCode = 400, // Bad Request
        };
    }

    public async Task<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>> GetAllFieldOfficers(int page,
        int size)
    {
        var fieldOfficers = await _context.Officers.Where(x => x.OfficerType == OfficerType.FieldOfficer)
            //.Skip(((page - 1) * size))
            //.Take(size)
            .ToListAsync();

        var totalItems = fieldOfficers.Count();

        var data = _mapper.Map<IEnumerable<AllOfficerResponseModel>>(fieldOfficers);
        var paginatedList = PagedList<AllOfficerResponseModel>.Paginates(data, page, size);

        return new ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>
        {
            IsSuccessful = true,
            Message = "Officers retrieved successfully",
            StatusCode = 200,
            Data = paginatedList
        };
    }

    public async Task<ApiResponseModel<bool>> EditOfficer(EditOfficerRequestModel model)
    {
        var officer = await _context.Officers.FindAsync(model.OfficerId);
        if (officer == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Officer not found",
                StatusCode = 404 // Not Found
            };
        }

        officer.OfficerName = model.OfficerName;
        officer.PhoneNumber = model.PhoneNumber;
        officer.EmailAddress = model.EmailAddress;
        officer.OfficerType = model.OfficerType;
        officer.CompanyId = model.CompanyId;
        officer.UpdatedAt = DateTime.Now.ToUniversalTime();
        // Update company list logic can be added here similar to AddManager

        _context.Officers.Update(officer);
        await _context.SaveChangesAsync();
        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Officer details updated successfully",
            StatusCode = 200,
            Data = true
        };
    }

    public async Task<ApiResponseModel<AllOfficerResponseModel>> GetOfficerById(string officerId)
    {
        var officer = await _context.Officers.Include(o => o.User)
                                             .FirstOrDefaultAsync(o => o.Id == officerId);

        if (officer == null)
        {
            return new ApiResponseModel<AllOfficerResponseModel>
            {
                StatusCode = 404,
                IsSuccessful = false,
                Message = "Officer not found",
            };
        }

        var company = await _context.Businesses.FirstOrDefaultAsync(c => c.Id == officer.CompanyId);

        var officerToReturn = _mapper.Map<AllOfficerResponseModel>(officer);
        officerToReturn.Company = company != null ? _mapper.Map<OfficerBusinessResponseModel>(company) : null;

        return new ApiResponseModel<AllOfficerResponseModel>
        {
            Data = officerToReturn,
            StatusCode = 200,
            IsSuccessful = true,
            Message = "Officer retrieved successfully"
        };
    }

    public async Task<ApiResponseModel<string>> DeleteOfficers(string officerId)
    {
        var officer = await _context.Officers.FindAsync(officerId);
        if (officer == null)
        {
            return new ApiResponseModel<string>
            {
                StatusCode = 404,
                IsSuccessful = false,
                Message = "Officer not found",
            };
        }

        _context.Officers.Remove(officer);
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            StatusCode = 200,
            IsSuccessful = true,
            Message = "Officer deleted successfully",
        };
    }

    public async Task<ApiResponseModel<IEnumerable<AllOfficerResponseModel>>> SearchOfficer(string? searchWords)
    {
        IQueryable<Officer> query = _context.Officers;

        if (!string.IsNullOrEmpty(searchWords) && searchWords != "" && searchWords != " " &&
            searchWords.ToLower() != "null")
        {
            query = query.Where(d => d.OfficerName.ToLower().Contains(searchWords.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var officers = await query.ToListAsync();

        if (!officers.Any())
        {
            return new ApiResponseModel<IEnumerable<AllOfficerResponseModel>>
            {
                Data = new List<AllOfficerResponseModel> { },
                IsSuccessful = false,
                Message = "No officer found",
                StatusCode = 404
            };
        }

        var data = _mapper.Map<IEnumerable<AllOfficerResponseModel>>(officers);

        return new ApiResponseModel<IEnumerable<AllOfficerResponseModel>>
        {
            Data = data,
            IsSuccessful = true,
            Message = "Officers successfully retrieved",
            StatusCode = 200,
        };
    }

    public async Task<ApiResponseModel<string>> ReassignOfficerCompany(string officerId, string newCompanyId)
    {
        var officer = await _context.Officers.FirstOrDefaultAsync(o => o.Id == officerId);

        if (officer == null)
        {
            return new ApiResponseModel<string>
            {
                StatusCode = 404,
                IsSuccessful = false,
                Message = "Officer not found",
            };
        }

        var newCompany = await _context.Businesses.FirstOrDefaultAsync(c => c.Id == newCompanyId);

        if (newCompany == null)
        {
            return new ApiResponseModel<string>
            {
                StatusCode = 404,
                IsSuccessful = false,
                Message = "New company not found",
            };
        }

        officer.CompanyId = newCompanyId;
        await _context.SaveChangesAsync();

        return new ApiResponseModel<string>
        {
            StatusCode = 200,
            IsSuccessful = true,
            Message = "Officer reassigned to the new company successfully",
        };
    }


}