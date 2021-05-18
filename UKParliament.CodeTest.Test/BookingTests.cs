using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using UKParliament.CodeTest.Data;
using UKParliament.CodeTest.Data.Domain;
using UKParliament.CodeTest.Services;
using UKParliament.CodeTest.Web;
using Xunit;

namespace UKParliament.CodeTest.Test
{
    public class BookingTests : IDisposable
    {
        private readonly BookingService bookingService;
        private readonly RoomBookingsContext _context;

        public BookingTests()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            _context = new RoomBookingsContext(new DbContextOptionsBuilder<RoomBookingsContext>()
                                                                           .UseInMemoryDatabase("BookingTests").Options);
            _context.People.Add(new Person { Id = 3, Name = "Test Person", DateOfBirth = DateTime.Parse("3 May 1970") });
            _context.Rooms.Add(new Room { Id = 1, Name = "Room 1" });
            _context.SaveChanges();

            bookingService = new BookingService(mapper, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async void CanAddBooking()
        {
            var response = await bookingService.AddAsync(new BookingInfo { RoomId= 1, PersonId = 3, StartTime = DateTime.Parse("1 Jan 2021 10:00"), DurationMinutes=60 });

            Assert.True(string.IsNullOrEmpty(response.ErrorMessage));
            var added = await _context.RoomBookings.FindAsync(response.Id);
            Assert.Equal(DateTime.Parse("1 Jan 2021 10:00"), added.StartTime);
            Assert.Equal(DateTime.Parse("1 Jan 2021 11:00"), added.EndTime);
        }

        [Fact]
        public async void AddBookingDurationGt60()
        {
            var response = await bookingService.AddAsync(new BookingInfo { RoomId = 1, PersonId = 3, StartTime = DateTime.Parse("1 Jan 2021 10:00"), DurationMinutes = 61 });

            Assert.Equal("Duration must be between 1 and 60 minutes", response.ErrorMessage); 
        }

        [Fact]
        public async void AddBookingDurationLt1()
        {
            var response = await bookingService.AddAsync(new BookingInfo { RoomId = 1, PersonId = 3, StartTime = DateTime.Parse("1 Jan 2021 10:00"), DurationMinutes = 0 });

            Assert.Equal("Duration must be between 1 and 60 minutes", response.ErrorMessage);
        }

        [Fact]
        public async void AddBookingPersonNotFound()
        {
            var response = await bookingService.AddAsync(new BookingInfo { RoomId = 1, PersonId = 1, StartTime = DateTime.Parse("1 Jan 2021 10:00"), DurationMinutes =15 });

            Assert.Equal("Person not found", response.ErrorMessage);
        }

        [Fact]
        public async void AddBookingRoomNotFound()
        {
            var response = await bookingService.AddAsync(new BookingInfo { RoomId = 3, PersonId = 3, StartTime = DateTime.Parse("1 Jan 2021 10:00"), DurationMinutes = 15 });

            Assert.Equal("Room not found", response.ErrorMessage);
        }

        [Fact]
        public async void AddBookingOverlapsExistingBooking()
        {
            _context.RoomBookings.Add(new RoomBooking { Id = 1, PersonId = 3, RoomId = 1, StartTime = DateTime.Parse("1 Jan 2021 10:30"), EndTime = DateTime.Parse("1 Jan 2021 11:00") });
            _context.SaveChanges();

            var response = await bookingService.AddAsync(new BookingInfo { RoomId = 1, PersonId = 3, StartTime = DateTime.Parse("1 Jan 2021 10:00"), DurationMinutes = 31 });

            Assert.Equal("Overlaps existing booking", response.ErrorMessage);
        }

        [Fact]
        public async void AddBookingJustFitsInSlot()
        {
            _context.RoomBookings.Add(new RoomBooking { Id = 1, PersonId = 3, RoomId = 1, StartTime = DateTime.Parse("1 Jan 2021 10:00"), EndTime = DateTime.Parse("1 Jan 2021 10:30") });
            _context.RoomBookings.Add(new RoomBooking { Id = 2, PersonId = 3, RoomId = 1, StartTime = DateTime.Parse("1 Jan 2021 11:00"), EndTime = DateTime.Parse("1 Jan 2021 11:30") });
            _context.SaveChanges();

            var response = await bookingService.AddAsync(new BookingInfo { RoomId = 1, PersonId = 3, StartTime = DateTime.Parse("1 Jan 2021 10:30"), DurationMinutes = 30 });

            Assert.True(string.IsNullOrEmpty(response.ErrorMessage));
        }
 
        [Fact]
        public async void CanDeleteBooking()
        {
            _context.RoomBookings.Add(new RoomBooking { Id = 1, PersonId = 3, RoomId = 1, StartTime = DateTime.Parse("1 Jan 2021 10:30"), EndTime = DateTime.Parse("1 Jan 2021 11:00") });
            _context.SaveChanges();

            await bookingService.DeleteAsync(1);

            Assert.Null(_context.RoomBookings.Find(1));
        }

        [Fact]
        public async void FindAvailableRooms()
        {
            _context.Rooms.Add(new Room { Id = 2, Name = "Room 2" });
            _context.Rooms.Add(new Room { Id = 3, Name = "Room 3" });
            _context.RoomBookings.Add(new RoomBooking { Id = 1, PersonId = 3, RoomId = 1, StartTime = DateTime.Parse("1 Jan 2021 10:00"), EndTime = DateTime.Parse("1 Jan 2021 10:30") });
            _context.RoomBookings.Add(new RoomBooking { Id = 2, PersonId = 3, RoomId = 2, StartTime = DateTime.Parse("1 Jan 2021 11:00"), EndTime = DateTime.Parse("1 Jan 2021 11:30") });
            _context.SaveChanges();

            var result = await bookingService.FindAvailableRoomsAsync(DateTime.Parse("1 Jan 2021 10:00"), 60);

            Assert.Equal(2, result.Response.Count());
            Assert.Single(result.Response.Where(r => r.Name == "Room 2"));
            Assert.Single(result.Response.Where(r => r.Name == "Room 3"));

        }

        [Fact]
        public async void FindAvailableRoomsDurationLt1Min()
        {
            var result = await bookingService.FindAvailableRoomsAsync(DateTime.Parse("1 Jan 2021 10:00"), 0);

            Assert.Equal("Search duration cannot be less than 1 minute", result.ErrorMessage);
        }
    }
}
