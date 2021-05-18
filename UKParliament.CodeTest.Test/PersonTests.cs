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
    public class PersonTests : IDisposable
    {
        private readonly PersonService personService;
        private readonly RoomBookingsContext _context;

        public PersonTests()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            _context = new RoomBookingsContext(new DbContextOptionsBuilder<RoomBookingsContext>()
                                                                           .UseInMemoryDatabase("PersonTests").Options);
            _context.People.Add(new Person { Id = 3, Name = "Test Person", DateOfBirth = DateTime.Parse("3 May 1970") });
            _context.Rooms.Add(new Room { Id = 1, Name = "Room 1" });
            _context.SaveChanges();

            personService = new PersonService(mapper, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async void CanGetPerson()
        {

            var result = await personService.GetAsync(3);

            Assert.Equal("Test Person", result.Name);
            Assert.Equal(DateTime.Parse("3 May 1970"), result.DateOfBirth);
        }

        [Fact]
        public async void CanSearchPeople()
        {
            _context.People.Add(new Person { Id = 1, Name = "Test Person 2", DateOfBirth = DateTime.Parse("3 May 1971") });
            _context.People.Add(new Person { Id = 2, Name = "Not Found", DateOfBirth = DateTime.Parse("3 May 1972") });
            _context.SaveChanges();

            var result = await personService.SearchAsync("Test p");

            Assert.Equal(2, result.Response.Count());
            Assert.Single(result.Response.Where(r => r.Name == "Test Person"));
            Assert.Single(result.Response.Where(r => r.Name == "Test Person 2"));
        }

        [Fact]
        public async void CanAddPerson()
        {
            var id = await personService.AddAsync(new AddPersonInfo { Name = "Test", DateOfBirth = DateTime.Parse("12 Jan 1998") });

            var added = await _context.People.FirstOrDefaultAsync(p => p.Id == id);
            Assert.Equal("Test", added.Name);
            Assert.Equal(DateTime.Parse("12 Jan 1998"), added.DateOfBirth);
        }

        [Fact]
        public async void GetPersonNotFoundReturnsNull()
        {

            var result = await personService.GetAsync(4);

            Assert.Null(result);
        }

        [Fact]
        public async void CanUpdatePerson()
        {

            var result = await personService.UpdateAsync(new PersonInfo { Id = 3, Name = "Test", DateOfBirth = DateTime.Parse("12 Jan 1998") });

            Assert.Equal(string.Empty,result);
            var added = await _context.People.FirstOrDefaultAsync(p => p.Id == 3);
            Assert.Equal("Test", added.Name);
            Assert.Equal(DateTime.Parse("12 Jan 1998"), added.DateOfBirth);
        }

        [Fact]
        public async void UpdatePersonNotFoundReturnsError()
        {

            var result = await personService.UpdateAsync(new PersonInfo { Id = 2, Name = "Test", DateOfBirth = DateTime.Parse("12 Jan 1998") });

            Assert.Equal("Person not found",result);
        }

        [Fact]
        public async void DeletePersonNoBookings()
        {
            var result = await personService.DeleteAsync(3, false);

            Assert.Equal(string.Empty, result);
            Assert.Null(_context.People.Find(3));
        }

        [Fact]
        public async void DeletePersonAndBookings()
        {
            _context.RoomBookings.Add(new RoomBooking { Id = 1, RoomId = 1, PersonId = 3 });
            _context.SaveChanges();

            var result = await personService.DeleteAsync(3, true);

            Assert.Equal(string.Empty, result);
            Assert.Null(_context.People.Find(3));
        }

        [Fact]
        public async void DeletePersonWithBookingsFails()
        {
            _context.RoomBookings.Add(new RoomBooking { Id = 1, RoomId = 1, PersonId = 3 });
            _context.SaveChanges();

            var result = await personService.DeleteAsync(3, false);

            Assert.Equal("Person has bookings", result);
            Assert.NotNull(_context.People.Find(3));
        }

    }
}
