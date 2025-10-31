using Bookify.Application.Business.Dtos.Bookings;

namespace Bookify.Application.Business.Interfaces
{
    public interface IAdminBookingService
    {
        Task<(IEnumerable<BookingDto> Bookings, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default);
        Task ApproveAsync(int bookingId, CancellationToken cancellationToken = default);
        Task CancelAsync(int bookingId, CancellationToken cancellationToken = default);
    }
}
