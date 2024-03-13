
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
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

        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        if (!(Regex.IsMatch(request.email, emailPattern)))
            return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, Message = "Invalid email address format", StatusCode = 400 };

        var validityResult = await ValidateUser(request);
        if (!validityResult.IsSuccessful)
        {
            return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, Message = validityResult.Message, StatusCode = 400 };
        }

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
            isPasswordChanged = user.IsPasswordChanged,
            isEmailConfirmed = user.EmailConfirmed,
            isPhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LastLoginDate = DateTime.Now
        };

        await _userManager.UpdateAsync(user);


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

    /*public async Task<ApiResponseModel<CreatTruckiUserResponseDto>> RegisterTruckiAsync(CreatTruckiUserDto registrationRequest)
    {

        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        
        if (!(Regex.IsMatch(registrationRequest.Email, emailPattern)))
            return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Invalid email address format", StatusCodes.Status400BadRequest);

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
            IsPasswordChanged = false,
            PhoneNumber = "",
            Role = _configuration.GetSection("UserRole")["Users"],
            UserName = registrationRequest.Email

        };

        if (ExistingUser != null)
        {
                return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"User account already exist", StatusCodes.Status400BadRequest);
  
        }
        if (ExistingUser == null)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {

                var result = await _userManager.CreateAsync(user, registrationRequest.Password);


                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, user.Role);


                    // Add the Permission Coming in as a claim for the Users..
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

            var newResponse = new CreatTruckiUserResponseDto { Id = currentUser.Id, Token = tokenResponse.AccessToken };

            newResponse.Role = await _userManager.GetRolesAsync(currentUser);

            return ApiResponseModel<CreatTruckiUserResponseDto>.Success("User created successfully", newResponse, StatusCodes.Status201Created);


        }



        return ApiResponseModel<CreatTruckiUserResponseDto>.Fail("user not created", 500);
    }*/

    public async Task<ApiResponseModel<CreatTruckiUserResponseDto>> RegisterTruckiAsync(CreatTruckiUserDto registrationRequest)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        if (!Regex.IsMatch(registrationRequest.Email, emailPattern))
            return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Invalid email address format", StatusCodes.Status400BadRequest);

        var existingUser = await _userManager.FindByEmailAsync(registrationRequest.Email);

        if (existingUser != null)
        {
            return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"User account already exists", StatusCodes.Status400BadRequest);
        }
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
            IsPasswordChanged = false,
            PhoneNumber = "",
            Role = _configuration.GetSection("UserRole")["Users"],
            UserName = registrationRequest.Email

        };

        var userRoles = user.Role.Split(',').ToList();

        if (userRoles.Any(role => !_configuration.GetSection("UserRole").GetChildren().Any(x => x.Value.Split(',').Contains(role))))
        {
            return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Invalid user role(s) provided", StatusCodes.Status400BadRequest);
        }

        var userPermissions = registrationRequest.Permissions.Select(permission => new Claim("Permission", permission)).ToList();

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            foreach (var role in userRoles)
            {
                if (role.ToLower().Contains(registrationRequest.Role.ToLower()))
                {
                    user = new User
                    {
                        Email = registrationRequest.Email,
                        firstName = registrationRequest.Name,
                        lastName = "",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        EmailConfirmed = true,
                        Id = Guid.NewGuid().ToString(),
                        IsActive = true,
                        IsPasswordChanged = false,
                        PhoneNumber = "",
                        Role = role,
                        UserName = registrationRequest.Email
                    };

                    var result = await _userManager.CreateAsync(user, registrationRequest.Password);


                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, user.Role);
                        await _userManager.AddClaimsAsync(user, userPermissions);

                        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var emailconf = await _userManager.ConfirmEmailAsync(user, emailToken);
                    }
                    else
                    {
                        // Rollback transaction if user creation failed
                        transaction.Dispose();
                        return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Failed to create user with role {role}", StatusCodes.Status500InternalServerError);
                    }
                }

            }

            transaction.Complete();
        }

        var currentUser = await _userManager.FindByEmailAsync(registrationRequest.Email);
        //var tokenResponse = await _tokenService.GetToken(currentUser.UserName, registrationRequest.Password);

        var newResponse = new CreatTruckiUserResponseDto
        {
            Id = currentUser.Id,
            EmailAddress = currentUser.Email,
            Permissions = registrationRequest.Permissions
        };

        return ApiResponseModel<CreatTruckiUserResponseDto>.Success("User(s) created successfully", newResponse, StatusCodes.Status201Created);
    }

    public async Task<ApiResponseModel<RefreshTokenResponseDto>> RefreshToken(string refreshToken)
    {
        try
        {
            var token = await _tokenService.RefreshTokenAsync(refreshToken);

            var result = new ApiResponseModel<RefreshTokenResponseDto>
            {
                Data = new RefreshTokenResponseDto
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken
                }
            };
            return ApiResponseModel<RefreshTokenResponseDto>.Success("Token refreshed successfully", result.Data, StatusCodes.Status201Created);
        }
        catch (Exception)
        {
            return ApiResponseModel<RefreshTokenResponseDto>.Fail("Token was not refreshed", StatusCodes.Status404NotFound);
        }
    }

    public async Task<ApiResponseModel<string>> VerifyUser(string email)
    {

        try
        {
            var user = await _userManager.FindByEmailAsync(email);
           

            if (user == null)
            {
                //_logger.LogInformation($"Customer with email {email} not found");
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    StatusCode = StatusCodes.Status404NotFound ,
                    Message = $"Customer with email {email} not found"
                };

            }
/*
            if (user.EmailConfirmed == true)
            {
               // _logger.LogInformation($"Customer with email {email} has been verified already");
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Customer's email has been verified already"
                };

            }*/

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            //_logger.LogInformation($"Customer with email {email} verified successfully");

            var callBackUrlToLogin = $"{_configuration.GetSection("ExternalAPIs")["VerifiedLoginUrl"]}";

            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Customer's email has been verified already",
                Data = callBackUrlToLogin
            };
        }
        catch (Exception ex)
        {
           // _logger.LogError($"error occured while verifying the Customer {email}. Errpr message:{ex.Message}");
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"error occured while verifying the Customer {email}",
                Data = null
            };
        }
    }

    /* public async Task<GenericResponse<string>> ForgotPassword(string email)
     {
         try
         {
             var userId = _httpContext.HttpContext?.GetSessionUser().UserId ?? "";
             if (customer == null)
             {
                 _logger.LogInformation($"Customer with email {email} not found");
                 return new GenericResponse<string>
                 {
                     IsSuccessful = false,
                     ResponseCode = "01",
                     ResponseMessage = "Customer not found"
                 };

             }

             if (!Util.IsValidEmailAddress(email))
                 return new GenericResponse<string>
                 {
                     ResponseCode = "02",
                     IsSuccessful = false,
                     ResponseMessage = "Enter a valid email address"
                 };

             var hashedPassword = Util.ComputeSha256Hash(customer.Password);

             var sendOTPRequest = new SendOtp2Dto
             {
                 Email = email,
                 Password = hashedPassword,
                 PhoneNumber = customer.PhoneNumber,

             };


             using var scope = _serviceProvider.CreateScope();
             var _serviceManager = scope.ServiceProvider.GetRequiredService<IOTPService>();
             var otpResponse = await _serviceManager.SendOTPToUser(sendOTPRequest);

             if (otpResponse.ResponseCode == "00")
             {

                 _logger.LogInformation($"User Id [{customer.CustomerId}]. OTP was sent successfully, Kindly validate to Complete the Password Change");

                 return new GenericResponse<string>
                 {
                     ResponseCode = "00",
                     ResponseMessage = "OTP was sent successfully, Kindly validate to Complete the Password Change",
                     IsSuccessful = true,
                     Data = sendOTPRequest.PhoneNumber
                 };


             }

             return new GenericResponse<string>
             {
                 ResponseCode = "03",
                 ResponseMessage = "OTP was not sent",
                 IsSuccessful = false,
                 Data = null
             };
             var callBackUrlPasswordReset = $"{_configuration.GetSection("ExternalAPIs")["resetPasswordUrl"]}";

             if (callBackUrlPasswordReset != null)
             {

                 var mailRequest = new MailRequestDto
                 {
                     FirstName = customer.FirstName,
                     RecipientEmail = customer.Email,
                     Subject = "Customer Forgot Password Reset Link",
                     Message = "Kindly click  on the link below to reset your password," + " " + "Reset date:" + " " + DateTime.Now.ToLongDateString() + "<br/>" +
                               $"{callBackUrlPasswordReset}" + "<br/>" +
                              "If you did not initiate this, please change your password and contact our Customer Centre on 01-7000555 or send an email to help@saf.ng" + "<br/>" +
                              "Why send this mail? We take security very seriously and we want to keep you in the loop of activities on your account.",

                 };

                 var resetPassword = await _notification.SendVerifyMail(mailRequest);
                 if (resetPassword.ResponseCode == "00")
                     _logger.LogInformation($"mail sent to reset password with customer id {customer.CustomerId} successfully");
                 else _logger.LogInformation($"unable to send mail to new to reset customer's  password due to {resetPassword.ResponseMessage}");
             }


             _logger.LogInformation($"Customer Password Reset was successful {customer.CustomerId}");
             return new GenericResponse<string>
             {
                 IsSuccessful = true,
                 ResponseCode = "00",
                 ResponseMessage = "Customer  forgot password reset was successful, an email have been sent to change the password",
                 Data = null

             };



         }
         catch (Exception ex)
         {
             _logger.LogError($"something went wrong while resetting customer's password {ex.Message}");
             return new GenericResponse<string>
             {
                 IsSuccessful = false,
                 ResponseCode = "99",
                 ResponseMessage = "something went wrong while  processing customer's email for forgot password"
             };
         }

     }*/

}