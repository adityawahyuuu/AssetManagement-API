using API.DTOs;
using API.Models;
using AutoMapper;

namespace API.Services.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserRegisterDto, user_login>()
                .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.username, opt => opt.MapFrom(src => src.Username));
        }
    }
}
