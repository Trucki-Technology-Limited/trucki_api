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
        CreateMap<Business, BusinessResponseModel>()
            .ForMember(dest => dest.Metrics, opt => opt.Ignore());
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
        CreateMap<DriverResponseModel, Driver>().ReverseMap()
            .ForMember(dest => dest.OwnershipType, opt => opt.MapFrom(src => (int?)src.OwnershipType))
            .ForMember(dest => dest.IsManagedByDispatcher, opt => opt.MapFrom(src => (bool?)(src.OwnershipType == DriverOwnershipType.DispatcherManaged)))
            .ForMember(dest => dest.ManagedByDispatcherName, opt => opt.MapFrom(src => src.ManagedByDispatcher != null ? src.ManagedByDispatcher.Name : null));
        CreateMap<DriverProfileResponseModel, Driver>().ReverseMap()
            .ForMember(dest => dest.ManagedByDispatcherName, opt => opt.MapFrom(src => src.ManagedByDispatcher != null ? src.ManagedByDispatcher.Name : null));
        CreateMap<OfficerBusinessResponseModel, Business>().ReverseMap();
        CreateMap<CargoOwner, CargoOwnerProfileResponseModel>();
        CreateMap<CargoOrders, CargoOrderResponseModel>();
        CreateMap<CargoTruckResponseModel, Truck>().ReverseMap();
        CreateMap<Bid, BidResponseModel>()
         .ForMember(dest => dest.Driver, opt => opt.MapFrom(src => src.Truck.Driver))
         .ForMember(dest => dest.SubmittedByDispatcherName, opt => opt.MapFrom(src => src.SubmittedByDispatcher != null ? src.SubmittedByDispatcher.Name : null));
        CreateMap<Invoice, InvoiceResponseModel>()
     .ForMember(dest => dest.Order, opt => opt.Ignore());
        CreateMap<PaymentAccount, BankAccountResponseModel>();
        CreateMap<CargoOrderItem, CargoOrderItemResponseModel>();
        CreateMap<CargoOrders, CargoOrderSummaryModel>()
             .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Items.Count))
             .ForMember(dest => dest.TotalWeight, opt => opt.MapFrom(src => src.Items.Sum(i => i.Weight * i.Quantity)));
        CreateMap<CargoOwner, CargoOwnerResponseModel>();
        CreateMap<DriverBankAccount, DriverBankAccountResponseModel>();
        CreateMap<DatabaseNotification, NotificationResponseModel>();
        CreateMap<WalletTransaction, WalletTransactionResponseModel>()
           .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount));
        CreateMap<CargoOwner, AdminCargoOwnerResponseModel>();
        CreateMap<CargoOwner, AdminCargoOwnerDetailsResponseModel>();
        CreateMap<AccountDeletionRequest, AccountDeletionResponseModel>();

    }
}