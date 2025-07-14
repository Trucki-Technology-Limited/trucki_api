using trucki.Entities;

namespace trucki.Models.RequestModel
{
    /// <summary>
    /// Query parameters for fetching trucks with pagination, filtering, and search
    /// </summary>
    public class GetTrucksQueryDto
    {
        /// <summary>
        /// Page number (starts from 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// Number of items per page (max 50)
        /// </summary>
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// Search term for PlateNumber and TruckName
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// Filter by truck type (e.g., "Flatbed", "BoxBody", "Cargo Van", etc.)
        /// </summary>
        public string? TruckType { get; set; }
        
        /// <summary>
        /// Filter by truck status
        /// </summary>
        public TruckStatus? TruckStatus { get; set; }
        
        /// <summary>
        /// Filter by approval status
        /// </summary>
        public ApprovalStatus? ApprovalStatus { get; set; }
        
        /// <summary>
        /// Filter by driver owned trucks only
        /// </summary>
        public bool? IsDriverOwned { get; set; }
        
        /// <summary>
        /// Filter by truck owner ID (for truck owner dashboard)
        /// </summary>
        public string? TruckOwnerId { get; set; }
        
        /// <summary>
        /// Sort field (PlateNumber, TruckName, TruckType, TruckStatus, ApprovalStatus, CreatedAt)
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";
        
        /// <summary>
        /// Sort direction (true for descending, false for ascending)
        /// </summary>
        public bool SortDescending { get; set; } = true;
        
        /// <summary>
        /// Validate and normalize page size
        /// </summary>
        public void ValidateAndNormalize()
        {
            PageNumber = PageNumber < 1 ? 1 : PageNumber;
            PageSize = PageSize < 1 ? 10 : (PageSize > 50 ? 50 : PageSize);
            SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim();
            TruckType = string.IsNullOrWhiteSpace(TruckType) ? null : TruckType.Trim();
            TruckOwnerId = string.IsNullOrWhiteSpace(TruckOwnerId) ? null : TruckOwnerId.Trim();
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? "CreatedAt" : SortBy.Trim();
        }
    }
}