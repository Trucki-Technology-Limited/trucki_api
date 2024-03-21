
using System.Net;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using trucki.DatabaseContext;
using trucki.DTOs;
using trucki.Entities;
using trucki.Enums;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;
using trucki.Shared;
using trucki.Utility;

namespace trucki.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly TruckiDBContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly INotificationService _notification;
    public AuthService(UserManager<User> userManager, TruckiDBContext context, IMapper mapper,
        ITokenService tokenService, IConfiguration configuration, INotificationService notification, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _mapper = mapper;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tokenService = tokenService;
        _configuration = configuration;
        _notification = notification;
        _roleManager = roleManager;
    }
    public async Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request)
    {
        var loginResponse = new ApiResponseModel<LoginResponseModel>();

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


        var responseDto = new ApiResponseModel<LoginResponseModel>
        {
            Data = new LoginResponseModel
            {

                Id = user.Id,
                UserName = user.UserName,
                Token = "",
                Role = role,
                FirstName = user.firstName,
                LastName = user.lastName,
                RefreshToken = "",
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

        // Check if the role in the request is a valid enum value
        if (!Enum.TryParse<UserRoles>(registrationRequest.Role, out var userRole))
        {
            return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Invalid user role provided", StatusCodes.Status400BadRequest);
        }

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var user = new User
            {
                Email = registrationRequest.Email,
                firstName = registrationRequest.FirstName,
                lastName = registrationRequest.LastName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                EmailConfirmed = true,
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                PhoneNumber = registrationRequest.PhoneNumber,
                Role = userRole.ToString(), // Assign the parsed enum value as a string
                UserName = registrationRequest.Email
            };

            var result = await _userManager.CreateAsync(user, registrationRequest.Password);
            IdentityResult roleResult;

            if (result.Succeeded)
            {
                // Check if the role exists in the role manager, create it if not
                var roleExist = await _roleManager.RoleExistsAsync(userRole.ToString());
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(userRole.ToString()));
                }

                await _userManager.AddToRoleAsync(user, userRole.ToString());

                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var emailconf = await _userManager.ConfirmEmailAsync(user, emailToken);
            }
            else
            {
                // Rollback transaction if user creation failed
                transaction.Dispose();
                return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Failed to create user with role", StatusCodes.Status500InternalServerError);
            }

            transaction.Complete();
        }

        var currentUser = await _userManager.FindByEmailAsync(registrationRequest.Email);

        var newResponse = new CreatTruckiUserResponseDto
        {
            Id = currentUser.Id,
            EmailAddress = currentUser.Email
        };

        return ApiResponseModel<CreatTruckiUserResponseDto>.Success("User(s) created successfully", newResponse, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Generate Refresh Token for JWT
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ApiResponseModel<RefreshTokenResponseDto>> GenerateRefreshToken(RefreshTokenDto request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return ApiResponseModel<RefreshTokenResponseDto>.Fail("User is not logged in or does not exists", StatusCodes.Status404NotFound);
            }

            /* if (user. != request.RefreshToken)
             {
                 return ApiResponseModel<RefreshTokenResponseDto>.Fail("Refresh token not valid", StatusCodes.Status404NotFound);
             }*/

            var loginResponse = new ApiResponseModel<LoginResponseModel>
            {
                Data = new LoginResponseModel
                {

                    Id = user.Id,
                    UserName = user.UserName,
                    Token = "",
                    Role = { },
                    FirstName = user.firstName,
                    LastName = user.lastName,
                    RefreshToken = "",
                    EmailAddress = user.Email,
                    // isPasswordChanged = user.IsPasswordChanged,
                    isEmailConfirmed = user.EmailConfirmed,
                    isPhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    LastLoginDate = DateTime.Now
                }

            };

            var result = new ApiResponseModel<RefreshTokenResponseDto>
            {
                Data = new RefreshTokenResponseDto
                {
                    AccessToken = _tokenService.GenerateToken(ref loginResponse),
                    RefreshToken = _tokenService.GenerateRefreshToken(ref loginResponse),
                    EmailAddress = user.Email
                }
            };
            return ApiResponseModel<RefreshTokenResponseDto>.Success("Token refreshed successfully", result.Data, StatusCodes.Status201Created);
        }
        catch (Exception)
        {
            return ApiResponseModel<RefreshTokenResponseDto>.Fail("Token was not refreshed", StatusCodes.Status404NotFound);
        }
    }

    /// <summary>
    /// Refresh Token Using Identity Server
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
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
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Customer with email {email} not found"
                };

            }

            //user.EmailConfirmed = true;
            //await _userManager.UpdateAsync(user);
            //_logger.LogInformation($"Customer with email {email} verified successfully");

            var callBackUrlToLogin = $"{_configuration.GetSection("ExternalAPIs")["VerifiedLoginUrl"]}";

            return new ApiResponseModel<string>
            {
                IsSuccessful = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "User's email has been verified",
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

    public async Task<ApiResponseModel<string>> ForgotPassword(string email)
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
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Customer with email {email} not found"
                };

            }

            //user.EmailConfirmed = true;
            //await _userManager.UpdateAsync(user);
            //_logger.LogInformation($"Customer with email {email} verified successfully");




            var callBackUrlToChangePassword = $"{_configuration.GetSection("ExternalAPIs")["ChangePasswordUrl"]}";

            string encodePasswordChangeUrl = HttpUtility.UrlEncode(callBackUrlToChangePassword);

            if (callBackUrlToChangePassword != null)
            {

                var mailRequest = new MailRequest
                {
                    ToEmail = email,
                    FirstName = user.firstName,
                    TemplateName = GetEmailTemplate("template.html"),
                    Subject = "User Password Changed",
                    Message = "Kindly click on the link below to Change Password for your account, registered on " + DateTime.Now.ToLongDateString() + ":<br/>" +
                              $"<a href=\"{encodePasswordChangeUrl}\">{encodePasswordChangeUrl}</a><br/><br/>" +
                             "Best Regards,<br/>" +
                             "Trucki."

                };


                var changeUserPasswordEmail = await _notification.SendEmailAsync(mailRequest);
                if (changeUserPasswordEmail == true)
                {
                    return new ApiResponseModel<string>
                    {
                        IsSuccessful = true,
                        StatusCode = StatusCodes.Status200OK,
                        Message = "user's email has been sent for a Password Changed",
                        Data = encodePasswordChangeUrl.ToString()
                    };
                }
                else
                {
                    Console.WriteLine($"mail sent to newly created user with user id {user.Id} successfully & {JsonConvert.SerializeObject(changeUserPasswordEmail)}");
                }
                //_logger.LogInformation($"mail sent to newly created vendor with vendor id {vendorEntity.VendorId} successfully");
                //else _logger.LogInformation($"unable to send mail to new to newly created vendor due to {verifyCustomerAccount.ResponseMessage}");
            }

            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "user's email was not sent for a Password Changed",
                Data = null
            };
        }
        catch (Exception ex)
        {
            // _logger.LogError($"error occured while verifying the Customer {email}. Errpr message:{ex.Message}");
            return new ApiResponseModel<string>
            {
                IsSuccessful = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"error occured while Changing Password for the user {email}",
                Data = null
            };
        }

    }


    public async Task<ApiResponseModel<string>> ChangePassword(ChangePasswordDto request)
    {
        try
        {
            //_logger.LogInformation($"Inside ChangePassword method");

            var userDetails = await _userManager.FindByIdAsync(request.userId);

            var IsPasswordRegularExpression = Util.ValidUserRegistrationPassword(request.NewPassword);
            if (IsPasswordRegularExpression != "passed")
            {
                //_logger.LogInformation($"Password must contain UpperCase, characters and number");
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = IsPasswordRegularExpression
                };
            }

            if (userDetails == null)
            {
                //_logger.LogInformation($"User email doesn't exist {request.Email}. Vendor does not exist");

                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Please check your user details and try again"
                };
            }
            var result = await _userManager.ChangePasswordAsync(userDetails, request.OldPassword, request.NewPassword);
            if (result.Succeeded)
            {
                //_logger.LogInformation($"User email [{request.Email}]. Password Succesfully Changed");
                return new ApiResponseModel<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Password changed successfuly",
                    IsSuccessful = true
                };
            }
            return new ApiResponseModel<string>
            {
                Message = "Ensure that your old password is correct",
                IsSuccessful = false,
                StatusCode = StatusCodes.Status404NotFound
            };

        }

        catch (Exception ex)
        {
            //_logger.LogInformation($"Error occurred for User   [{request.Email}] while changing password, Error Message:{ex.Message}");
            return new ApiResponseModel<string>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Error occured while trying to change user password"
            };
        }

    }


    public async Task<ApiResponseModel<string>> ResetPassword(ResetPasswordDto request)
    {
        try
        {
            //_logger.LogInformation($"Inside ResetPassword method");

            var userDetails = await _userManager.FindByIdAsync(request.UserId);

            var IsPasswordRegularExpression = Util.ValidUserRegistrationPassword(request.NewPassword);
            if (IsPasswordRegularExpression != "passed")
            {
                //_logger.LogInformation($"Password must contain UpperCase, characters and number");
                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = IsPasswordRegularExpression
                };
            }

            if (userDetails == null)
            {
                //_logger.LogInformation($"User email doesn't exist {request.Email}. Vendor does not exist");

                return new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Please check your user details and try again"
                };
            }

            var tokenResponse = await GenerateResetTokenAsync(request.Email);
            request.Token = tokenResponse;
            var purpose = UserManager<User>.ResetPasswordTokenPurpose;
            var tokenProvider = _userManager.Options.Tokens.PasswordResetTokenProvider;

            var isValidToken = await _userManager.VerifyUserTokenAsync(userDetails, tokenProvider, purpose, tokenResponse);
            if (isValidToken)
            {
                _mapper.Map<User>(request);
                var result = await _userManager.ResetPasswordAsync(userDetails, tokenResponse, request.NewPassword);
                return new ApiResponseModel<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Password Reset was successfuly",
                    IsSuccessful = true
                };
            }

            return new ApiResponseModel<string>
            {
                Message = "password reset failed",
                IsSuccessful = false,
                StatusCode = StatusCodes.Status404NotFound
            };

        }

        catch (Exception ex)
        {
            //_logger.LogInformation($"Error occurred for User   [{request.Email}] while changing password, Error Message:{ex.Message}");
            return new ApiResponseModel<string>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Error occured while trying to reset password"
            };
        }

    }

    private async Task<string> GenerateResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Handle user not found
            return "User is not found";
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return token;
    }

    private string GetEmailTemplate(string templateName)
    {
        var baseDir = Directory.GetCurrentDirectory();
        string folderName = "/Email/";
        var path = Path.Combine(baseDir + folderName, templateName);
        return File.ReadAllText(path);
    }

}



