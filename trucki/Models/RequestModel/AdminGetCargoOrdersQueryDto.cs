using trucki.Entities;

namespace trucki.Models.RequestModel
{
    /// <summary>
    /// Query parameters for admin to fetch cargo orders with advanced filtering, pagination, and search
    /// </summary>
    public class AdminGetCargoOrdersQueryDto
    {
        /// <summary>
        /// Page number (starts from 1)
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// Number of items per page (max 100)
        /// </summary>
        public int PageSize { get; set; } = 20;
        
        /// <summary>
        /// Search term for cargo owner name, company name, email, pickup/delivery location, consignment number
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// Filter by cargo order status
        /// </summary>
        public CargoOrderStatus? Status { get; set; }
        
        /// <summary>
        /// Filter by payment status
        /// </summary>
        public PaymentStatus? PaymentStatus { get; set; }
        
        /// <summary>
        /// Filter by cargo owner name
        /// </summary>
        public string? CargoOwnerName { get; set; }
        
        
        /// <summary>
        /// Filter orders created from this date (inclusive)
        /// </summary>
        public DateTime? CreatedFrom { get; set; }
        
        /// <summary>
        /// Filter orders created up to this date (inclusive)
        /// </summary>
        public DateTime? CreatedTo { get; set; }
        
        
        /// <summary>
        /// Filter by payment method
        /// </summary>
        public PaymentMethodType? PaymentMethod { get; set; }
        
        /// <summary>
        /// Filter flagged orders only
        /// </summary>
        public bool? IsFlagged { get; set; }
        
        /// <summary>
        /// Filter paid orders only
        /// </summary>
        public bool? IsPaid { get; set; }
        
        /// <summary>
        /// Filter orders with accepted bids only
        /// </summary>
        public bool? HasAcceptedBid { get; set; }
        
        /// <summary>
        /// Sort field (CreatedAt, TotalAmount, Status, PaymentStatus, PickupDateTime, DeliveryDateTime, CargoOwnerName)
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";
        
        /// <summary>
        /// Sort direction (true for descending, false for ascending)
        /// </summary>
        public bool SortDescending { get; set; } = true;
        
        /// <summary>
        /// Include related entities in response (default true for admin)
        /// </summary>
        public bool IncludeCargoOwner { get; set; } = true;
        public bool IncludeItems { get; set; } = true;
        public bool IncludeBids { get; set; } = true;
        public bool IncludeAcceptedBid { get; set; } = true;
        
        /// <summary>
        /// Validate and normalize query parameters
        /// </summary>
        public void ValidateAndNormalize()
        {
            PageNumber = PageNumber < 1 ? 1 : PageNumber;
            PageSize = PageSize < 1 ? 20 : (PageSize > 100 ? 100 : PageSize);
            SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim();
            CargoOwnerName = string.IsNullOrWhiteSpace(CargoOwnerName) ? null : CargoOwnerName.Trim();
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? "CreatedAt" : SortBy.Trim();
            
            // Validate date ranges
            if (CreatedFrom.HasValue && CreatedTo.HasValue && CreatedFrom > CreatedTo)
            {
                var temp = CreatedFrom;
                CreatedFrom = CreatedTo;
                CreatedTo = temp;
            }
            
        }
    }
}