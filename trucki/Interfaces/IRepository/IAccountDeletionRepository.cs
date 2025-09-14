using trucki.Entities;

namespace trucki.Interfaces.IRepository
{
    public interface IAccountDeletionRepository
    {
        Task<AccountDeletionRequest> CreateAsync(AccountDeletionRequest request);
        Task<AccountDeletionRequest?> GetByUserIdAsync(string userId);
        Task<AccountDeletionRequest> UpdateAsync(AccountDeletionRequest request);
        Task<bool> HasPendingRequestAsync(string userId);
    }
}