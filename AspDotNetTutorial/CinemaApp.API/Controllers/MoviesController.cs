using AutoMapper;
using CinemaApp.Data;
using CinemaApp.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly CinemaDbContext _context;
        private readonly IMapper _mapper;

        // Dependency Injection: Requesting the DB and the Mapper
        public MoviesController(CinemaDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<ActionResult<List<MovieDto>>> GetMovies()
        {
            // 1. Fetch from DB (Entities)
            // We use .Include() to fetch the related Showtimes
            var movies = await _context.Movies
                                       .Include(m => m.Showtimes)
                                       .ToListAsync();

            // 2. Convert to DTOs using AutoMapper
            var movieDtos = _mapper.Map<List<MovieDto>>(movies);

            // 3. Return JSON
            return Ok(movieDtos);
        }

        // GET: api/movies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovie(int id)
        {
            var movie = await _context.Movies
                                      .Include(m => m.Showtimes)
                                      .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var movieDto = _mapper.Map<MovieDto>(movie);

            return Ok(movieDto);
        }
    }
}
