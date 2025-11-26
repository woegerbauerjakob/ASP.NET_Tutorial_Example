using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaApp.Shared
{
    public class ShowtimeDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public decimal TicketPrice { get; set; }
    }
}
