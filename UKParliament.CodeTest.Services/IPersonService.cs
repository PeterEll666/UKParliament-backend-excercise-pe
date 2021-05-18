using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UKParliament.CodeTest.Data;
using UKParliament.CodeTest.Data.Domain;
using UKParliament.CodeTest.Services.Models;

namespace UKParliament.CodeTest.Services
{
    public interface IPersonService
    {
        Task<PersonInfo> GetAsync(int personId);
        Task<SearchResponse<PersonInfo>> SearchAsync(string searchName);
        Task<int> AddAsync(AddPersonInfo personInfo);
        Task<string> UpdateAsync(PersonInfo personInfo);
        Task<string> DeleteAsync(int personId, bool deleteBookings);
    }

    public class PersonService : IPersonService
    {
        private readonly IMapper _mapper;
        private readonly RoomBookingsContext _roomBookingsContext;

        public PersonService(IMapper mapper, RoomBookingsContext roomBookingsContext)
        {
            _mapper = mapper;
            _roomBookingsContext = roomBookingsContext;
        }

        public async Task<PersonInfo> GetAsync(int personId)
        {
            var person = await _roomBookingsContext.People.FindAsync(personId);
            if (person!=null)
            {
                return _mapper.Map<PersonInfo>(person);
            }
            return null;
        }

        public async Task<SearchResponse<PersonInfo>> SearchAsync(string searchName)
        {
            var response = new SearchResponse<PersonInfo>();
            if(string.IsNullOrEmpty(searchName))
            {
                response.ErrorMessage = "Search name cannot be empty";
                return response;
            }
            response.Response = _mapper.Map<List<PersonInfo>>(await _roomBookingsContext.People
                                                       .Where(person => person.Name.ToUpper().Contains(searchName.ToUpper())).ToListAsync());
            return response;
        }

        public async Task<int> AddAsync(AddPersonInfo personInfo)
        {
            var person = _mapper.Map<Person>(personInfo);
            person.DateOfBirth = person.DateOfBirth.Date;
            await _roomBookingsContext.People.AddAsync(person);
            await _roomBookingsContext.SaveChangesAsync();
            return person.Id;
        }

        public async Task<string> UpdateAsync(PersonInfo personInfo)
        {
            var person = await _roomBookingsContext.People.FindAsync(personInfo.Id);
            if (person == null)
            {
                return "Person not found";
            }
            _mapper.Map(personInfo, person);
            person.DateOfBirth = person.DateOfBirth.Date;
            await _roomBookingsContext.SaveChangesAsync();
            return string.Empty;
        }

        public async Task<string> DeleteAsync(int personId, bool deleteBookings)
        {
            var person = await _roomBookingsContext.People.Include("Bookings").FirstOrDefaultAsync(Person => Person.Id == personId);
            if (person == null)
            {
                return string.Empty;
            }

            if (person.Bookings.Count > 0)
            {
                if (deleteBookings)
                {
                    person.Bookings.Clear();
                }
                else
                {
                    return "Person has bookings";
                }
            }
            _roomBookingsContext.People.Remove(person);
            _roomBookingsContext.SaveChanges();
            return string.Empty;
        }
    }
}