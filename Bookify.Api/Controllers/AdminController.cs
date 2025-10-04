using Bookify.Application.Business.Interfaces.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize(Roles = "Admin,Manager")]
	public class AdminController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;

		public AdminController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[HttpGet("dashboard")]
		public async Task<IActionResult> Dashboard()
		{
			var rooms = await _unitOfWork.Rooms.GetAllAsync();
			var bookings = await _unitOfWork.Bookings.GetAllAsync();

			var stats = new
			{
				TotalRooms = rooms.Count(),
				TotalBookings = bookings.Count(),
				PendingBookings = bookings.Count(b => b.Status == "Pending"),
				ConfirmedBookings = bookings.Count(b => b.Status == "Confirmed")
			};

			return Ok(stats);
		}

		[HttpPost("approve-booking/{id}")]
		public async Task<IActionResult> ApproveBooking(int id)
		{
			var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
			if (booking == null) return NotFound();

			booking.Status = "Confirmed";
			_unitOfWork.Bookings.Update(booking);
			await _unitOfWork.SaveChangesAsync();

			return Ok(new { message = "Booking approved" });
		}

		[HttpPost("cancel-booking/{id}")]
		public async Task<IActionResult> CancelBooking(int id)
		{
			var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
			if (booking == null) return NotFound();

			booking.Status = "Cancelled";
			_unitOfWork.Bookings.Update(booking);
			await _unitOfWork.SaveChangesAsync();

			return Ok(new { message = "Booking cancelled" });
		}
	}
}
