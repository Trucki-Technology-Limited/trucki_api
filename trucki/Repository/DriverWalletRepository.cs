using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository
{
    public class DriverWalletRepository : IDriverWalletRepository
    {
        private readonly TruckiDBContext _dbContext;
        private readonly ILogger<DriverWalletRepository> _logger;

        public DriverWalletRepository(TruckiDBContext dbContext, ILogger<DriverWalletRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<DriverWallet> EnsureWalletExistsAsync(string driverId)
        {
            var wallet = await _dbContext.Set<DriverWallet>()
                .FirstOrDefaultAsync(w => w.DriverId == driverId);

            if (wallet == null)
            {
                // Create new wallet
                wallet = new DriverWallet
                {
                    DriverId = driverId,
                    Balance = 0,
                    PendingWithdrawal = 0,
                    NextWithdrawal = 0
                };

                _dbContext.Set<DriverWallet>().Add(wallet);
                await _dbContext.SaveChangesAsync();
            }

            return wallet;
        }

        public async Task<ApiResponseModel<DriverWalletBalanceResponseModel>> GetWalletBalanceAsync(string driverId)
        {
            try
            {
                // Get the driver and wallet
                var driver = await _dbContext.Drivers
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<DriverWalletBalanceResponseModel>.Fail(
                        "Driver not found",
                        404);
                }

                // Get or create wallet
                var wallet = await EnsureWalletExistsAsync(driverId);

                // Get recent transactions
                var recentTransactions = await _dbContext.Set<DriverWalletTransaction>()
                    .Where(t => t.Wallet.DriverId == driverId)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .Select(t => new DriverWalletTransactionResponseModel
                    {
                        Id = t.Id,
                        Amount = t.Amount,
                        Description = t.Description,
                        RelatedOrderId = t.RelatedOrderId,
                        TransactionType = t.TransactionType,
                        CreatedAt = t.CreatedAt,
                        IsProcessed = t.IsProcessed,
                        ProcessedAt = t.ProcessedAt,
                        BankTransferReference = t.BankTransferReference
                    })
                    .ToListAsync();

                // Calculate totals
                var completedOrders = await _dbContext.Set<DriverWalletTransaction>()
                    .CountAsync(t => t.Wallet.DriverId == driverId && t.TransactionType == DriverTransactionType.Delivery);

                var totalEarnings = await _dbContext.Set<DriverWalletTransaction>()
                    .Where(t => t.Wallet.DriverId == driverId && t.TransactionType == DriverTransactionType.Delivery)
                    .SumAsync(t => t.Amount);

                // Calculate next withdrawal date (upcoming Friday)
                var today = DateTime.UtcNow.Date;
                var daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
                if (daysUntilFriday == 0) // If today is Friday, next withdrawal is next Friday
                    daysUntilFriday = 7;

                var nextWithdrawalDate = today.AddDays(daysUntilFriday);

                // Create the response model
                var response = new DriverWalletBalanceResponseModel
                {
                    DriverId = driverId,
                    DriverName = driver.Name,
                    TotalBalance = wallet.Balance,
                    AvailableBalance = wallet.Balance - wallet.PendingWithdrawal,
                    PendingWithdrawal = wallet.PendingWithdrawal,
                    NextWithdrawal = wallet.NextWithdrawal,
                    NextWithdrawalDate = nextWithdrawalDate,
                    CompletedOrders = completedOrders,
                    TotalEarnings = totalEarnings,
                    RecentTransactions = recentTransactions
                };

                // Add order details to transactions if needed
                await EnrichTransactionsWithOrderDetails(recentTransactions);

                return ApiResponseModel<DriverWalletBalanceResponseModel>.Success(
                    "Wallet balance retrieved successfully",
                    response,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver wallet balance");
                return ApiResponseModel<DriverWalletBalanceResponseModel>.Fail(
                    $"Error retrieving wallet balance: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<bool>> CreditDeliveryAmountAsync(
            string driverId,
            string orderId,
            decimal amount,
            string description)
        {
            try
            {
                if (amount <= 0)
                {
                    return ApiResponseModel<bool>.Fail("Amount must be greater than zero", 400);
                }

                // Check if order exists and is valid for crediting
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponseModel<bool>.Fail("Order not found", 404);
                }

                if (order.Status != CargoOrderStatus.Delivered)
                {
                    return ApiResponseModel<bool>.Fail("Order must be in Delivered status to credit payment", 400);
                }

                // Verify this is the correct driver for the order
                var driver = await _dbContext.Drivers
                    .Include(d => d.Truck)
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<bool>.Fail("Driver not found", 404);
                }

                if (driver.Truck == null || order.AcceptedBid == null || driver.Truck.Id != order.AcceptedBid.TruckId)
                {
                    return ApiResponseModel<bool>.Fail("This driver is not assigned to this order", 400);
                }

                // Check if this order has already been credited
                var existingTransaction = await _dbContext.Set<DriverWalletTransaction>()
                    .AnyAsync(t => t.RelatedOrderId == orderId && t.TransactionType == DriverTransactionType.Delivery);

                if (existingTransaction)
                {
                    return ApiResponseModel<bool>.Fail("This order has already been credited", 400);
                }

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Get or create the wallet
                        var wallet = await EnsureWalletExistsAsync(driverId);

                        // Create transaction record
                        var walletTransaction = new DriverWalletTransaction
                        {
                            WalletId = wallet.Id,
                            Amount = amount,
                            Description = description,
                            RelatedOrderId = orderId,
                            TransactionType = DriverTransactionType.Delivery
                        };

                        // Update wallet balance
                        wallet.Balance += amount;

                        // Determine which withdrawal bucket to add this to
                        // If today is before Tuesday, add to PendingWithdrawal, otherwise to NextWithdrawal
                        var today = DateTime.UtcNow.Date;
                        var dayOfWeek = today.DayOfWeek;

                        if (dayOfWeek <= DayOfWeek.Tuesday)
                        {
                            // Credit will be part of this week's Friday withdrawal
                            wallet.PendingWithdrawal += amount;
                        }
                        else
                        {
                            // Credit will be part of next week's Friday withdrawal
                            wallet.NextWithdrawal += amount;
                        }

                        // Save transaction
                        _dbContext.Set<DriverWalletTransaction>().Add(walletTransaction);
                        _dbContext.Set<DriverWallet>().Update(wallet);
                        await _dbContext.SaveChangesAsync();

                        // Mark order as completed
                        order.Status = CargoOrderStatus.Completed;
                        await _dbContext.SaveChangesAsync();

                        // Commit transaction
                        await transaction.CommitAsync();

                        return ApiResponseModel<bool>.Success(
                            $"Successfully credited {amount:C} for delivery completion",
                            true,
                            200);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error crediting driver wallet");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crediting driver wallet");
                return ApiResponseModel<bool>.Fail($"Error crediting wallet: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<DriverWithdrawalResultModel>> ProcessWeeklyWithdrawalsAsync(string adminId)
        {
            try
            {
                // Verify if today is Friday - the designated withdrawal day
                var today = DateTime.UtcNow.Date;
                if (today.DayOfWeek != DayOfWeek.Friday)
                {
                    return ApiResponseModel<DriverWithdrawalResultModel>.Fail(
                        "Withdrawals can only be processed on Fridays",
                        400);
                }

                // Create a withdrawal batch
                var batchReference = $"DRV-WD-{today:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";

                var withdrawalSchedule = new DriverWithdrawalSchedule
                {
                    ScheduledDate = today,
                    ProcessedBy = adminId,
                    ProcessedAt = DateTime.UtcNow,
                    Notes = $"Regular weekly withdrawal - {today:yyyy-MM-dd}"
                };

                _dbContext.Set<DriverWithdrawalSchedule>().Add(withdrawalSchedule);
                await _dbContext.SaveChangesAsync();

                // Get all driver wallets with pending withdrawals
                var driversWithPendingWithdrawals = await _dbContext.Set<DriverWallet>()
                    .Include(w => w.Driver)
                    .ThenInclude(d => d.BankAccounts.Where(ba => ba.IsDefault))
                    .Where(w => w.PendingWithdrawal > 0)
                    .ToListAsync();

                var result = new DriverWithdrawalResultModel
                {
                    ProcessedCount = 0,
                    TotalAmount = 0,
                    BatchReference = batchReference,
                    ProcessedDate = today
                };

                foreach (var wallet in driversWithPendingWithdrawals)
                {
                    try
                    {
                        // Get default bank account
                        var defaultBankAccount = wallet.Driver.BankAccounts
                            .FirstOrDefault(ba => ba.IsDefault);

                        if (defaultBankAccount == null)
                        {
                            result.Errors.Add(new DriverWithdrawalError
                            {
                                DriverId = wallet.DriverId,
                                DriverName = wallet.Driver.Name,
                                ErrorMessage = "No default bank account found"
                            });
                            continue;
                        }

                        // Create withdrawal transaction
                        var withdrawalTransaction = new DriverWalletTransaction
                        {
                            WalletId = wallet.Id,
                            Amount = -wallet.PendingWithdrawal, // Negative amount for withdrawal
                            Description = $"Automatic weekly withdrawal to bank account ending in {defaultBankAccount.AccountNumber.Substring(Math.Max(0, defaultBankAccount.AccountNumber.Length - 4))}",
                            TransactionType = DriverTransactionType.Withdrawal,
                            BankTransferReference = $"{batchReference}-{wallet.DriverId.Substring(0, 8)}",
                            IsProcessed = true, // Mark as processed immediately
                            ProcessedAt = DateTime.UtcNow,
                            ProcessedBy = adminId
                        };

                        // Add to the withdrawal schedule
                        withdrawalSchedule.Transactions.Add(withdrawalTransaction);

                        // Update wallet
                        wallet.Balance -= wallet.PendingWithdrawal;
                        wallet.PendingWithdrawal = 0;

                        // Prepare for next week - move NextWithdrawal to PendingWithdrawal
                        wallet.PendingWithdrawal = wallet.NextWithdrawal;
                        wallet.NextWithdrawal = 0;

                        // Update counts
                        result.ProcessedCount++;
                        result.TotalAmount += (-withdrawalTransaction.Amount);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new DriverWithdrawalError
                        {
                            DriverId = wallet.DriverId,
                            DriverName = wallet.Driver.Name,
                            ErrorMessage = ex.Message
                        });
                    }
                }

                // Update the withdrawal schedule with totals
                withdrawalSchedule.TotalAmount = result.TotalAmount;
                withdrawalSchedule.TransactionsCount = result.ProcessedCount;
                withdrawalSchedule.IsProcessed = true;

                // Save all changes
                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<DriverWithdrawalResultModel>.Success(
                    $"Successfully processed {result.ProcessedCount} withdrawals for a total of {result.TotalAmount:C}",
                    result,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing weekly withdrawals");
                return ApiResponseModel<DriverWithdrawalResultModel>.Fail(
                    $"Error processing withdrawals: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<bool>> UpdateWithdrawalProjectionsAsync()
        {
            try
            {
                // This method would be run by a scheduled job to recalculate projections
                // for all drivers in case of any changes or corrections needed

                var today = DateTime.UtcNow.Date;
                var dayOfWeek = today.DayOfWeek;
                var isBeforeTuesday = dayOfWeek <= DayOfWeek.Tuesday;

                // Process each driver separately to ensure accuracy
                var driverWallets = await _dbContext.Set<DriverWallet>()
                    .Include(w => w.Transactions.Where(t => !t.IsProcessed && t.TransactionType == DriverTransactionType.Delivery))
                    .ToListAsync();

                foreach (var wallet in driverWallets)
                {
                    // Reset projections
                    wallet.PendingWithdrawal = 0;
                    wallet.NextWithdrawal = 0;

                    // Recalculate based on unprocessed delivery transactions
                    foreach (var transaction in wallet.Transactions)
                    {
                        if (transaction.TransactionType == DriverTransactionType.Delivery && !transaction.IsProcessed)
                        {
                            var transactionDate = transaction.CreatedAt.Date;
                            var transactionDayOfWeek = transactionDate.DayOfWeek;

                            // Determine if this transaction is for this week's withdrawal
                            bool isForCurrentWeek = IsTransactionForCurrentWeekWithdrawal(
                                transactionDate, transactionDayOfWeek, today, dayOfWeek);

                            if (isForCurrentWeek && isBeforeTuesday)
                            {
                                wallet.PendingWithdrawal += transaction.Amount;
                            }
                            else
                            {
                                wallet.NextWithdrawal += transaction.Amount;
                            }
                        }
                    }
                }

                // Save all changes
                await _dbContext.SaveChangesAsync();

                return ApiResponseModel<bool>.Success(
                    "Successfully updated withdrawal projections for all drivers",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating withdrawal projections");
                return ApiResponseModel<bool>.Fail(
                    $"Error updating projections: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>> GetTransactionHistoryAsync(
            string driverId,
            GetDriverWalletTransactionsQueryDto query)
        {
            try
            {
                // Verify driver exists
                var driver = await _dbContext.Drivers
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>.Fail(
                        "Driver not found",
                        404);
                }

                // Get wallet
                var wallet = await _dbContext.Set<DriverWallet>()
                    .FirstOrDefaultAsync(w => w.DriverId == driverId);

                if (wallet == null)
                {
                    // Driver has no wallet, return empty transactions
                    return ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>.Success(
                        "No transactions found",
                        new PagedResponse<DriverWalletTransactionResponseModel>
                        {
                            Data = new List<DriverWalletTransactionResponseModel>(),
                            PageNumber = query.PageNumber,
                            PageSize = query.PageSize,
                            TotalCount = 0,
                            TotalPages = 0
                        },
                        200);
                }

                // Build query for transactions
                var transactionsQuery = _dbContext.Set<DriverWalletTransaction>()
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

                if (query.OnlyWithdrawals)
                {
                    transactionsQuery = transactionsQuery.Where(t => t.TransactionType == DriverTransactionType.Withdrawal);
                }

                if (query.OnlyEarnings)
                {
                    transactionsQuery = transactionsQuery.Where(t => t.TransactionType == DriverTransactionType.Delivery);
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
                var transactionResponses = transactions.Select(t => new DriverWalletTransactionResponseModel
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Description = t.Description,
                    RelatedOrderId = t.RelatedOrderId,
                    TransactionType = t.TransactionType,
                    CreatedAt = t.CreatedAt,
                    IsProcessed = t.IsProcessed,
                    ProcessedAt = t.ProcessedAt,
                    BankTransferReference = t.BankTransferReference
                }).ToList();

                // Add order details to transactions if needed
                await EnrichTransactionsWithOrderDetails(transactionResponses);

                // Create paged response
                var pagedResponse = new PagedResponse<DriverWalletTransactionResponseModel>
                {
                    Data = transactionResponses,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>.Success(
                    "Transactions retrieved successfully",
                    pagedResponse,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver wallet transactions");
                return ApiResponseModel<PagedResponse<DriverWalletTransactionResponseModel>>.Fail(
                    $"Error retrieving transactions: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<DriverWithdrawalScheduleResponseModel>> GetWithdrawalScheduleAsync(string driverId)
        {
            try
            {
                // Verify driver exists
                var driver = await _dbContext.Drivers
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<DriverWithdrawalScheduleResponseModel>.Fail(
                        "Driver not found",
                        404);
                }

                // Get wallet
                var wallet = await _dbContext.Set<DriverWallet>()
                    .FirstOrDefaultAsync(w => w.DriverId == driverId);

                if (wallet == null)
                {
                    // Create a default response with zeros if no wallet exists
                    var todayDate = DateTime.UtcNow.Date;
                    var TuesdayOffset = ((int)DayOfWeek.Tuesday - (int)todayDate.DayOfWeek + 7) % 7;
                    var fridayOffset = ((int)DayOfWeek.Friday - (int)todayDate.DayOfWeek + 7) % 7;

                    return ApiResponseModel<DriverWithdrawalScheduleResponseModel>.Success(
                        "No wallet found for driver",
                        new DriverWithdrawalScheduleResponseModel
                        {
                            CurrentWeekCutoffDate = todayDate.AddDays(TuesdayOffset),
                            NextWithdrawalDate = todayDate.AddDays(fridayOffset),
                            AmountDueThisWeek = 0,
                            AmountDueNextWeek = 0,
                            UpcomingWithdrawals = new List<ScheduledWithdrawalItemResponseModel>(),
                            PastWithdrawals = new List<DriverWalletTransactionResponseModel>()
                        },
                        200);
                }

                // Calculate dates
                var today = DateTime.UtcNow.Date;
                var dayOfWeek = today.DayOfWeek;

                // Calculate the date of this week's Tuesday (cutoff)
                var daysUntilTuesday = ((int)DayOfWeek.Tuesday - (int)dayOfWeek + 7) % 7;
                if (daysUntilTuesday == 0 && dayOfWeek != DayOfWeek.Tuesday)
                    daysUntilTuesday = 7;
                var TuesdayCutoff = today.AddDays(daysUntilTuesday);

                // Calculate the date of this week's Friday (withdrawal)
                var daysUntilFriday = ((int)DayOfWeek.Friday - (int)dayOfWeek + 7) % 7;
                if (daysUntilFriday == 0 && dayOfWeek != DayOfWeek.Friday)
                    daysUntilFriday = 7;
                var fridayWithdrawal = today.AddDays(daysUntilFriday);

                // Get past withdrawals
                var pastWithdrawals = await _dbContext.Set<DriverWalletTransaction>()
                    .Where(t => t.WalletId == wallet.Id &&
                           t.TransactionType == DriverTransactionType.Withdrawal)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10) // Limit to last 10 withdrawals
                    .Select(t => new DriverWalletTransactionResponseModel
                    {
                        Id = t.Id,
                        Amount = t.Amount,
                        Description = t.Description,
                        TransactionType = t.TransactionType,
                        CreatedAt = t.CreatedAt,
                        IsProcessed = t.IsProcessed,
                        ProcessedAt = t.ProcessedAt,
                        BankTransferReference = t.BankTransferReference
                    })
                    .ToListAsync();

                // Get upcoming scheduled withdrawals
                var upcomingWithdrawals = new List<ScheduledWithdrawalItemResponseModel>();

                // Add this week's withdrawal if there's a pending amount
                if (wallet.PendingWithdrawal > 0)
                {
                    // Get order IDs associated with this withdrawal
                    var thisWeekOrderIds = await _dbContext.Set<DriverWalletTransaction>()
                        .Where(t => t.WalletId == wallet.Id &&
                               t.TransactionType == DriverTransactionType.Delivery &&
                               !t.IsProcessed &&
                               t.CreatedAt.Date <= TuesdayCutoff)
                        .Select(t => t.RelatedOrderId)
                        .ToListAsync();

                    upcomingWithdrawals.Add(new ScheduledWithdrawalItemResponseModel
                    {
                        ScheduledDate = fridayWithdrawal,
                        Amount = wallet.PendingWithdrawal,
                        DeliveryCount = thisWeekOrderIds.Count,
                        OrderIds = thisWeekOrderIds.Where(id => id != null).ToList()
                    });
                }

                // Add next week's withdrawal if there's a next amount
                if (wallet.NextWithdrawal > 0)
                {
                    // Get order IDs associated with next withdrawal
                    var nextWeekOrderIds = await _dbContext.Set<DriverWalletTransaction>()
                        .Where(t => t.WalletId == wallet.Id &&
                               t.TransactionType == DriverTransactionType.Delivery &&
                               !t.IsProcessed &&
                               t.CreatedAt.Date > TuesdayCutoff)
                        .Select(t => t.RelatedOrderId)
                        .ToListAsync();

                    upcomingWithdrawals.Add(new ScheduledWithdrawalItemResponseModel
                    {
                        ScheduledDate = fridayWithdrawal.AddDays(7), // Next Friday
                        Amount = wallet.NextWithdrawal,
                        DeliveryCount = nextWeekOrderIds.Count,
                        OrderIds = nextWeekOrderIds.Where(id => id != null).ToList()
                    });
                }

                // Create response
                var response = new DriverWithdrawalScheduleResponseModel
                {
                    CurrentWeekCutoffDate = TuesdayCutoff,
                    NextWithdrawalDate = fridayWithdrawal,
                    AmountDueThisWeek = wallet.PendingWithdrawal,
                    AmountDueNextWeek = wallet.NextWithdrawal,
                    UpcomingWithdrawals = upcomingWithdrawals,
                    PastWithdrawals = pastWithdrawals
                };

                return ApiResponseModel<DriverWithdrawalScheduleResponseModel>.Success(
                    "Withdrawal schedule retrieved successfully",
                    response,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver withdrawal schedule");
                return ApiResponseModel<DriverWithdrawalScheduleResponseModel>.Fail(
                    $"Error retrieving withdrawal schedule: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<List<PendingDriverWithdrawalResponseModel>>> GetAllPendingWithdrawalsAsync()
        {
            try
            {
                // Get all wallets with pending withdrawals
                var walletsWithPendingWithdrawals = await _dbContext.Set<DriverWallet>()
                    .Include(w => w.Driver)
                    .ThenInclude(d => d.BankAccounts.Where(ba => ba.IsDefault))
                    .Where(w => w.PendingWithdrawal > 0)
                    .ToListAsync();

                var result = new List<PendingDriverWithdrawalResponseModel>();

                foreach (var wallet in walletsWithPendingWithdrawals)
                {
                    // Get default bank account
                    var defaultBankAccount = wallet.Driver.BankAccounts
                        .FirstOrDefault(ba => ba.IsDefault);

                    if (defaultBankAccount == null)
                    {
                        // Skip drivers with no bank account
                        continue;
                    }

                    // Count orders associated with this withdrawal
                    var today = DateTime.UtcNow.Date;
                    var dayOfWeek = today.DayOfWeek;

                    // Calculate the date of this week's Tuesday (cutoff)
                    var daysUntilTuesday = ((int)DayOfWeek.Tuesday - (int)dayOfWeek + 7) % 7;
                    if (daysUntilTuesday == 0 && dayOfWeek != DayOfWeek.Tuesday)
                        daysUntilTuesday = 7;
                    var TuesdayCutoff = today.AddDays(daysUntilTuesday);

                    // Calculate the date of this week's Friday (withdrawal)
                    var daysUntilFriday = ((int)DayOfWeek.Friday - (int)dayOfWeek + 7) % 7;
                    if (daysUntilFriday == 0 && dayOfWeek != DayOfWeek.Friday)
                        daysUntilFriday = 7;
                    var fridayWithdrawal = today.AddDays(daysUntilFriday);

                    var orderCount = await _dbContext.Set<DriverWalletTransaction>()
                        .CountAsync(t => t.WalletId == wallet.Id &&
                                   t.TransactionType == DriverTransactionType.Delivery &&
                                   !t.IsProcessed &&
                                   t.CreatedAt.Date <= TuesdayCutoff);

                    result.Add(new PendingDriverWithdrawalResponseModel
                    {
                        DriverId = wallet.DriverId,
                        DriverName = wallet.Driver.Name,
                        BankName = defaultBankAccount.BankName,
                        AccountNumber = defaultBankAccount.AccountNumber,
                        AccountName = defaultBankAccount.AccountHolderName,
                        Amount = wallet.PendingWithdrawal,
                        OrderCount = orderCount,
                        ScheduledDate = fridayWithdrawal
                    });
                }

                return ApiResponseModel<List<PendingDriverWithdrawalResponseModel>>.Success(
                    "Pending withdrawals retrieved successfully",
                    result,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending driver withdrawals");
                return ApiResponseModel<List<PendingDriverWithdrawalResponseModel>>.Fail(
                    $"Error retrieving pending withdrawals: {ex.Message}",
                    500);
            }
        }

        public async Task<ApiResponseModel<DriverEarningsSummaryResponseModel>> GetEarningsSummaryAsync(
            string driverId,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                // Verify driver exists
                var driver = await _dbContext.Drivers
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver == null)
                {
                    return ApiResponseModel<DriverEarningsSummaryResponseModel>.Fail(
                        "Driver not found",
                        404);
                }

                // Get wallet
                var wallet = await _dbContext.Set<DriverWallet>()
                    .FirstOrDefaultAsync(w => w.DriverId == driverId);

                if (wallet == null)
                {
                    // Create an empty summary if no wallet exists
                    return ApiResponseModel<DriverEarningsSummaryResponseModel>.Success(
                        "No earnings found for driver",
                        new DriverEarningsSummaryResponseModel
                        {
                            TotalEarnings = 0,
                            CompletedDeliveries = 0,
                            AveragePerDelivery = 0,
                            HighestPayout = 0,
                            WithdrawnAmount = 0,
                            AvailableBalance = 0,
                            WeeklyBreakdown = new List<EarningsByWeekResponseModel>()
                        },
                        200);
                }

                // Get all earnings transactions in the date range
                var earningsTransactions = await _dbContext.Set<DriverWalletTransaction>()
                    .Where(t => t.WalletId == wallet.Id &&
                           t.TransactionType == DriverTransactionType.Delivery &&
                           t.CreatedAt >= startDate &&
                           t.CreatedAt <= endDate)
                    .ToListAsync();

                // Get all withdrawal transactions in the date range
                var withdrawalTransactions = await _dbContext.Set<DriverWalletTransaction>()
                    .Where(t => t.WalletId == wallet.Id &&
                           t.TransactionType == DriverTransactionType.Withdrawal &&
                           t.CreatedAt >= startDate &&
                           t.CreatedAt <= endDate)
                    .ToListAsync();

                // Calculate summary statistics
                decimal totalEarnings = earningsTransactions.Sum(t => t.Amount);
                int completedDeliveries = earningsTransactions.Count;
                decimal averagePerDelivery = completedDeliveries > 0 ? totalEarnings / completedDeliveries : 0;
                decimal highestPayout = earningsTransactions.Any() ? earningsTransactions.Max(t => t.Amount) : 0;
                decimal withdrawnAmount = withdrawalTransactions.Sum(t => -t.Amount); // Withdrawals are stored as negative amounts

                // Group by week for weekly breakdown
                var weeklyBreakdown = earningsTransactions
                    .GroupBy(t => GetWeekNumber(t.CreatedAt))
                    .Select(g => new EarningsByWeekResponseModel
                    {
                        WeekNumber = g.Key,
                        StartDate = GetStartDateOfWeek(g.Min(t => t.CreatedAt)),
                        EndDate = GetEndDateOfWeek(g.Min(t => t.CreatedAt)),
                        EarningsAmount = g.Sum(t => t.Amount),
                        DeliveryCount = g.Count()
                    })
                    .OrderBy(w => w.StartDate)
                    .ToList();

                // Create the response
                var response = new DriverEarningsSummaryResponseModel
                {
                    TotalEarnings = totalEarnings,
                    CompletedDeliveries = completedDeliveries,
                    AveragePerDelivery = averagePerDelivery,
                    HighestPayout = highestPayout,
                    WithdrawnAmount = withdrawnAmount,
                    AvailableBalance = wallet.Balance,
                    WeeklyBreakdown = weeklyBreakdown
                };

                return ApiResponseModel<DriverEarningsSummaryResponseModel>.Success(
                    "Earnings summary retrieved successfully",
                    response,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver earnings summary");
                return ApiResponseModel<DriverEarningsSummaryResponseModel>.Fail(
                    $"Error retrieving earnings summary: {ex.Message}",
                    500);
            }
        }

        private async Task EnrichTransactionsWithOrderDetails(List<DriverWalletTransactionResponseModel> transactions)
        {
            // Get all order IDs from the transactions
            var orderIds = transactions
                .Where(t => !string.IsNullOrEmpty(t.RelatedOrderId))
                .Select(t => t.RelatedOrderId)
                .Distinct()
                .ToList();

            if (!orderIds.Any())
                return;

            // Get order details for these IDs
            var orders = await _dbContext.Set<CargoOrders>()
                .Where(o => orderIds.Contains(o.Id))
                .Select(o => new
                {
                    OrderId = o.Id,
                    PickupLocation = o.PickupLocation,
                    DeliveryLocation = o.DeliveryLocation
                })
                .ToDictionaryAsync(o => o.OrderId, o => new { o.PickupLocation, o.DeliveryLocation });

            // Add order details to transactions
            foreach (var transaction in transactions)
            {
                if (!string.IsNullOrEmpty(transaction.RelatedOrderId) &&
                    orders.TryGetValue(transaction.RelatedOrderId, out var orderDetails))
                {
                    transaction.OrderPickupLocation = orderDetails.PickupLocation;
                    transaction.OrderDeliveryLocation = orderDetails.DeliveryLocation;
                }
            }
        }

        private bool IsTransactionForCurrentWeekWithdrawal(
            DateTime transactionDate,
            DayOfWeek transactionDayOfWeek,
            DateTime today,
            DayOfWeek currentDayOfWeek)
        {
            // Calculate the start of the current week (Sunday)
            var daysToSubtract = (int)currentDayOfWeek;
            var startOfCurrentWeek = today.AddDays(-daysToSubtract);

            // If transaction is from this week and before Tuesday cutoff, it's for this week's withdrawal
            if (transactionDate >= startOfCurrentWeek)
            {
                if (transactionDayOfWeek <= DayOfWeek.Tuesday)
                {
                    return true;
                }
            }

            // If transaction is from last week after Tuesday cutoff, it's for this week's withdrawal
            var startOfLastWeek = startOfCurrentWeek.AddDays(-7);
            if (transactionDate >= startOfLastWeek && transactionDate < startOfCurrentWeek)
            {
                if (transactionDayOfWeek > DayOfWeek.Tuesday)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetWeekNumber(DateTime date)
        {
            // ISO 8601 week number
            var day = (int)System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                date.AddDays(4 - (day == 0 ? 7 : day)),
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

        private DateTime GetStartDateOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private DateTime GetEndDateOfWeek(DateTime date)
        {
            return GetStartDateOfWeek(date).AddDays(6);
        }
    }
}