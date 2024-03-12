
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Transactions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using trucki.DatabaseContext;
using trucki.DTOs;
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
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, TruckiDBContext context, IMapper mapper,
        ITokenService tokenService, IConfiguration configuration)
    {
        _userManager = userManager;
        _mapper = mapper;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tokenService = tokenService;
        _configuration = configuration;
    }
        public async Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request)
        {
            var validityResult = await ValidateUser(request);
            if (!validityResult.IsSuccessful)
            {
                return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, Message = validityResult.Message, StatusCode = 400 };
            }
        //var user = await _userManager.FindByNameAsync(request.email);
            var user = await _userManager.FindByEmailAsync(request.email);
            var tokenResponse = await _tokenService.GetToken(user.UserName, request.password);
            if (tokenResponse.IsError)
            {
                return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, StatusCode = 500, Message = "Unknown error getting access token" };
            }


       // var user = await _userManager.FindByEmailAsync(loginRequest.Email);

       // var tokenResponse = await _tokenService.GetToken(user.UserName, loginRequest.Password);
        var role = await _userManager.GetRolesAsync(user);

        //Get the user permissions if any is avaialable
        List<Claim> getUserClaims = (List<Claim>)await _userManager.GetClaimsAsync(user);
        List<string> adminPermission = new();
        foreach (var userClaim in getUserClaims)
        {
            adminPermission.Add(userClaim.Value);
        }
        var responseDto = new LoginResponseModel()
        {
            Id = user.Id,
            UserName = $"{user.firstName} {user.lastName}",
            Token = tokenResponse.AccessToken,
            Role = role,
            RefreshToken = tokenResponse.RefreshToken,
            Permissions = adminPermission,
            EmailAddress = user.Email,
            //isPasswordChanged = user.IsPassWordChanged,
            isEmailConfirmed = user.EmailConfirmed,
            isPhoneNumberConfirmed = user.PhoneNumberConfirmed
        };

   /*     var responseDto = new LoginResponseModel
            {
                Id = user.Id,
                RefreshToken = tokenResponse.RefreshToken,
                Token = tokenResponse.AccessToken,
                UserName = $"{user.firstName} {user.lastName}",
            };*/

            return new ApiResponseModel<LoginResponseModel> { IsSuccessful = true, Message = "Success", StatusCode = 200, Data = responseDto };
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

    public async Task<ApiResponseModel<CreatTruckiUserResponseDto>> RegisterTruckiAsync(CreatTruckiUserDto registrationRequest)
    {

        var ExistingUser = await _userManager.FindByEmailAsync(registrationRequest.Email);

        var user = new User
        {
            Email = registrationRequest.Email,
            firstName = registrationRequest.Name,
            lastName = "",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            EmailConfirmed = true,
            Id = Guid.NewGuid().ToString(),
            IsActive = true,
            PhoneNumber = "",
            Role = registrationRequest.Role,
            UserName = registrationRequest.Email

        };

        if (ExistingUser != null)
        {
            var roles = await _userManager.GetRolesAsync(ExistingUser);
            if (roles.Contains(user.Role))
                return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"User already has {user.Role} role", StatusCodes.Status400BadRequest);



            await _context.SaveChangesAsync();
            await _userManager.AddToRoleAsync(ExistingUser, "admin");
            // Add the Permission Coming in as a claim for the sponsor..
            var userPermission = new List<Claim>();
            foreach (var permission in registrationRequest.Permissions)
            {
                var claim = new Claim("Permission", permission);
                userPermission.Add(claim);
            }
            await _userManager.AddClaimsAsync(user, userPermission);

            return ApiResponseModel<CreatTruckiUserResponseDto>.Success("User created successfully", new CreatTruckiUserResponseDto
            {
                Id = ExistingUser.Id,
            }, 201);
        }
        if (ExistingUser == null)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {

                var result = await _userManager.CreateAsync(user, registrationRequest.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Sponsor");
                    // Add the Permission Coming in as a claim for the sponsor..
                    var userPermission = new List<Claim>();
                    foreach (var permission in registrationRequest.Permissions)
                    {
                        var claim = new Claim("Permission", permission);
                        userPermission.Add(claim);
                    }
                    await _userManager.AddClaimsAsync(user, userPermission);

                    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var emailconf = await _userManager.ConfirmEmailAsync(user, emailToken);
                }
                transaction.Complete();
            }

            var currentUser = await _userManager.FindByEmailAsync(registrationRequest.Email);
            var tokenResponse = await _tokenService.GetToken(currentUser.UserName, registrationRequest.Password);

            var newResponse = new CreatTruckiUserResponseDto { Id = currentUser.Id, AccessToken = tokenResponse.AccessToken };

            newResponse.Role = await _userManager.GetRolesAsync(currentUser);
            if (currentUser.Religion == null)
            {
                currentUser.Religion = string.Empty;
            }



            return ApiResponseModel<CreatTruckiUserResponseDto>.Success("User created successfully", newResponse, StatusCodes.Status201Created);


        }



        return Response<CreatTruckiUserResponseDto>.Fail("user not created", 500);
    }
}