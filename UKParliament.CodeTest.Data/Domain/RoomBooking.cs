using System;

namespace UKParliament.CodeTest.Data.Domain
{
    public class RoomBooking
    {
        public int Id { get; set; }
        
        public int PersonId { get; set; }
        
        public Person Person { get; set; }
        
        public int RoomId { get; set; }
        
        public Room Room { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}