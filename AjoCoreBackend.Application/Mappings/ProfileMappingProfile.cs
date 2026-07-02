using AjoCoreBackend.Application.DTOs.Profile;
using AjoCoreBackend.Domain.Entities;
using AutoMapper;
using System.Linq;

namespace AjoCoreBackend.Application.Mappings
{
    public class ProfileMappingProfile : Profile
    {
        public ProfileMappingProfile()
        {
            CreateMap<Trader, TraderProfileDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<CooperativeAdmin, CooperativeAdminProfileDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.AdministeredGroups, opt => opt.MapFrom(src => src.AdministeredGroups));

            CreateMap<CooperativeGroup, AdministeredGroupDto>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Name));
        }
    }
}
