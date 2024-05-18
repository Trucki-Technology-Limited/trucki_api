
using System.Net;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly TruckiDBContext _context;
    private readonly IUploadService _uploadService;
    private readonly IMapper _mapper;

    public AuthService(UserManager<User> userManager, TruckiDBContext context, IMapper mapper, ITokenService tokenService, IUploadService uploadService
    
    )
    {
        _userManager = userManager;
        _mapper = mapper;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tokenService = tokenService;
        _uploadService = uploadService;
    }
    public async Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        if (!(Regex.IsMatch(request.email, emailPattern)))
            return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, Message = "Invalid email address format", StatusCode = 400 };

        var validityResult = await ValidateUser(request);
        if (!validityResult.IsSuccessful)
        {
            return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, Message = validityResult.Message, StatusCode = 400 };
        }

        var user = await _userManager.FindByEmailAsync(request.email);
        var role = await _userManager.GetRolesAsync(user);
        
        var tokenResponse = await _tokenService.GetToken(user.UserName, request.password);
        if (tokenResponse.IsError)
        {
            return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, StatusCode = 500, Message = "Unknown error getting access token" };
        }
        var responseDto = new ApiResponseModel<LoginResponseModel>
        {
            Data = new LoginResponseModel
            {

                Id = user.Id,
                UserName = user.UserName,
                Token = tokenResponse.AccessToken,
                Role = role,
                FirstName = user.firstName,
                LastName = user.lastName,
                RefreshToken = tokenResponse.RefreshToken,
                EmailAddress = user.Email,
                //isPasswordChanged = user.IsPasswordChanged,
                isEmailConfirmed = user.EmailConfirmed,
                isPhoneNumberConfirmed = user.PhoneNumberConfirmed,
                LastLoginDate = DateTime.Now
            }

        };

        return ApiResponseModel<LoginResponseModel>.Success("User created successfully", responseDto.Data, StatusCodes.Status201Created);

    }

        private async Task<ApiResponseModel<bool>> ValidateUser(LoginRequestModel loginRequest)
        {
            var response = new ApiResponseModel<bool>();
            var user = await _userManager.FindByNameAsync(loginRequest.email);
            if (user == null)
            {
                response.Message = "Email does not exist";
                response.IsSuccessful = false;
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                return response;
            }
            if (!await _userManager.CheckPasswordAsync(user, loginRequest.password))
            {
                response.Message = "Password is incorrect";
                response.IsSuccessful = false;
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                return response;
            }
            //TODO:: Implement email Confirmation
            //if (!await _userManager.IsEmailConfirmedAsync(user))
            //{
            //    return new ApiResponseModel<bool> { Message = "Email Address not confirmed", StatusCode = (int)HttpStatusCode.UnprocessableEntity, IsSuccessful = false };
            //}


            response.IsSuccessful = true;
            return response;

        }
        
        public async Task<ApiResponseModel<bool>> AddNewUserAsync(string name, string email, string role, string password)
        {
            var user = new User
            {
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                EmailConfirmed = true,
                PasswordHash =
                    new PasswordHasher<User>().HashPassword(null,
                        password),
                SecurityStamp = string.Empty,
                firstName = name,
                lastName = name,
                IsActive = true,
            };

            var result = await _userManager.CreateAsync(user);
            switch (result.Succeeded)
            {
                case true:
                    await _userManager.AddToRoleAsync(user, role);
                    break;
                case false:
                    // Handle user creation failure
                    return new ApiResponseModel<bool>
                    {
                        IsSuccessful = false,
                        Message = result.Errors.FirstOrDefault()?.Description ?? "Failed to create user",
                        StatusCode = 400
                    };
            }

            return new ApiResponseModel<bool>
            {
                IsSuccessful = true,
                Message = "User created successfully",
                StatusCode = 201
            };

        }
    public async Task<ApiResponseModel<bool>> RegisterTransporterAsync(RegisterTransporterRequestModel model)
    {
        string[] fullName = model.FullName.Split(' ');
        if (fullName.Length == 2)
        {
            var user = new User
            {
                UserName = model.EmailAddress,
                NormalizedUserName = model.EmailAddress.ToUpper(),
                Email = model.EmailAddress,
                NormalizedEmail = model.EmailAddress.ToUpper(),
                PasswordHash =
                    new PasswordHasher<User>().HashPassword(null,
                        model.Password),
                SecurityStamp = string.Empty,
                firstName = fullName[0],
                lastName = fullName[1],
                IsActive = false
            };
            var result = await _userManager.CreateAsync(user);
            switch (result.Succeeded)
            {
                case true:
                    await _userManager.AddToRoleAsync(user, "Transporter");
                    break;
                case false:
                    // Handle user creation failure
                    return new ApiResponseModel<bool>
                    {
                        IsSuccessful = false,
                        Message = result.Errors.FirstOrDefault()?.Description ?? "Failed to create user",
                        StatusCode = 400
                    };
            }
            var transporterDetails = new TransporterUser
            {
                UserId = user.Id,
                Location = model.Location,
                NumberOfDrivers = model.NumberOfDrivers,
                TruckType = model.TruckType,
                TruckCapacity = model.TruckCapacity,
                TruckLicenseExpiryDate = model.TruckLicenseExpiryDate,
                RoadWorthinessExpiryDate = model.RoadWorthinessExpiryDate,
                InsruanceExpiryDate = model.InsruanceExpiryDate,
                //Documents = model.Documents
            };
            List<string> documents = new List<string>();

            if (model.Documents != null)
            {
                foreach (var document in model.Documents)
                {
                    documents.Add(await _uploadService.UploadFile(document, $"{transporterDetails.Id}"));
                }
            }

            transporterDetails.Documents = documents;

            // Step 4: Save transporter details to the database
            _context.TransporterUsers.Add(transporterDetails);
            await _context.SaveChangesAsync();
        }
        else
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Please enter just your firstname and lastname",
                StatusCode = 400
            };
        }
        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "User created successfully",
            StatusCode = 201
        };

    }
}