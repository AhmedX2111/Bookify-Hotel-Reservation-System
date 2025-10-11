using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;

        public AdminController(IBookingService bookingService, IRoomService roomService)
        {
            _bookingService = bookingService;
            _roomService = roomService;
        }

     
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllBookings(CancellationToken cancellationToken)
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync(cancellationToken);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
          
                return StatusCode(500, "An error occurred while retrieving all bookings.");
            }
        }



    }
}