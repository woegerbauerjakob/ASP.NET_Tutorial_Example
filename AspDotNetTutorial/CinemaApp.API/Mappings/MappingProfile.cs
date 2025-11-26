using AutoMapper;
using CinemaApp.Domain;
using CinemaApp.Shared;

namespace CinemaApp.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Movie, MovieDto>();
            CreateMap<Showtime, ShowtimeDto>();

            CreateMap<MovieDto, Movie>();
            CreateMap<ShowtimeDto, Showtime>();
        }
    }
}
