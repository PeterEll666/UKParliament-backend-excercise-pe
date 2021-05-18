using System.Collections.Generic;

namespace UKParliament.CodeTest.Data.Domain
{
    public class Room
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public ICollection<RoomBooking> Bookings { get; set; }
    }
}