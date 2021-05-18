using System;
using System.Collections.Generic;

namespace UKParliament.CodeTest.Data.Domain
{
    public class Person
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public DateTime DateOfBirth { get; set; }

        public ICollection<RoomBooking> Bookings { get; set; }
    }
}