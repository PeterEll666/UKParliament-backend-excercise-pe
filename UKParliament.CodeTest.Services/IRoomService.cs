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
    public interface IRoomService
    {
        Task<RoomInfo> GetAsync(int roomId);
        Task<SearchResponse<RoomInfo>> SearchAsync(string searchName);
        Task<AddResponse> AddAsync(AddRoomInfo roomInfo);
        Task<string> UpdateAsync(RoomInfo roomInfo);
        Task<string> DeleteAsync(int roomId, int shiftToRoomId);
    }

    public class RoomService : IRoomService
    {
        private readonly IMapper _mapper;
        private readonly RoomBookingsContext _roomBookingsContext;

        public RoomService(IMapper mapper, RoomBookingsContext roomBookingsContext)
        {
            _mapper = mapper;
            _roomBookingsContext = roomBookingsContext;
        }

        public async Task<RoomInfo> GetAsync(int roomId)
        {
            var room = await _roomBookingsContext.Rooms.FindAsync(roomId);
            if (room!=null)
            {
                return _mapper.Map<RoomInfo>(room);
            }
            return null;
        }

        public async Task<SearchResponse<RoomInfo>> SearchAsync(string searchName)
        {
            var response = new SearchResponse<RoomInfo>();
            if (string.IsNullOrEmpty(searchName))
            {
                response.ErrorMessage = "Search name cannot be empty";
                return response;
            }
            response.Response = _mapper.Map<List<RoomInfo>>(await _roomBookingsContext.Rooms
                                                                  .Where(room => room.Name.ToUpper().Contains(searchName.ToUpper())).ToListAsync());
            return response;
        }

        public async Task<AddResponse> AddAsync(AddRoomInfo roomInfo)
        {
            var response = new AddResponse { Id = -1 };
            if (await _roomBookingsContext.Rooms.AnyAsync(Room => Room.Name == roomInfo.Name))
            {
                response.ErrorMessage = "Room already exists";
            }
            var room = _mapper.Map<Room>(roomInfo);
            await _roomBookingsContext.Rooms.AddAsync(room);
            await _roomBookingsContext.SaveChangesAsync();
            response.Id = room.Id;
            return response;
        }

        public async Task<string> UpdateAsync(RoomInfo roomInfo)
        {
            var room = await _roomBookingsContext.Rooms.FindAsync(roomInfo.Id);
            if (room == null)
            {
                return "Room not found";
            }
            _mapper.Map(roomInfo,room);
            await _roomBookingsContext.SaveChangesAsync();
            return string.Empty;
        }

        public async Task<string> DeleteAsync(int roomId, int shiftToRoomId)
        {
            var room = await _roomBookingsContext.Rooms.Include("Bookings").FirstOrDefaultAsync(room => room.Id == roomId);
            if (room == null)
            {
                return string.Empty;
            }
            if (shiftToRoomId > 0)
            {
                if (await _roomBookingsContext.Rooms.FindAsync(shiftToRoomId) == null)
                {
                    return "Shift to room not found";
                }
                else
                {
                    foreach (var booking in room.Bookings)
                    {
                        booking.RoomId = shiftToRoomId;
                    }
                }
            }
            else
            {
                room.Bookings.Clear();
            }
            await _roomBookingsContext.SaveChangesAsync();
            return string.Empty;
        }
    }
}