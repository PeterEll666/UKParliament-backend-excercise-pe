using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UKParliament.CodeTest.Services;
using UKParliament.CodeTest.Services.Models;

namespace UKParliament.CodeTest.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : RoomBookingsControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet("{roomId}")]
        public async Task<ActionResult<RoomInfo>> Get(int roomId)
        {
            return await _roomService.GetAsync(roomId) ?? new ActionResult<RoomInfo>(NotFound()); 
        }

        [Route("Search")]
        [HttpGet]
        public async Task<ActionResult<List<RoomInfo>>> Search(string searchName)
        {
            var response = await _roomService.SearchAsync(searchName);
            return HandleResponseBadRequest(response.ErrorMessage, response.Response);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add([FromBody()] AddRoomInfo room)
        {
            var response = await _roomService.AddAsync(room);
            return HandleResponseBadRequest(response.ErrorMessage, response.Id);
        }

        [HttpPut]
        public async Task<ActionResult<int>> Update([FromBody()] RoomInfo room)
        {
            return HandleResponseNotFound(await _roomService.UpdateAsync(room));
        }

        [HttpDelete("{roomId}")]
        public async Task<ActionResult> Delete(int roomId, int shiftToRoomId)
        {
            return HandleResponseBadRequest(await _roomService.DeleteAsync(roomId, shiftToRoomId));
        }
    }
}
