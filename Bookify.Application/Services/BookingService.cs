﻿using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Bookify.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Business.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;

        public BookingService(IUnitOfWork unitOfWork, IBookingRepository bookingRepository, IRoomRepository roomRepository)
        {
            _unitOfWork = unitOfWork;
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
        }

        // Implement all required interface methods
        public async Task<BookingDto> CreateBookingAsync(string userId, BookingCreateDto bookingDto, CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetByIdAsync(bookingDto.RoomId);
            if (room == null)
                throw new NotFoundException($"Room with ID {bookingDto.RoomId} not found.");

            var isAvailable = await IsRoomAvailableAsync(bookingDto.RoomId, bookingDto.CheckInDate, bookingDto.CheckOutDate);
            if (!isAvailable)
                throw new ValidationException("Room is not available for the selected dates.");

            if (bookingDto.CheckInDate >= bookingDto.CheckOutDate)
                throw new ValidationException("Check-out date must be after check-in date.");

            if (bookingDto.CheckInDate.Date <= DateTime.Today)
                throw new ValidationException("Check-in date must be in the future.");

            var numberOfNights = (int)(bookingDto.CheckOutDate - bookingDto.CheckInDate).TotalDays;
            var totalCost = numberOfNights * (room.RoomType?.PricePerNight ?? 100m);

            var booking = new Booking
            {
                UserId = userId,
                RoomId = bookingDto.RoomId,
                CheckInDate = bookingDto.CheckInDate,
                CheckOutDate = bookingDto.CheckOutDate,
                TotalCost = totalCost,
                Status ="Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDto(booking);
        }

        public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync(CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return bookings.Select(MapToDto);
        }

        public async Task<BookingDto> GetBookingByIdAsync(int id, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new NotFoundException($"Booking with ID {id} not found.");

            return MapToDto(booking);
        }

        public async Task<bool> CancelBookingAsync(int id, string userId, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new NotFoundException($"Booking with ID {id} not found.");

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own bookings.");

            if (booking.Status == "Cancelled")
                throw new ValidationException("Booking is already cancelled.");

            if (booking.Status == "Completed")
                throw new ValidationException("Cannot cancel a completed booking.");

            if (booking.Status == "Active")
                throw new ValidationException("Cannot cancel an active booking. Please contact support.");

            var (refundAmount, cancellationFee) = CalculateRefund(booking);

            booking.Status = "Cancelled";
            booking.CancellationReason = "User cancelled";
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.RefundAmount = refundAmount;
            booking.CancellationFee = cancellationFee;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var overlappingBookings = await _bookingRepository.GetOverlappingBookingsAsync(roomId, checkIn, checkOut);

            return !overlappingBookings.Any(b => b.Status != "Cancelled" && b.Status != "Rejected");
        }

        public async Task<bool> RejectBookingAsync(int id, string reason, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new NotFoundException($"Booking with ID {id} not found.");

            if (booking.Status != "Pending")
                throw new ValidationException($"Cannot reject booking with status: {booking.Status}");

            booking.Status = "Rejected";
            booking.RejectionReason = reason;
            booking.RejectedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true; // Just return true if successful
        }

        public async Task<BookingStatusInfo> GetBookingStatusAsync(int id, string userId, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new NotFoundException($"Booking with ID {id} not found.");

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("Access denied to this booking.");

            return new BookingStatusInfo
            {
                Status = booking.Status.ToString(),
                CanCancel = CanCancelBooking(booking),
                CancellationPolicy = GetCancellationPolicy(booking),
                ConfirmedAt = booking.ConfirmedAt,
                CancelledAt = booking.CancelledAt
            };
        }

        private string GetCancellationPolicy(Booking booking)
        {
            var daysUntilCheckIn = (booking.CheckInDate - DateTime.Today).Days;

            if (daysUntilCheckIn >= 7)
                return "Free cancellation up to 7 days before check-in";
            else if (daysUntilCheckIn >= 3)
                return "50% refund for cancellations within 3-7 days";
            else if (daysUntilCheckIn >= 1)
                return "20% refund for cancellations within 1-3 days";
            else
                return "No refund for same-day cancellations";
        }

        public async Task<BookingDto?> GetBookingByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                return null;

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("Access denied to this booking.");

            return MapToDto(booking);
        }

        public async Task<CancellationResult> CancelBookingWithReasonAsync(int id, string userId, string reason, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return new CancellationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Booking with ID {id} not found.",
                    RefundAmount = 0,
                    CancellationFee = 0
                };
            }

            if (booking.UserId != userId)
            {
                return new CancellationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "You can only cancel your own bookings.",
                    RefundAmount = 0,
                    CancellationFee = 0
                };
            }

            if (booking.Status == "Cancelled")
            {
                return new CancellationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Booking is already cancelled.",
                    RefundAmount = 0,
                    CancellationFee = 0
                };
            }

            if (booking.Status == "Completed")
            {
                return new CancellationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Cannot cancel a completed booking.",
                    RefundAmount = 0,
                    CancellationFee = 0
                };
            }

            if (booking.Status == "Active")
            {
                return new CancellationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Cannot cancel an active booking. Please contact support.",
                    RefundAmount = 0,
                    CancellationFee = 0
                };
            }

            var (refundAmount, cancellationFee) = CalculateRefund(booking);

            booking.Status = "Cancelled";
            booking.CancellationReason = reason;
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.RefundAmount = refundAmount;
            booking.CancellationFee = cancellationFee;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CancellationResult
            {
                IsSuccess = true,
                ErrorMessage = string.Empty, // Clear error on success
                RefundAmount = refundAmount,
                CancellationFee = cancellationFee
            };
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, CancellationToken cancellationToken = default)
        {
            var overlappingBookings = await _bookingRepository.GetOverlappingBookingsAsync(roomId, checkIn, checkOut);

            // Room is available if there are no overlapping bookings that are active (not cancelled or rejected)
            return !overlappingBookings.Any(b => b.Status != "Cancelled" && b.Status != "Rejected");
        }

        public async Task<CancellationResult> CancelBookingAsync(int id, string userId, string reason, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new NotFoundException($"Booking with ID {id} not found.");

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own bookings.");

            if (booking.Status == "Cancelled")
                throw new ValidationException("Booking is already cancelled.");

            if (booking.Status == "Completed")
                throw new ValidationException("Cannot cancel a completed booking.");

            if (booking.Status == "Active")
                throw new ValidationException("Cannot cancel an active booking. Please contact support.");

            var (refundAmount, cancellationFee) = CalculateRefund(booking);

            booking.Status = "Cancelled";
            booking.CancellationReason = reason; // Use the passed reason
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.RefundAmount = refundAmount;
            booking.CancellationFee = cancellationFee;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CancellationResult
            {
                IsSuccess = true,
                RefundAmount = refundAmount,
                CancellationFee = cancellationFee
            };
        }

        async Task<ConfirmationResult> IBookingService.ConfirmBookingAsync(int bookingId, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return new ConfirmationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Booking with ID {bookingId} not found."
                };
            }

            if (booking.Status != "Pending")
            {
                return new ConfirmationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Cannot confirm booking with status: {booking.Status}"
                };
            }

            bool isAvailable = await IsRoomAvailableAsync(booking.RoomId, booking.CheckInDate, booking.CheckOutDate, cancellationToken);
            if (!isAvailable)
            {
                return new ConfirmationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Room is no longer available for the selected dates."
                };
            }

            booking.Status = "Confirmed";
            booking.ConfirmedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return the booking details as DTO
            var bookingDto = MapToDto(booking);

            return new ConfirmationResult
            {
                IsSuccess = true,
                BookingDetails = bookingDto,
                ErrorMessage = string.Empty // Clear error message on success
            };
        }


        #region DTO Mapping
        private BookingDto MapToDto(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.RoomNumber ?? "N/A",
                RoomType = booking.Room?.RoomType?.Name ?? "N/A",
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalCost = booking.TotalCost,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt,
                ConfirmedAt = booking.ConfirmedAt,
                CancelledAt = booking.CancelledAt,
                CancellationReason = booking.CancellationReason,
                RefundAmount = booking.RefundAmount,
                CancellationFee = booking.CancellationFee
            };
        }
        #endregion

        #region Private helper methods
        private (decimal RefundAmount, decimal CancellationFee) CalculateRefund(Booking booking)
        {
            var daysUntilCheckIn = (booking.CheckInDate - DateTime.Today).Days;
            decimal refundPercentage = 1.0m;

            if (daysUntilCheckIn < 1)
                refundPercentage = 0.0m;
            else if (daysUntilCheckIn < 3)
                refundPercentage = 0.5m;
            else if (daysUntilCheckIn < 7)
                refundPercentage = 0.8m;

            var refundAmount = booking.TotalCost * refundPercentage;
            var cancellationFee = booking.TotalCost - refundAmount;

            return (refundAmount, cancellationFee);
        }

        private bool CanCancelBooking(Booking booking)
        {
            return booking.Status == "Pending" ||
                   booking.Status == "Confirmed";
        } 
        #endregion
    }
}