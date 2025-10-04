using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Services
{
	public class ReservationCartService : IReservationCartService
	{

		//private readonly IHttpContextAccessor _httpContextAccessor;
		//private readonly IUnitOfWork _unitOfWork;

		//public ReservationCartService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
		//{
		//	_httpContextAccessor = httpContextAccessor;
		//	_unitOfWork = unitOfWork;
		//}

		//private string GetCartKey(string sessionId) => $"Cart_{sessionId}";

		//public Task<ReservationCartDto> GetCartAsync(string sessionId)
		//{
		//	var session = _httpContextAccessor.HttpContext?.Session;
		//	if (session == null)
		//		return Task.FromResult(new ReservationCartDto());

		//	var cartJson = session.GetString(GetCartKey(sessionId));
		//	if (string.IsNullOrEmpty(cartJson))
		//		return Task.FromResult(new ReservationCartDto());

		//	var cart = JsonSerializer.Deserialize<ReservationCartDto>(cartJson) ?? new ReservationCartDto();
		//	return Task.FromResult(cart);
		//}

		//public async Task AddToCartAsync(string sessionId, ReservationCartItemDto item)
		//{
		//	var cart = await GetCartAsync(sessionId);
		//	cart.Items.RemoveAll(x => x.RoomId == item.RoomId); // overwrite if exists
		//	cart.Items.Add(item);
		//	SaveCart(sessionId, cart);
		//}

		//public async Task RemoveFromCartAsync(string sessionId, int roomId)
		//{
		//	var cart = await GetCartAsync(sessionId);
		//	var item = cart.Items.FirstOrDefault(i => i.RoomId == roomId);
		//	if (item != null)
		//	{
		//		cart.Items.Remove(item);
		//		SaveCart(sessionId, cart);
		//	}
		//}

		//public Task ClearCartAsync(string sessionId)
		//{
		//	var session = _httpContextAccessor.HttpContext?.Session;
		//	session?.Remove(GetCartKey(sessionId));
		//	return Task.CompletedTask;
		//}

		///// <summary>
		///// Persists cart items into DB as Bookings and clears session cart.
		///// </summary>
		//public async Task<bool> ConfirmBookingAsync(string sessionId)
		//{
		//	var cart = await GetCartAsync(sessionId);
		//	if (!cart.Items.Any())
		//		return false;

		//	var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

		//	foreach (var item in cart.Items)
		//	{
		//		var booking = new Booking
		//		{
		//			UserId = userId, // null if guest, but in your domain this may be required
		//			RoomId = item.RoomId,
		//			CheckInDate = item.CheckInDate,
		//			CheckOutDate = item.CheckOutDate,
		//			NumberOfNights = (item.CheckOutDate - item.CheckInDate).Days,
		//			TotalCost = item.TotalPrice,
		//			Status = "Pending",
		//			CreatedAt = DateTime.UtcNow
		//		};

		//		await _unitOfWork.Bookings.AddAsync(booking);
		//	}

		//	await _unitOfWork.SaveChangesAsync();

		//	// clear cart after persisting
		//	await ClearCartAsync(sessionId);

		//	return true;
		//}

		//private void SaveCart(string sessionId, ReservationCartDto cart)
		//{
		//	var session = _httpContextAccessor.HttpContext?.Session;
		//	if (session != null)
		//	{
		//		var cartJson = JsonSerializer.Serialize(cart);
		//		session.SetString(GetCartKey(sessionId), cartJson);
		//	}
		//}

		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<User> _userManager;
		private const string CartKeyPrefix = "ReservationCart_";

		public ReservationCartService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, UserManager<User> userManager)
		{
			_httpContextAccessor = httpContextAccessor;
			_unitOfWork = unitOfWork;
			_userManager = userManager;
		}

		private ISession Session => _httpContextAccessor.HttpContext!.Session;

		private string GetCartKey(string sessionId) => $"{CartKeyPrefix}{sessionId}";

		private ReservationCartDto LoadCart(string sessionId)
		{
			var data = Session.GetString(GetCartKey(sessionId));
			if (string.IsNullOrEmpty(data))
				return new ReservationCartDto();

			return JsonSerializer.Deserialize<ReservationCartDto>(data) ?? new ReservationCartDto();
		}


		public Task<ReservationCartDto> GetCartAsync(string sessionId)
		{
			var cart = LoadCart(sessionId);
			return Task.FromResult(cart);
		}

		public Task AddToCartAsync(string sessionId, ReservationCartItemDto item)
		{
			var cart = LoadCart(sessionId);

			var existing = cart.Items.FirstOrDefault(i =>
				i.RoomId == item.RoomId &&
				i.CheckInDate == item.CheckInDate &&
				i.CheckOutDate == item.CheckOutDate);

			if (existing != null)
			{
				existing.NumberOfGuests += item.NumberOfGuests;
				existing.TotalPrice += item.TotalPrice;
			}
			else
			{
				cart.Items.Add(item);
			}

			cart.TotalAmount = cart.Items.Sum(i => i.TotalPrice);
			SaveCart(sessionId, cart);

			return Task.CompletedTask;
		}

		public Task RemoveFromCartAsync(string sessionId, int roomId)
		{
			var cart = LoadCart(sessionId);
			cart.Items.RemoveAll(i => i.RoomId == roomId);
			cart.TotalAmount = cart.Items.Sum(i => i.TotalPrice);
			SaveCart(sessionId, cart);

			return Task.CompletedTask;
		}

		public Task ClearCartAsync(string sessionId)
		{
			Session.Remove(GetCartKey(sessionId));
			return Task.CompletedTask;
		}

		public async Task<bool> ConfirmBookingAsync(string sessionId, string userId)
		{
			try
			{
				Console.WriteLine($"=== SERVICE: ConfirmBookingAsync ===");
				Console.WriteLine($"Service received - SessionId: {sessionId}");
				Console.WriteLine($"Service received - UserId: '{userId}'");

				// Get cart items first
				var cart = await GetCartAsync(sessionId);
				Console.WriteLine($"Cart items count: {cart.Items.Count}");

				if (!cart.Items.Any())
				{
					Console.WriteLine("❌ Cart is empty");
					return false;
				}

				// Double-check user exists
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
				{
					Console.WriteLine($"❌ SERVICE: User '{userId}' not found in database!");
					return false;
				}

				Console.WriteLine($"✅ SERVICE: User confirmed - {user.UserName}");

				// 🔧 ADDED: Validate rooms exist before creating bookings
				var roomIds = cart.Items.Select(item => item.RoomId).Distinct().ToList();
				var allRooms = await _unitOfWork.Rooms.GetAllAsync();
				var existingRooms = allRooms.Where(r => roomIds.Contains(r.Id)).ToList();
				var existingRoomIds = existingRooms.Select(r => r.Id).ToList();

				Console.WriteLine($"🔍 Room validation:");
				Console.WriteLine($"  - Requested RoomIds: {string.Join(", ", roomIds)}");
				Console.WriteLine($"  - Existing RoomIds: {string.Join(", ", existingRoomIds)}");

				// Log all available rooms for better debugging
				Console.WriteLine($"  - All available RoomIds: {string.Join(", ", allRooms.Select(r => r.Id))}");

				var missingRooms = roomIds.Except(existingRoomIds).ToList();
				if (missingRooms.Any())
				{
					Console.WriteLine($"❌ Rooms not found in database: {string.Join(", ", missingRooms)}");

					// 🔧 ADDED: Optionally remove invalid rooms from cart
					foreach (var invalidRoomId in missingRooms)
					{
						Console.WriteLine($"  - Removing invalid RoomId {invalidRoomId} from cart");
						await RemoveFromCartAsync(sessionId, invalidRoomId);
					}

					return false;
				}

				// 🔧 ADDED: Validate room availability (optional - if you have availability logic)
				foreach (var item in cart.Items)
				{
					var room = existingRooms.First(r => r.Id == item.RoomId);
					if (room != null && !room.IsAvailable)
					{
						Console.WriteLine($"❌ Room {item.RoomId} is not available");
						return false;
					}
				}

				Console.WriteLine($"✅ All rooms validated successfully");

				// Create bookings
				foreach (var item in cart.Items)
				{
					var booking = new Booking
					{
						UserId = userId,
						RoomId = item.RoomId,
						CheckInDate = item.CheckInDate,
						CheckOutDate = item.CheckOutDate,
						NumberOfNights = item.NumberOfNights, 
						TotalCost = item.TotalPrice,
						Status = "Confirmed",
						CreatedAt = DateTime.UtcNow
					};

					Console.WriteLine($"Creating booking:");
					Console.WriteLine($"  - UserId: {booking.UserId}");
					Console.WriteLine($"  - RoomId: {booking.RoomId}");
					Console.WriteLine($"  - CheckIn: {booking.CheckInDate:yyyy-MM-dd}");
					Console.WriteLine($"  - CheckOut: {booking.CheckOutDate:yyyy-MM-dd}");
					Console.WriteLine($"  - Nights: {booking.NumberOfNights}");
					Console.WriteLine($"  - TotalCost: {booking.TotalCost}");
				}

				Console.WriteLine("Saving changes to database...");
				await _unitOfWork.SaveChangesAsync();
				Console.WriteLine("✅ BOOKINGS SAVED SUCCESSFULLY!");

				// 🔧 ADDED: Clear cart only after successful booking creation
				await ClearCartAsync(sessionId);
				Console.WriteLine("✅ Cart cleared after successful booking");

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ SERVICE EXCEPTION: {ex.Message}");
				Console.WriteLine($"❌ SERVICE STACK TRACE: {ex.StackTrace}");

				if (ex.InnerException != null)
				{
					Console.WriteLine($"❌ INNER EXCEPTION: {ex.InnerException.Message}");

					
				}

				throw;
			}
		}
		private void SaveCart(string sessionId, ReservationCartDto cart)
		{
			var session = _httpContextAccessor.HttpContext?.Session;
			if (session != null)
			{
				var cartJson = JsonSerializer.Serialize(cart);
				session.SetString(GetCartKey(sessionId), cartJson);
			}
		}
	}
}
