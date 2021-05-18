using Microsoft.EntityFrameworkCore;
using UKParliament.CodeTest.Data.Domain;

namespace UKParliament.CodeTest.Data
{
    public class RoomBookingsContext : DbContext
    {
        public RoomBookingsContext(DbContextOptions<RoomBookingsContext> options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }

        public DbSet<Room> Rooms { get; set; }

        public DbSet<RoomBooking> RoomBookings { get; set; }
    }
}