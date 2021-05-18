using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UKParliament.CodeTest.Data;
using UKParliament.CodeTest.Data.Domain;
using UKParliament.CodeTest.Services.Models;

namespace UKParliament.CodeTest.Services
{
    public interface IBookingService
    {
         Task<AddResponse> AddAsync(BookingInfo bookingInfo);
         Task DeleteAsync(int bookingId);
         Task<SearchResponse<RoomInfo>> FindAvailableRoomsAsync(DateTime startTime, int duration);
    }

    public class BookingService : IBookingService
    {

        private readonly IMapper _mapper;
        private readonly RoomBookingsContext _roomBookingsContext;

        public BookingService(IMapper mapper, RoomBookingsContext roomBookingsContext)
        {
            _mapper = mapper;
            _roomBookingsContext = roomBookingsContext;
        }

        public async Task<AddResponse> AddAsync(BookingInfo bookingInfo)
        {
            var response = new AddResponse { Id = -1 };
            if (bookingInfo.DurationMinutes < 1 || bookingInfo.DurationMinutes > 60)
            {
                response.ErrorMessage = "Duration must be between 1 and 60 minutes";
                return response;
            }
            var person = await _roomBookingsContext.People.FindAsync(bookingInfo.PersonId);
            if (person == null)
            {
                response.ErrorMessage = "Person not found";
                return response;
            }
            var room = await _roomBookingsContext.Rooms.FindAsync(bookingInfo.RoomId);
            if (room == null)
            {
                response.ErrorMessage = "Room not found";
                return response;
            }
            var endTime = bookingInfo.StartTime.AddMinutes(bookingInfo.DurationMinutes);
            if (await _roomBookingsContext.RoomBookings.AnyAsync(booking => booking.RoomId == bookingInfo.RoomId &&
                                                                            booking.EndTime > bookingInfo.StartTime &&
                                                                            booking.StartTime < endTime ))
            {
                response.ErrorMessage = "Overlaps existing booking";
                return response;
            }

            var booking = _mapper.Map<RoomBooking>(bookingInfo);
            booking.EndTime = endTime;
            await _roomBookingsContext.RoomBookings.AddAsync(booking);
            await _roomBookingsContext.SaveChangesAsync();
            response.Id = booking.Id;
            return response;

        }

        public async Task DeleteAsync(int bookingId)
        {
            var booking = _roomBookingsContext.RoomBookings.Find(bookingId);
            if (booking != null)
            {
                _roomBookingsContext.RoomBookings.Remove(booking);
                await _roomBookingsContext.SaveChangesAsync();
            }
            
        }

        public async Task<SearchResponse<RoomInfo>> FindAvailableRoomsAsync(DateTime startTime, int duration)
        {
            var response = new SearchResponse<RoomInfo>();
            if (duration < 1)
            {
                response.ErrorMessage = "Search duration cannot be less than 1 minute";
                return response;
            }
            var endTime = startTime.AddMinutes(duration);
            var bookings = _roomBookingsContext.RoomBookings.Where(booking => booking.EndTime > startTime &&
                                                                              booking.StartTime < endTime);
            response.Response = _mapper.Map<List<RoomInfo>>(await _roomBookingsContext.Rooms
                                                                  .Where(room => !bookings.Any(b => b.RoomId == room.Id))
                                                                  .ToListAsync());
            return response;
        }

    }
}