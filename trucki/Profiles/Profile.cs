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
        CreateMap<CargoOwner, CargoOwnerProfileResponseModel>();
        CreateMap<CargoOrders, CargoOrderResponseModel>();
        CreateMap<CargoTruckResponseModel, Truck>().ReverseMap();
        CreateMap<Bid, BidResponseModel>()
         .ForMember(dest => dest.Driver, opt => opt.MapFrom(src => src.Truck.Driver));
        CreateMap<Invoice, InvoiceResponseModel>()
               .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order));
        CreateMap<PaymentAccount, BankAccountResponseModel>();
        CreateMap<CargoOrderItem, CargoOrderItemResponseModel>();
        CreateMap<CargoOrders, CargoOrderSummaryModel>()
             .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Items.Count))
             .ForMember(dest => dest.TotalWeight, opt => opt.MapFrom(src => src.Items.Sum(i => i.Weight * i.Quantity)));
        CreateMap<CargoOwner, CargoOwnerResponseModel>();
        CreateMap<DriverBankAccount, DriverBankAccountResponseModel>();
        CreateMap<DatabaseNotification, NotificationResponseModel>();

    }
}