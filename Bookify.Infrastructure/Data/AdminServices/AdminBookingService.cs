using AutoMapper;
using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces;
using Bookify.Application.Business.Interfaces.Data;

namespace Bookify.Infrastructure.Data.Data.AdminServices
{
    public class AdminBookingService : IAdminBookingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AdminBookingService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<BookingDto> Bookings, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default)
        {
            // Fetch filtered + paged bookings
            var pagedBookings = await _uow.Bookings.GetBookingsPagedAsync(
                pageNumber,
                pageSize,
                search,
                cancellationToken);

            // Count total *filtered* results (same search term!)
            var totalCount = await _uow.Bookings.CountAsync(
                search,
                cancellationToken);

            // Map to DTOs
            var dtos = _mapper.Map<IEnumerable<BookingDto>>(pagedBookings);

            return (dtos, totalCount);
        }


        public async Task ApproveAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId, cancellationToken)
                          ?? throw new KeyNotFoundException("Booking not found.");

            booking.Status = "Approved";
            _uow.Bookings.Update(booking);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        public async Task CancelAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId, cancellationToken)
                          ?? throw new KeyNotFoundException("Booking not found.");

            booking.Status = "Cancelled";
            _uow.Bookings.Update(booking);
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
