using System;
using System.Collections.Generic;

namespace UKParliament.CodeTest.Data.Domain
{
    public class BookingInfo
    {
        public int PersonId { get; set; }
        
        public int RoomId { get; set; }

        public DateTime StartTime { get; set; }

        public int DurationMinutes { get; set; }
    }
}