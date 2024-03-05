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


    }
}