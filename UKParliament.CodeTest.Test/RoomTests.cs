using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using UKParliament.CodeTest.Data;
using UKParliament.CodeTest.Data.Domain;
using UKParliament.CodeTest.Services;
using UKParliament.CodeTest.Services.Models;
using UKParliament.CodeTest.Web;
using Xunit;

namespace UKParliament.CodeTest.Test
{
    public class RoomTests : IDisposable
    {
        private readonly RoomService roomService;
        private readonly RoomBookingsContext _context;

        public RoomTests()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            _context = new RoomBookingsContext(new DbContextOptionsBuilder<RoomBookingsContext>()
                                                                           .UseInMemoryDatabase("RoomTests").Options);
            _context.People.Add(new Person { Id = 3, Name = "Test Person", DateOfBirth = DateTime.Parse("3 May 1970") });
            _context.Rooms.Add(new Room { Id = 1, Name = "Room 1" });
            _context.SaveChanges();

            roomService = new RoomService(mapper, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async void CanGetRoom()
        {

            var result = await roomService.GetAsync(1);

            Assert.Equal("Room 1", result.Name);
        }

        [Fact]
        public async void CanSearchRooms()
        {
            _context.Rooms.Add(new Room { Id = 2, Name = "Room 2" });
            _context.Rooms.Add(new Room { Id = 3, Name = "Not Found" });
            _context.SaveChanges();

            var result = await roomService.SearchAsync("ROoM");

            Assert.Equal(2, result.Response.Count());
            Assert.Single(result.Response.Where(r => r.Name == "Room 1"));
            Assert.Single(result.Response.Where(r => r.Name == "Room 2"));
        }

        [Fact]
        public async void SearchRoomsSearchNameEmpty()
        {

            var result = await roomService.SearchAsync(string.Empty);

            Assert.Equal("Search name cannot be empty", result.ErrorMessage);
        }



        [Fact]
        public async void CanAddRoom()
        {
            var response = await roomService.AddAsync(new AddRoomInfo { Name = "Room 2" });

            var added = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == response.Id);
            Assert.Equal("Room 2", added.Name);
        }

        [Fact]
        public async void AddRoomWithDuplicateName()
        {
            var response = await roomService.AddAsync(new AddRoomInfo { Name = "Room 1" });

            Assert.Equal("Room already exists", response.ErrorMessage);
        }

        [Fact]
        public async void CanFindRoom()
        {

            var result = await roomService.GetAsync(1);

            Assert.Equal("Room 1", result.Name);
        }

        [Fact]
        public async void GetRoomnNotFoundReturnsNull()
        {

            var result = await roomService.GetAsync(4);

            Assert.Null(result);
        }

        [Fact]
        public async void CanUpdateRoom()
        {

            var result = await roomService.UpdateAsync(new RoomInfo { Id = 1, Name = "New Room 1"});

            Assert.Equal(string.Empty,result);
            var updated = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == 1);
            Assert.Equal("New Room 1", updated.Name);
        }

        [Fact]
        public async void UpdateRoomNotFoundReturnsError()
        {

            var result = await roomService.UpdateAsync(new RoomInfo { Id = 2, Name = "New Room 1" });

            Assert.Equal("Room not found",result);
        }

        [Fact]
        public async void DeleteRoomNotFoundReturnsOK()
        {
            var result = await roomService.DeleteAsync(3,-1);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async void DeleteRoomShiftToRoomNotFound()
        {
            var result = await roomService.DeleteAsync(1, 3);

            Assert.Equal("Shift to room not found", result);
        }

        [Fact]
        public async void DeleteRoomNoShiftBookingsDeleted()
        {
            _context.Rooms.Add(new Room { Id = 2, Name = "Room 2" });
            _context.RoomBookings.Add(new RoomBooking { Id = 1, PersonId = 1, RoomId = 1, StartTime = DateTime.Parse("12 Jan 2021 10:00"), EndTime = DateTime.Parse("12 Jan 2021 10:30") });
            _context.RoomBookings.Add(new RoomBooking { Id = 2, PersonId = 1, RoomId = 1, StartTime = DateTime.Parse("12 Jan 2021 10:30"), EndTime = DateTime.Parse("12 Jan 2021 11:00") });
            _context.RoomBookings.Add(new RoomBooking { Id = 3, PersonId = 1, RoomId = 2, StartTime = DateTime.Parse("12 Jan 2021 10:00"), EndTime = DateTime.Parse("12 Jan 2021 10:30") });

            var result = await roomService.DeleteAsync(1, -1);

            Assert.Equal(string.Empty, result);
            Assert.Empty(_context.RoomBookings.Where(rb => rb.RoomId == 1));
            Assert.Single(_context.RoomBookings.Where(rb => rb.RoomId == 2));
        }

        [Fact]
        public async void DeleteRoomWithShiftBookings()
        {
            _context.Rooms.Add(new Room { Id = 2, Name = "Room 2" });
            _context.RoomBookings.Add(new RoomBooking { Id = 1, PersonId = 1, RoomId = 1, StartTime = DateTime.Parse("12 Jan 2021 10:00"), EndTime = DateTime.Parse("12 Jan 2021 10:30") });
            _context.RoomBookings.Add(new RoomBooking { Id = 2, PersonId = 1, RoomId = 1, StartTime = DateTime.Parse("12 Jan 2021 10:30"), EndTime = DateTime.Parse("12 Jan 2021 11:00") });
            _context.RoomBookings.Add(new RoomBooking { Id = 3, PersonId = 1, RoomId = 2, StartTime = DateTime.Parse("12 Jan 2021 10:00"), EndTime = DateTime.Parse("12 Jan 2021 10:30") });

            var result = await roomService.DeleteAsync(1, 2);

            Assert.Equal(string.Empty, result);
            Assert.Empty(_context.RoomBookings.Where(rb => rb.RoomId == 1));
            Assert.Equal(3, _context.RoomBookings.Where(rb => rb.RoomId == 2).Count());
        }


    }
}
