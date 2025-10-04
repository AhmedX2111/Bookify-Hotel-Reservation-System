using Bookify.Application.Business.Dtos.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Services
{
	public interface IReservationCartService
	{
		//Task<ReservationCartDto> GetCartAsync();
		//Task AddToCartAsync(ReservationCartItemDto item);
		//Task RemoveFromCartAsync(Guid roomId);
		//Task ClearCartAsync();
		//Task<bool> ConfirmBookingAsync();

		//Task AddToCartAsync(string userId, ReservationCartItemDto item);
		//Task<ReservationCartDto> GetCartAsync(string userId);
		//Task ClearCartAsync(string userId);
		//Task RemoveFromCartAsync(string userId, int roomId);
		//Task<bool> ConfirmBookingAsync(string userId);

		Task<ReservationCartDto> GetCartAsync(string sessionId);
		Task AddToCartAsync(string sessionId, ReservationCartItemDto item);
		Task RemoveFromCartAsync(string sessionId, int roomId);
		Task ClearCartAsync(string sessionId);
		Task<bool> ConfirmBookingAsync(string sessionId, string userId);
	}
}
