using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services
{
    public class WalletService : IWalletService
    {
        private readonly TruckiDBContext _dbContext;
        private readonly ILogger<WalletService> _logger;

        public WalletService(TruckiDBContext dbContext, ILogger<WalletService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<decimal> GetWalletBalance(string cargoOwnerId)
        {
            var wallet = await EnsureWalletExists(cargoOwnerId);
            return wallet.Balance;
        }

        public async Task<ApiResponseModel<bool>> AddFundsToWallet(
            string cargoOwnerId, 
            decimal amount, 
            string description, 
            WalletTransactionType type, 
            string relatedOrderId = null)
        {
            try
            {
                if (amount <= 0)
                {
                    return ApiResponseModel<bool>.Fail("Amount must be greater than zero", 400);
                }

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var wallet = await EnsureWalletExists(cargoOwnerId);

                        // Create transaction record
                        var walletTransaction = new WalletTransaction
                        {
                            WalletId = wallet.Id,
                            Amount = amount, // Positive for adding funds
                            Description = description,
                            RelatedOrderId = relatedOrderId,
                            TransactionType = type,
                        };

                        // Update wallet balance
                        wallet.Balance += amount;

                        // Save transaction
                        _dbContext.WalletTransactions.Add(walletTransaction);
                        _dbContext.CargoOwnerWallets.Update(wallet);
                        await _dbContext.SaveChangesAsync();

                        // Commit transaction
                        await transaction.CommitAsync();

                        return ApiResponseModel<bool>.Success(
                            $"Successfully added {amount:C} to wallet", 
                            true, 
                            200);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error adding funds to wallet");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding funds to wallet");
                return ApiResponseModel<bool>.Fail($"Error adding funds: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<bool>> DeductFundsFromWallet(
            string cargoOwnerId, 
            decimal amount, 
            string description, 
            WalletTransactionType type, 
            string relatedOrderId = null)
        {
            try
            {
                if (amount <= 0)
                {
                    return ApiResponseModel<bool>.Fail("Amount must be greater than zero", 400);
                }

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var wallet = await _dbContext.CargoOwnerWallets
                            .FirstOrDefaultAsync(w => w.CargoOwnerId == cargoOwnerId);

                        if (wallet == null)
                        {
                            return ApiResponseModel<bool>.Fail("Wallet not found", 404);
                        }

                        // Check if sufficient funds
                        if (wallet.Balance < amount)
                        {
                            return ApiResponseModel<bool>.Fail("Insufficient funds", 400);
                        }

                        // Create transaction record
                        var walletTransaction = new WalletTransaction
                        {
                            WalletId = wallet.Id,
                            Amount = -amount, // Negative for deducting funds
                            Description = description,
                            RelatedOrderId = relatedOrderId,
                            TransactionType = type,
                        };

                        // Update wallet balance
                        wallet.Balance -= amount;

                        // Save transaction
                        _dbContext.WalletTransactions.Add(walletTransaction);
                        _dbContext.CargoOwnerWallets.Update(wallet);
                        await _dbContext.SaveChangesAsync();

                        // Commit transaction
                        await transaction.CommitAsync();

                        return ApiResponseModel<bool>.Success(
                            $"Successfully deducted {amount:C} from wallet", 
                            true, 
                            200);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error deducting funds from wallet");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting funds from wallet");
                return ApiResponseModel<bool>.Fail($"Error deducting funds: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<PagedResponse<WalletTransactionResponseModel>>> GetWalletTransactions(
            string cargoOwnerId, 
            GetWalletTransactionsQueryDto query)
        {
            try
            {
                var wallet = await _dbContext.CargoOwnerWallets
                    .FirstOrDefaultAsync(w => w.CargoOwnerId == cargoOwnerId);

                if (wallet == null)
                {
                    return ApiResponseModel<PagedResponse<WalletTransactionResponseModel>>.Fail(
                        "Wallet not found", 
                        404);
                }

                // Build query for transactions
                var transactionsQuery = _dbContext.WalletTransactions
                    .Where(t => t.WalletId == wallet.Id);

                // Apply filters
                if (query.StartDate.HasValue)
                {
                    transactionsQuery = transactionsQuery.Where(t => t.CreatedAt >= query.StartDate);
                }

                if (query.EndDate.HasValue)
                {
                    transactionsQuery = transactionsQuery.Where(t => t.CreatedAt <= query.EndDate);
                }

                if (query.TransactionType.HasValue)
                {
                    transactionsQuery = transactionsQuery.Where(t => t.TransactionType == query.TransactionType);
                }

                // Apply sorting
                transactionsQuery = query.SortDescending
                    ? transactionsQuery.OrderByDescending(t => t.CreatedAt)
                    : transactionsQuery.OrderBy(t => t.CreatedAt);

                // Get total count
                var totalCount = await transactionsQuery.CountAsync();

                // Apply pagination
                var transactions = await transactionsQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                // Map to response models
                var transactionResponses = transactions.Select(t => new WalletTransactionResponseModel
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Description = t.Description,
                    RelatedOrderId = t.RelatedOrderId,
                    TransactionType = t.TransactionType,
                    CreatedAt = t.CreatedAt
                }).ToList();

                // Create paged response
                var pagedResponse = new PagedResponse<WalletTransactionResponseModel>
                {
                    Data = transactionResponses,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return ApiResponseModel<PagedResponse<WalletTransactionResponseModel>>.Success(
                    "Transactions retrieved successfully",
                    pagedResponse,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wallet transactions");
                return ApiResponseModel<PagedResponse<WalletTransactionResponseModel>>.Fail(
                    $"Error retrieving transactions: {ex.Message}", 
                    500);
            }
        }

        public async Task<CargoOwnerWallet> EnsureWalletExists(string cargoOwnerId)
        {
            var wallet = await _dbContext.CargoOwnerWallets
                .FirstOrDefaultAsync(w => w.CargoOwnerId == cargoOwnerId);

            if (wallet == null)
            {
                // Create new wallet
                wallet = new CargoOwnerWallet
                {
                    CargoOwnerId = cargoOwnerId,
                    Balance = 0
                };

                _dbContext.CargoOwnerWallets.Add(wallet);
                await _dbContext.SaveChangesAsync();
            }

            return wallet;
        }
    }
}