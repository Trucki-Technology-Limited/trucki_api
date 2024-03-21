using AutoMapper;
using trucki.DTOs;
using trucki.Entities;
using trucki.Models.ResponseModels;

namespace trucki.Profiles;

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<AllBusinessResponseModel, Business>().ReverseMap();
        CreateMap<BusinessResponseModel, Business>().ReverseMap();
        CreateMap<RouteResponseModel, Routes>().ReverseMap();
        CreateMap<DriverResponse, Driver>().ReverseMap();
        CreateMap<DriversResponse, Driver>().ReverseMap();
        CreateMap<Driver, CreateDriverDto>().ReverseMap();
        CreateMap<Company, CompanyResponseDto>().ReverseMap();
        CreateMap<CreateCompanyDto, Company>().ReverseMap();
        CreateMap<Manager, ManagerResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ReverseMap();
        CreateMap<CreateManagerDto, Manager>().ReverseMap();
        CreateMap<User, ResetPasswordDto>().ReverseMap();
        CreateMap<RoutesResponse, BusinessResponse>().ReverseMap();
        CreateMap<Business, BusinessResponse>().ReverseMap();
        CreateMap<Business, CreateBusinessDto>().ReverseMap();

        CreateMap<User, Manager>();

        CreateMap<CreateManagerDto, User>()
          .ForMember(dest => dest.firstName, opt => opt.MapFrom(src => src.ManagerName))
          .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Ignore mapping for auto-generated values
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Ignore mapping for auto-generated values
          .ForMember(dest => dest.LastLogin, opt => opt.Ignore()); // Ignore mapping for auto-generated values

        CreateMap<Manager, CreateManagerDto>();

        CreateMap<User, CreateManagerDto>()
               .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => $"{src.firstName} {src.lastName}"))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
               .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role)); // Assuming there's a property called Role in User class

        CreateMap<UpdateManagerDto, Manager>()
           .ForMember(dest => dest.ManagerType, opt => opt.MapFrom(src => GetManagerType(src.Role)))
           .ReverseMap();

    }

    private ManagerType GetManagerType(string role)
    {
        // Implement logic to determine ManagerType based on the role or any other criteria
        // For example, you can use a switch statement or if-else conditions
        // This is just a placeholder; replace it with your actual logic
        return role switch
        {
            "Operational" => ManagerType.OperationalManager,
            "Financial" => ManagerType.FinancialManager,
            _ => ManagerType.OperationalManager // Default value if role doesn't match
        };
    }
}