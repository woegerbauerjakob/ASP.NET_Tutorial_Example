using CinemaApp.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaApp.Data
{
    public static class DbSeeder
    {
        public static void Seed(CinemaDbContext context)
        {
            // 1. Check if database is already populated
            if (context.Movies.Any())
            {
                return; // DB has been seeded
            }

            // 2. Create Dummy Data
            var dune = new Movie
            {
                Title = "Dune: Part Two",
                Description = "Paul Atreides unites with Chani and the Fremen.",
                Showtimes = new List<Showtime>
                {
                    new Showtime { StartTime = DateTime.UtcNow.AddDays(1).AddHours(18), TicketPrice = 14.50m },
                    new Showtime { StartTime = DateTime.UtcNow.AddDays(1).AddHours(21), TicketPrice = 14.50m }
                }
            };

            var barbie = new Movie
            {
                Title = "Barbie",
                Description = "Barbie suffers a crisis that leads her to question her world and her existence.",
                Showtimes = new List<Showtime>
                {
                    new Showtime { StartTime = DateTime.UtcNow.AddDays(2).AddHours(16), TicketPrice = 12.00m }
                }
            };

            // 3. Add to Context and Save
            context.Movies.AddRange(dune, barbie);
            context.SaveChanges();
        }
    }
}
