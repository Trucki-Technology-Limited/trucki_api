﻿using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class AllTruckResponseModel
    {
        public string Id { get; set; }
        public List<string> Documents { get; set; } = new List<string>(); // Initialized to prevent null        
        public string? CertOfOwnerShip { get; set; }
        public string PlateNumber { get; set; }
        public string TruckCapacity { get; set; }
        public string? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string Capacity { get; set; }
        public string? TruckOwnerId { get; set; } // Made nullable
        public string? TruckOwnerName { get; set; } // Made nullable
        public string? TruckName { get; set; }
        public TruckiType TruckType { get; set; }
        public string TruckLicenseExpiryDate { get; set; }
        public string RoadWorthinessExpiryDate { get; set; }
        public string InsuranceExpiryDate { get; set; }
        public string TruckNumber { get; set; }
        public TruckStatus TruckStatus { get; set; }
         public DateTime CreatedAt { get; set; }
    }
       public class CargoTruckResponseModel
    {
        public string Id { get; set; }      
        public string PlateNumber { get; set; }
        public string TruckCapacity { get; set; }
        public string? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string Capacity { get; set; }
        public string? TruckName { get; set; }
        public TruckiType TruckType { get; set; }
        public string TruckNumber { get; set; }
    }
    public class TruckStatusCountResponseModel
    {
        public int EnRouteCount { get; set; }
        public int AvailableCount { get; set; }
        public int OutOfServiceCount { get; set; }
    }
}