/* public async Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request)
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
        *//* var tokenResponse = await _tokenService.GenerateToken(user.UserName, request.password);
         if (tokenResponse.IsError)
         {
             return new ApiResponseModel<LoginResponseModel> { IsSuccessful = false, StatusCode = 500, Message = "Unknown error getting access token" };
         }*//*

        //var roles = _roleManager.Roles.ToList(); 

        // var user = await _userManager.FindByEmailAsync(loginRequest.Email);

        // var tokenResponse = await _tokenService.GetToken(user.UserName, loginRequest.Password);
        var role = await _userManager.GetRolesAsync(user);

        //Get the user permissions if any is avaialable
        *//* List<Claim> getUserClaims = (List<Claim>)await _userManager.GetClaimsAsync(user);
         List<string> adminPermission = new();
         foreach (var userClaim in getUserClaims)
         {
             adminPermission.Add(userClaim.Value);
         }*/
/* var responseDto = new LoginResponseModel()
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

 await _userManager.UpdateAsync(user);*//*


return new ApiResponseModel<LoginResponseModel> { IsSuccessful = true, Message = "Success", StatusCode = 200, Data = responseDto };
}
*/

/* private async Task<List<Claim>> GetClaims(LoginRequestModel loginRequest)
 {
     var loggedInUser = await _userManager.FindByEmailAsync(loginRequest.email);
     var claims = new List<Claim>
      {
          new Claim(ClaimTypes.Name, loggedInUser)
      };
     var roles = await _userManager.GetRolesAsync(loggedInUser);
     foreach (var role in roles)
     {
         claims.Add(new Claim(ClaimTypes.Role, role));
     }
     return claims;
 }*/

