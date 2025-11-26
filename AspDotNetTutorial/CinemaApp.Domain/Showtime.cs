using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaApp.Domain
{
    public class Showtime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public decimal TicketPrice { get; set; }

        public DateTime LastModified { get; set; }

        // Foreign Key: Links this showtime to a specific Movie ID
        public int MovieId { get; set; }

        // Navigation Property: Allows us to access the full Movie object if needed
        public Movie? Movie { get; set; }
    }
}
