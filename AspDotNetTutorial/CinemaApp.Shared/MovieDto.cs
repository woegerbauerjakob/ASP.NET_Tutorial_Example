using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaApp.Shared
{
    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ShowtimeDto> Showtimes { get; set; } = new();
    }
}
