using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices
{
    public interface IWalletService
    {
        // Get wallet balance for a cargo owner
        Task<decimal> GetWalletBalance(string cargoOwnerId);
        
        // Add funds to a wallet
        Task<ApiResponseModel<bool>> AddFundsToWallet(
            string cargoOwnerId, 
            decimal amount, 
            string description, 
            WalletTransactionType type, 
            string relatedOrderId = null);
        
        // Deduct funds from a wallet
        Task<ApiResponseModel<bool>> DeductFundsFromWallet(
            string cargoOwnerId, 
            decimal amount, 
            string description, 
            WalletTransactionType type, 
            string relatedOrderId = null);
        
        // Get transaction history
        Task<ApiResponseModel<PagedResponse<WalletTransactionResponseModel>>> GetWalletTransactions(
            string cargoOwnerId, 
            GetWalletTransactionsQueryDto query);
        
        // Ensure a wallet exists for the cargo owner (create if not)
        Task<CargoOwnerWallet> EnsureWalletExists(string cargoOwnerId);
    }
}