/// <summary>
/// Registrations for User
/// </summary>
/// <param name="registrationRequest"></param>
/// <returns></returns>
/* public async Task<ApiResponseModel<CreatTruckiUserResponseDto>> RegisterTruckiAsync(CreatTruckiUserDto registrationRequest)
 {
     string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

     if (!Regex.IsMatch(registrationRequest.Email, emailPattern))
         return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Invalid email address format", StatusCodes.Status400BadRequest);

     var existingUser = await _userManager.FindByEmailAsync(registrationRequest.Email);

     if (existingUser != null)
     {
         return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"User account already exists", StatusCodes.Status400BadRequest);
     }
     *//* var user = new User
      {
          Email = registrationRequest.Email,
          firstName = registrationRequest.FirstName,
          lastName = registrationRequest.LastName,
          CreatedAt = DateTime.Now,
          UpdatedAt = DateTime.Now,
          EmailConfirmed = true,
          Id = Guid.NewGuid().ToString(),
          IsActive = true,
          IsPasswordChanged = false,
          PhoneNumber = registrationRequest.PhoneNumber,
          Role = _configuration.GetSection("UserRole")["Users"],
          UserName = registrationRequest.Email

      };*//*

    // string[] roleNames = { "User", "Transporter", "Cargo Owner", "Driver" };



     *//* var roles = _configuration.GetSection("UserRole")["Users"];
      var userRoles = roles.Split(',').ToList();

      if (userRoles.Any(role => !_configuration.GetSection("UserRole").GetChildren().Any(x => x.Value.Split(',').Contains(role))))
      {
          return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Invalid user role(s) provided", StatusCodes.Status400BadRequest);
      }*//*

     // var userPermissions = registrationRequest.Permissions.Select(permission => new Claim("Permission", permission)).ToList();

     using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
     {


         var user = new User
         {
             Email = registrationRequest.Email,
             firstName = registrationRequest.FirstName,
             lastName = registrationRequest.LastName,
             CreatedAt = DateTime.Now,
             UpdatedAt = DateTime.Now,
             EmailConfirmed = true,
             Id = Guid.NewGuid().ToString(),
             IsActive = true,
             //IsPasswordChanged = false,
             PhoneNumber = registrationRequest.PhoneNumber,
             Role = registrationRequest.Role,
             UserName = registrationRequest.Email

         };

         var result = await _userManager.CreateAsync(user, registrationRequest.Password);
         IdentityResult roleResult;

         if (result.Succeeded)
         {
             if(registrationRequest.Role.Contains(UserRoles.))
                 var roleExist = await _roleManager.RoleExistsAsync(registrationRequest.Role);
                 if (!roleExist)
                 {
                     roleResult = await _roleManager.CreateAsync(new IdentityRole(registrationRequest.Role));
                 }

             await _userManager.AddToRoleAsync(user, registrationRequest.Role);
             // await _userManager.AddClaimsAsync(user, roleName);

             var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
             var emailconf = await _userManager.ConfirmEmailAsync(user, emailToken);
         }
         else
         {
             // Rollback transaction if user creation failed
             transaction.Dispose();
             return ApiResponseModel<CreatTruckiUserResponseDto>.Fail($"Failed to create user with role", StatusCodes.Status500InternalServerError);
         }

         transaction.Complete();
     }

     var currentUser = await _userManager.FindByEmailAsync(registrationRequest.Email);
     //var tokenResponse = await _tokenService.GetToken(currentUser.UserName, registrationRequest.Password);


     var newResponse = new CreatTruckiUserResponseDto
     {
         Id = currentUser.Id,
         EmailAddress = currentUser.Email
     };

     return ApiResponseModel<CreatTruckiUserResponseDto>.Success("User(s) created successfully", newResponse, StatusCodes.Status201Created);
 }*/