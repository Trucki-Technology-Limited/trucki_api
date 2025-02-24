using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services
{
    public class DriverBankAccountService : IDriverBankAccountService
    {
        private readonly TruckiDBContext _dbContext;
        private readonly IDriverService _driverService;

        public DriverBankAccountService(
            TruckiDBContext dbContext,
            IDriverService driverService)
        {
            _dbContext = dbContext;
            _driverService = driverService;
        }

        public async Task<ApiResponseModel<DriverBankAccountResponseDto>> AddBankAccountAsync(
            AddDriverBankAccountDto request)
        {
            try
            {
                // Validate driver exists and is active
                var driver = await _driverService.GetDriverById(request.DriversId);
                if (driver == null)
                {
                    return ApiResponseModel<DriverBankAccountResponseDto>.Fail(
                        "Driver not found",
                        404);
                }

                if (!driver.IsActive)
                {
                    return ApiResponseModel<DriverBankAccountResponseDto>.Fail(
                        "Driver account is not active",
                        400);
                }

                // Validate routing number format for US accounts
                if (!IsValidRoutingNumber(request.RoutingNumber))
                {
                    return ApiResponseModel<DriverBankAccountResponseDto>.Fail(
                        "Invalid routing number",
                        400);
                }

                // Check if this is the first account (will be set as default)
                bool isFirst = !await _dbContext.driverBankAccounts
                    .AnyAsync(ba => ba.DriverId == request.DriversId);

                // If this is not the first account, check maximum accounts limit
                if (!isFirst)
                {
                    var accountCount = await _dbContext.driverBankAccounts
                        .CountAsync(ba => ba.DriverId == request.DriversId);

                    if (accountCount >= 3) // Maximum 3 accounts per driver
                    {
                        return ApiResponseModel<DriverBankAccountResponseDto>.Fail(
                            "Maximum number of bank accounts reached",
                            400);
                    }
                }

                // Create new bank account
                var bankAccount = new DriverBankAccount
                {
                    DriverId = request.DriversId,
                    BankName = request.BankName,
                    AccountHolderName = request.AccountHolderName,
                    AccountNumber = request.AccountNumber,
                    RoutingNumber = request.RoutingNumber,
                    SwiftCode = request.SwiftCode,
                    IsDefault = isFirst, // First account is automatically default
                    IsVerified = false
                };

                _dbContext.driverBankAccounts.Add(bankAccount);
                await _dbContext.SaveChangesAsync();

                // Map to response DTO
                var response = new DriverBankAccountResponseDto
                {
                    Id = bankAccount.Id,
                    BankName = bankAccount.BankName,
                    AccountHolderName = bankAccount.AccountHolderName,
                    MaskedAccountNumber = MaskAccountNumber(bankAccount.AccountNumber),
                    RoutingNumber = bankAccount.RoutingNumber,
                    IsDefault = bankAccount.IsDefault,
                    IsVerified = bankAccount.IsVerified,
                    CreatedAt = bankAccount.CreatedAt
                };

                return ApiResponseModel<DriverBankAccountResponseDto>.Success(
                    "Bank account added successfully",
                    response,
                    201);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<DriverBankAccountResponseDto>.Fail(
                    $"Error adding bank account: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<List<DriverBankAccountResponseDto>>> GetDriverBankAccountsAsync(string driverId)
        {
            try
            {
                var accounts = await _dbContext.driverBankAccounts
                    .Where(ba => ba.DriverId == driverId)
                    .OrderByDescending(ba => ba.IsDefault)
                    .ThenByDescending(ba => ba.CreatedAt)
                    .Select(ba => new DriverBankAccountResponseDto
                    {
                        Id = ba.Id,
                        BankName = ba.BankName,
                        AccountHolderName = ba.AccountHolderName,
                        MaskedAccountNumber = MaskAccountNumber(ba.AccountNumber),
                        RoutingNumber = ba.RoutingNumber,
                        IsDefault = ba.IsDefault,
                        IsVerified = ba.IsVerified,
                        CreatedAt = ba.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponseModel<List<DriverBankAccountResponseDto>>.Success(
                    "Bank accounts retrieved successfully",
                    accounts,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<List<DriverBankAccountResponseDto>>.Fail(
                    $"Error retrieving bank accounts: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<bool>> SetDefaultBankAccountAsync(string driverId, string accountId)
        {
            try
            {
                // Start transaction
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    // Get all driver's bank accounts
                    var driverAccounts = await _dbContext.driverBankAccounts
                        .Where(ba => ba.DriverId == driverId)
                        .ToListAsync();

                    // Clear default flag from all accounts
                    foreach (var account in driverAccounts)
                    {
                        account.IsDefault = false;
                    }

                    // Find and set the new default account
                    var newDefaultAccount = driverAccounts.FirstOrDefault(ba => ba.Id == accountId);
                    if (newDefaultAccount == null)
                    {
                        return ApiResponseModel<bool>.Fail("Bank account not found", 404);
                    }

                    newDefaultAccount.IsDefault = true;
                    await _dbContext.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();

                    return ApiResponseModel<bool>.Success(
                        "Default bank account updated successfully",
                        true,
                        200);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error setting default bank account: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<bool>> DeleteBankAccountAsync(string driverId, string accountId)
        {
            try
            {
                var account = await _dbContext.driverBankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == accountId && ba.DriverId == driverId);

                if (account == null)
                {
                    return ApiResponseModel<bool>.Fail("Bank account not found", 404);
                }

                // Don't allow deletion of default account if it's not the only one
                if (account.IsDefault)
                {
                    var hasOtherAccounts = await _dbContext.driverBankAccounts
                        .AnyAsync(ba => ba.DriverId == driverId && ba.Id != accountId);

                    if (hasOtherAccounts)
                    {
                        return ApiResponseModel<bool>.Fail(
                            "Cannot delete default bank account. Please set another account as default first",
                            400);
                    }
                }

                _dbContext.driverBankAccounts.Remove(account);
                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success(
                    "Bank account deleted successfully",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error deleting bank account: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<bool>> VerifyBankAccountAsync(string accountId, string adminId)
        {
            try
            {
                var account = await _dbContext.driverBankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == accountId);

                if (account == null)
                {
                    return ApiResponseModel<bool>.Fail("Bank account not found", 404);
                }

                account.IsVerified = true;
                account.VerifiedAt = DateTime.UtcNow;
                account.VerifiedBy = adminId;

                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success(
                    "Bank account verified successfully",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error verifying bank account: {ex.Message}",
                    500);
            }
        }

        private bool IsValidRoutingNumber(string routingNumber)
        {
            // US routing numbers are 9 digits
            if (!Regex.IsMatch(routingNumber, @"^\d{9}$"))
            {
                return false;
            }

            // Implement checksum validation for US routing numbers
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                int digit = routingNumber[i] - '0';
                sum += digit * (i % 3 == 0 ? 3 : (i % 3 == 1 ? 7 : 1));
            }

            return sum % 10 == 0;
        }

        // Make the method static
    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber))
            return string.Empty;

        var length = accountNumber.Length;
        if (length <= 4)
            return accountNumber;

        return new string('*', length - 4) + accountNumber[^4..];
    }
    }
}