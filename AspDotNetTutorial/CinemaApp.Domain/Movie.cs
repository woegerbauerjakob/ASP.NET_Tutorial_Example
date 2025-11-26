using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaApp.Domain
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // This will be handled automatically by the DbContext later
        public DateTime LastModified { get; set; }

        // Relationship: One Movie has many Showtimes
        // We initialize it to an empty list to avoid NullReferenceExceptions
        public List<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}
