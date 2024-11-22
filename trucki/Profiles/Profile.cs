using AutoMapper;
using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Profiles;

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<AllBusinessResponseModel, Business>().ReverseMap();
        CreateMap<BusinessResponseModel, Business>().ReverseMap();
        CreateMap<ManagersBusinessResponseModel, Business>().ReverseMap();
        CreateMap<RouteResponseModel, Routes>().ReverseMap();
        CreateMap<AllManagerResponseModel, Manager>().ReverseMap();
        CreateMap<AllManagerResponseModel, Manager>().ReverseMap();
        CreateMap<AllTruckOwnerResponseModel, TruckOwner>().ReverseMap();
        CreateMap<AllDriverResponseModel, Driver>().ReverseMap();
        CreateMap<AllOfficerResponseModel, Officer>().ReverseMap();
        CreateMap<AllTruckResponseModel, Truck>().ReverseMap();
        CreateMap<AllOfficerResponseModel, Officer>().ReverseMap();   
        CreateMap<AllCustomerResponseModel, Customer>().ReverseMap();
        CreateMap<AllOrderResponseModel, Order>().ReverseMap();
        CreateMap<BankDetailsResponseModel, BankDetails>().ReverseMap();
        CreateMap<TruckOwnerResponseModel, TruckOwner>().ReverseMap();
        CreateMap<DriverResponseModel, Driver>().ReverseMap();
        CreateMap<DriverProfileResponseModel, Driver>().ReverseMap();
          CreateMap<OfficerBusinessResponseModel, Business>().ReverseMap();
    }
}