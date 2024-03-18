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
        CreateMap<CreateBusinessDto, Business>().ReverseMap();
        CreateMap<ManagerResponse, Manager>().ReverseMap();
        CreateMap<CreateManagerDto, Manager>().ReverseMap();
        CreateMap<User, ResetPasswordDto>().ReverseMap();
        CreateMap<RoutesResponse, BusinessResponse>().ReverseMap();
        CreateMap<Business, BusinessResponse>().ReverseMap();

    }
}