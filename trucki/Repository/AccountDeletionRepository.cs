using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;

namespace trucki.Repository
{
    public class AccountDeletionRepository : IAccountDeletionRepository
    {
        private readonly TruckiDBContext _context;

        public AccountDeletionRepository(TruckiDBContext context)
        {
            _context = context;
        }

        public async Task<AccountDeletionRequest> CreateAsync(AccountDeletionRequest request)
        {
            _context.AccountDeletionRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<AccountDeletionRequest?> GetByUserIdAsync(string userId)
        {
            return await _context.AccountDeletionRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);
        }

        public async Task<AccountDeletionRequest> UpdateAsync(AccountDeletionRequest request)
        {
            _context.AccountDeletionRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> HasPendingRequestAsync(string userId)
        {
            return await _context.AccountDeletionRequests
                .AnyAsync(r => r.UserId == userId && r.Status == AccountDeletionStatus.Pending);
        }
    }
}