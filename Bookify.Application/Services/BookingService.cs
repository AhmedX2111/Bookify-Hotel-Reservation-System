// Bookify-Hotel-Reservation-System-master/Bookify.Application/Services/BookingService.cs
using AutoMapper; // Added
using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json; // Install-Package Newtonsoft.Json
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Services
{
    public class BookingService : IBookingService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper; // Added
        private const string CartSessionKey = "ReservationCart";

        public BookingService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IMapper mapper) // Updated constructor
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session ?? throw new InvalidOperationException("Session is not available.");

        private List<BookingCreateDto> GetCurrentCartItems(string userId)
        {
            var cartJson = Session.GetString($"{CartSessionKey}_{userId}");
            return cartJson == null ? new List<BookingCreateDto>() : JsonConvert.DeserializeObject<List<BookingCreateDto>>(cartJson);
        }

        private void SaveCartItems(string userId, List<BookingCreateDto> cart)
        {
            Session.SetString($"{CartSessionKey}_{userId}", JsonConvert.SerializeObject(cart));
        }

        // --- Reservation Cart Functionality ---
        public async Task<bool> AddToReservationCartAsync(string userId, BookingCreateDto bookingDetails, CancellationToken cancellationToken = default)
        {
            var cart = GetCurrentCartItems(userId);

            // Basic validation: Check if room is already in cart for the same dates
            if (cart.Any(b => b.RoomId == bookingDetails.RoomId &&
                              b.CheckInDate.Date == bookingDetails.CheckInDate.Date &&
                              b.CheckOutDate.Date == bookingDetails.CheckOutDate.Date))
            {
                return false; // Already in cart
            }

            // Fetch room details for display in cart
            var room = await _unitOfWork.Rooms.GetRoomByIdWithTypeAsync(bookingDetails.RoomId, cancellationToken);
            if (room == null || !room.IsAvailable) return false; // Room not found or not available

            // Populate display properties
            bookingDetails.RoomNumber = room.RoomNumber;
            bookingDetails.RoomTypeName = room.RoomType.Name;
            bookingDetails.PricePerNight = room.RoomType.PricePerNight;
            bookingDetails.Capacity = room.RoomType.Capacity;
            bookingDetails.ImageUrl = room.RoomType.ImageUrl;

            cart.Add(bookingDetails);
            SaveCartItems(userId, cart);
            return true;
        }

        public Task<List<BookingCreateDto>> GetReservationCartAsync(string userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetCurrentCartItems(userId));
        }

        public Task<bool> RemoveFromReservationCartAsync(string userId, int roomId, CancellationToken cancellationToken = default)
        {
            var cart = GetCurrentCartItems(userId);
            var initialCount = cart.Count;
            cart.RemoveAll(b => b.RoomId == roomId);
            if (cart.Count < initialCount)
            {
                SaveCartItems(userId, cart);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task ClearReservationCartAsync(string userId, CancellationToken cancellationToken = default)
        {
            Session.Remove($"{CartSessionKey}_{userId}");
            return Task.CompletedTask;
        }

        // --- Admin Booking Management ---
        public async Task<IEnumerable<BookingDto>> GetAllBookingsAdminAsync(
            string? status = null,
            DateTime? checkInFrom = null,
            DateTime? checkInTo = null,
            CancellationToken cancellationToken = default)
        {
            var bookings = await _unitOfWork.Bookings.GetAllBookingsWithDetailsAsync(status, checkInFrom, checkInTo, cancellationToken);
            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }

        public async Task<BookingDto?> GetBookingByIdAdminAsync(int id, CancellationToken cancellationToken = default)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id, cancellationToken);
            if (booking == null) return null;

            // To map to BookingDto, we need User, Room, and RoomType details.
            // The GetByIdAsync in generic repository doesn't include navigation properties by default.
            // We need a specific repository method or eager load here.
            // For simplicity, let's assume GetAllBookingsWithDetailsAsync can be adapted or we fetch separately.
            // A better approach would be to add a GetBookingWithDetailsByIdAsync to IBookingRepository.
            var allBookings = await _unitOfWork.Bookings.GetAllBookingsWithDetailsAsync(cancellationToken: cancellationToken);
            var detailedBooking = allBookings.FirstOrDefault(b => b.Id == id);

            return _mapper.Map<BookingDto>(detailedBooking);
        }

        public async Task<bool> UpdateBookingStatusAdminAsync(int bookingId, string newStatus, CancellationToken cancellationToken = default)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null) return false;

            // Basic validation for status (can be expanded)
            var allowedStatuses = new[] { "Pending", "Confirmed", "Cancelled", "Completed" };
            if (!allowedStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid booking status: {newStatus}. Allowed statuses are: {string.Join(", ", allowedStatuses)}");
            }

            booking.Status = newStatus;
            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteBookingAdminAsync(int id, CancellationToken cancellationToken = default)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id, cancellationToken);
            if (booking == null) return false;

            _unitOfWork.Bookings.Delete(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
