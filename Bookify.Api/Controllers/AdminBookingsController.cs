using Bookify.Application.Business.Interfaces;
using Bookify.Application.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class AdminBookingsController : ControllerBase
    {
        private readonly IAdminBookingService _service;
        private readonly IBookingService _bookingService;

        public AdminBookingsController(IAdminBookingService service, IBookingService bookingService)
        {
            _service = service;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var (bookings, totalCount) = await _service.GetPagedAsync(pageNumber, pageSize, search, cancellationToken);
            return Ok(new { totalCount, bookings });
        }

        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
        {
            await _service.ApproveAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            await _service.CancelAsync(id, cancellationToken);
            return NoContent();
        }
        [HttpPost("update-statuses")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatuses(CancellationToken cancellationToken)
        {
            await _bookingService.UpdateCompletedBookingsAsync(cancellationToken);
            return Ok(new { Message = "Completed bookings updated successfully." });
        }

    }
}
