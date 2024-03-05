using AutoMapper;
using trucki.DTOs;
using trucki.Entities;

namespace trucki.MappingProfiles
{
   public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Driver, CreateDriverDto>().ReverseMap();

        }
    }
}
