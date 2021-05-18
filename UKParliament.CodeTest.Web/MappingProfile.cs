using AutoMapper;
using UKParliament.CodeTest.Data.Domain;
using UKParliament.CodeTest.Services.Models;

namespace UKParliament.CodeTest.Web
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PersonInfo, Person>();
            CreateMap<Person, PersonInfo>();
            CreateMap<AddPersonInfo, Person>();
            CreateMap<Room, RoomInfo>();
            CreateMap<RoomInfo, Room>();
            CreateMap<AddRoomInfo, Room>();
            CreateMap<BookingInfo, RoomBooking>();
            CreateMap<RoomBooking, BookingInfo>();
        }
    }
}
