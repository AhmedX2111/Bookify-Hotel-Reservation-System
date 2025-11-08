using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Bookify.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly UserManager<User> _userManager;

        public UserService(IBookingRepository bookingRepository, UserManager<User> userManager)
        {
            _bookingRepository = bookingRepository;
            _userManager = userManager;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with ID {userId} not found.");

            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            var bookingHistory = bookings.Select(b => new BookingHistoryDto
            {
                Id = b.Id,
                RoomType = b.Room?.RoomType?.Name ?? "N/A",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalAmount = b.TotalCost,
                Status = b.Status
            }).ToList();

            return new UserProfileDto
            {
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                Bookings = bookingHistory
            };
        }

        public async Task<IEnumerable<BookingHistoryDto>> GetUserBookingHistoryAsync(string userId, CancellationToken cancellationToken = default)
        {
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            return bookings.Select(b => new BookingHistoryDto
            {
                Id = b.Id,
                RoomType = b.Room?.RoomType?.Name ?? "N/A",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalAmount = b.TotalCost,
                Status = b.Status
            }).ToList();
        }
    }
}
