using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UKParliament.CodeTest.Data.Domain;
using UKParliament.CodeTest.Services;
using UKParliament.CodeTest.Services.Models;

namespace UKParliament.CodeTest.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : RoomBookingsControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add([FromBody()] BookingInfo booking)
        {
            // Convert all supplied booking date/times to UTC to ensure consistency
            booking.StartTime = booking.StartTime.ToUniversalTime();
            var response = await _bookingService.AddAsync(booking);
            return HandleResponseBadRequest(response.ErrorMessage, response.Id);
        }

        [HttpDelete("{bookingId}")]
        public async Task<ActionResult> Delete(int bookingId)
        {
            await _bookingService.DeleteAsync(bookingId);
            return Ok();
        }

        [Route("AvailableRooms")]
        [HttpGet]
        public async Task<ActionResult<List<RoomInfo>>> GetAvailableRooms(DateTime StartTime, int durationMinutes)
        {
            var response = await _bookingService.FindAvailableRoomsAsync(StartTime.ToUniversalTime(), durationMinutes);
            return HandleResponseBadRequest(response.ErrorMessage, response.Response);
        }
    }
}
