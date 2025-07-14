using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    /// <summary>
    /// Model for CSV export of truck data
    /// </summary>
    public class TruckCsvExportModel
    {
        public string Id { get; set; }
        public string PlateNumber { get; set; }
        public string TruckName { get; set; }
        public string TruckType { get; set; }
        public string TruckCapacity { get; set; }
        public string TruckStatus { get; set; }
        public string ApprovalStatus { get; set; }
        public string TruckLicenseExpiryDate { get; set; }
        public string InsuranceExpiryDate { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }
        public string DriverEmail { get; set; }
        public string TruckOwnerName { get; set; }
        public string IsDriverOwned { get; set; }
        public int TotalCargoOrders { get; set; }
        public int TotalNormalOrders { get; set; }
        public string IsCurrentlyOnTrip { get; set; }
        public string CurrentTripType { get; set; }
        public string CreatedAt { get; set; }
        
        /// <summary>
        /// Convert from EnhancedTruckResponseModel to CSV model
        /// </summary>
        public static TruckCsvExportModel FromEnhancedModel(EnhancedTruckResponseModel model)
        {
            return new TruckCsvExportModel
            {
                Id = model.Id,
                PlateNumber = model.PlateNumber,
                TruckName = model.TruckName ?? "",
                TruckType = model.TruckType,
                TruckCapacity = model.TruckCapacity,
                TruckStatus = model.TruckStatus.ToString(),
                ApprovalStatus = model.ApprovalStatus.ToString(),
                TruckLicenseExpiryDate = model.TruckLicenseExpiryDate,
                InsuranceExpiryDate = model.InsuranceExpiryDate,
                DriverName = model.DriverName ?? "",
                DriverPhone = model.DriverPhone ?? "",
                DriverEmail = model.DriverEmail ?? "",
                TruckOwnerName = model.TruckOwnerName ?? "",
                IsDriverOwned = model.IsDriverOwnedTruck ? "Yes" : "No",
                TotalCargoOrders = model.TotalCargoOrders,
                TotalNormalOrders = model.TotalNormalOrders,
                IsCurrentlyOnTrip = model.IsCurrentlyOnTrip ? "Yes" : "No",
                CurrentTripType = model.CurrentTripType ?? "",
                CreatedAt = model.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
    }
}