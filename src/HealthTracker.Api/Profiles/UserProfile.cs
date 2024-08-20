using AutoMapper;
using HealthTracker.Entities.Dtos.Outgoing.Profile;
using HealtTracker.Entities.DbSet;
using HealtTracker.Entities.Dtos.Incoming;

namespace HealthTracker.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, User>()
                .ForMember(
                    dest => dest.FirstName,
                    from => from.MapFrom(x => $"{x.FirstName}")
                )
                .ForMember(
                    dest => dest.LastName,
                    from => from.MapFrom(x => $"{x.LastName}")
                )
                .ForMember(
                    dest => dest.Email,
                    from => from.MapFrom(x => $"{x.Email}")
                )
                .ForMember(
                    dest => dest.Phone,
                    from => from.MapFrom(x => $"{x.Phone}")
                )
                .ForMember(
                    dest => dest.DateofBirth,
                    from => from.MapFrom(x => Convert.ToDateTime(x.DateofBirth))
                )
                .ForMember(
                    dest => dest.Country,
                    from => from.MapFrom(x => $"{x.Country}")
                )
                .ForMember(
                    dest => dest.Status,
                    from => from.MapFrom(x => 1)
                );

            CreateMap<User, ProfileDto>()
            .ForMember(
                    dest => dest.FirstName,
                    from => from.MapFrom(x => $"{x.FirstName}")
                )
                .ForMember(
                    dest => dest.LastName,
                    from => from.MapFrom(x => $"{x.LastName}")
                )
                .ForMember(
                    dest => dest.Email,
                    from => from.MapFrom(x => $"{x.Email}")
                )
                .ForMember(
                    dest => dest.Phone,
                    from => from.MapFrom(x => $"{x.Phone}")
                )
                .ForMember(
                    dest => dest.DateofBirth,
                    from => from.MapFrom(x => $"{x.DateofBirth.ToShortDateString()}")
                )
                .ForMember(
                    dest => dest.Country,
                    from => from.MapFrom(x => $"{x.Country}")
                );
        }
    }
}