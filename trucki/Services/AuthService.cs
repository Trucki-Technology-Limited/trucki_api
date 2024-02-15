
using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using trucki.DBContext;
using trucki.Interfaces.IServices;
using trucki.Models;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly TruckiDBContext _context;
    private readonly IMapper _mapper;

    public AuthService(UserManager<User> userManager, TruckiDBContext context, IMapper mapper, ITokenService tokenService
    
    )
    {
        _userManager = userManager;
        _mapper = mapper;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tokenService = tokenService;
    }
        public async Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request)
        {
            var validityResult = await ValidateUser(request);
            if (!validityResult.IsSuccessful)
            {
                return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, Message = validityResult.Message, StatusCode = 400 };
            }
            var user = await _userManager.FindByNameAsync(request.email);

            var tokenResponse = await _tokenService.GetToken(user.UserName, request.password);
            if (tokenResponse.IsError)
            {
                return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, StatusCode = 500, Message = "Unknown error getting access token" };
            }
            
            var responseDto = new LoginResponseModel
            {
                Id = user.Id,
                RefreshToken = tokenResponse.RefreshToken,
                Token = tokenResponse.AccessToken,
                UserName = $"{user.firstName} {user.lastName}",
            };

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
}