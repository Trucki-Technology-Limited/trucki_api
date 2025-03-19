
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
    private readonly IEmailService _emailService;

    public AuthService(UserManager<User> userManager, TruckiDBContext context, IMapper mapper, ITokenService tokenService, IEmailService emailService

    )
    {
        _userManager = userManager;
        _emailService = emailService;
        _tokenService = tokenService;
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
                PhoneNumber = user.PhoneNumber,
                EmailAddress = user.Email,
                isPasswordChanged = user.HasChangedPassword,
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

    public async Task<ApiResponseModel<bool>> AddNewUserAsync(string name, string email, string phone, string role, string password, bool hasChangedPassword)
    {
        var user = new User
        {
            UserName = email,
            NormalizedUserName = email.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            EmailConfirmed = true,
            PhoneNumber = phone,
            PasswordHash =
                new PasswordHasher<User>().HashPassword(null,
                    password),
            SecurityStamp = string.Empty,
            HasChangedPassword = hasChangedPassword,
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
    public async Task<ApiResponseModel<UserResponseModel>> GetUserById(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ApiResponseModel<UserResponseModel>
            {
                IsSuccessful = false,
                Message = "User not found",
                StatusCode = 404
            };
        }
        var res = new UserResponseModel
        {
            name = user.firstName,
            phone = user.PhoneNumber,
            IsActive = user.IsActive,
            email = user.Email,
            IsPasswordChanged = false,

        };
        return new ApiResponseModel<UserResponseModel>
        {
            IsSuccessful = true,
            Data = res,
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<bool>> ChangePasswordAsync(string userId, string newPassword)
    {
        // Find the user by their ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "User not found",
                StatusCode = 404
            };
        }

        // Generate the new password hash
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

        // Update the user with the new password
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = result.Errors.FirstOrDefault()?.Description ?? "Failed to update password",
                StatusCode = 400
            };
        }
        // Update HasChangedPassword to true
        user.HasChangedPassword = false;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Failed to update user details",
                StatusCode = 400
            };
        }

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Password updated successfully",
            StatusCode = 200
        };
    }
    public async Task<ApiResponseModel<bool>> UpdateUserPassword(string userId, UpdateUsersPasswordRequestModel requestModel)
    {
        // Find the user by their ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "User not found",
                StatusCode = 404
            };
        }

        // Check if the old password is valid
        var isOldPasswordValid = await _userManager.CheckPasswordAsync(user, requestModel.OldPassword);
        if (!isOldPasswordValid)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Old password is incorrect",
                StatusCode = 400
            };
        }

        // Check if the new password and confirm password match
        if (requestModel.NewPassword != requestModel.ConfirmNewPassword)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "New password and confirm password do not match",
                StatusCode = 400
            };
        }

        // Change the password using UserManager's ChangePasswordAsync method
        var result = await _userManager.ChangePasswordAsync(user, requestModel.OldPassword, requestModel.NewPassword);
        if (!result.Succeeded)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = result.Errors.FirstOrDefault()?.Description ?? "Failed to update password",
                StatusCode = 400
            };
        }
        // Update HasChangedPassword to true
        user.HasChangedPassword = true;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Failed to update user details",
                StatusCode = 400
            };
        }

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Password updated successfully",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<bool>> ForgotPasswordAsync(string email)
    {
        // Find the user by email
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // For security reasons, we still return success even if the email doesn't exist
            return new ApiResponseModel<bool>
            {
                IsSuccessful = true,
                Message = "If your email exists in our system, you will receive a reset code shortly",
                StatusCode = 200
            };
        }

        // Generate a random 6-digit code
        var resetCode = new Random().Next(100000, 999999).ToString();

        // Store the reset code and expiration time in the user entity or a separate table
        // Here we're using UserTokens which is part of ASP.NET Identity
        await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "ResetCode", resetCode);
        await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "ResetCodeExpiry", DateTime.UtcNow.AddMinutes(15).ToString("o"));

        // Send the reset code to the user's email
        await _emailService.SendPasswordResetEmailAsync(
            email,
            "Reset Your Trucki Password",
            resetCode
        );


        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "If your email exists in our system, you will receive a reset code shortly",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<bool>> VerifyResetCodeAsync(string email, string resetCode)
    {
        // Find the user by email
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Invalid reset code or email",
                StatusCode = 400
            };
        }

        // Get the stored reset code and its expiry
        var storedResetCode = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "ResetCode");
        var expiryString = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "ResetCodeExpiry");

        if (string.IsNullOrEmpty(storedResetCode) || string.IsNullOrEmpty(expiryString))
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Reset code has expired or was not requested",
                StatusCode = 400
            };
        }

        // Check if the code has expired
        if (!DateTime.TryParse(expiryString, out var expiry) || expiry < DateTime.UtcNow)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Reset code has expired",
                StatusCode = 400
            };
        }

        // Verify the code
        if (storedResetCode != resetCode)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Invalid reset code",
                StatusCode = 400
            };
        }

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Reset code verified successfully",
            StatusCode = 200
        };
    }

    public async Task<ApiResponseModel<bool>> ResetPasswordAsync(string email, string resetCode, string newPassword)
    {
        // First verify the reset code
        var verifyResult = await VerifyResetCodeAsync(email, resetCode);
        if (!verifyResult.IsSuccessful)
        {
            return verifyResult;
        }

        // Find the user by email
        var user = await _userManager.FindByEmailAsync(email);

        // Generate the new password hash
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

        // Update the user with the new password
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = result.Errors.FirstOrDefault()?.Description ?? "Failed to update password",
                StatusCode = 400
            };
        }

        // Clear the reset code
        await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "ResetCode");
        await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "ResetCodeExpiry");

        // Update HasChangedPassword to true
        user.HasChangedPassword = true;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return new ApiResponseModel<bool>
            {
                IsSuccessful = false,
                Message = "Password reset was successful but failed to update user details",
                StatusCode = 200  // Still return 200 as the password was reset
            };
        }

        return new ApiResponseModel<bool>
        {
            IsSuccessful = true,
            Message = "Password reset successfully",
            StatusCode = 200
        };
    }

}