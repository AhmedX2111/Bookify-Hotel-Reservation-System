using Bookify.Domain.Entities;
using System.Linq.Expressions;

namespace Bookify.Application.Business.Interfaces.Data
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
        Task<IEnumerable<Booking>> GetOverlappingBookingsAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<Booking> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> GetBookingsPagedAsync(
      int pageNumber,
      int pageSize,
      string? search = null,
      CancellationToken cancellationToken = default);

        Task<int> CountAsync(string? search = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Booking>> GetAllAsync(Expression<Func<Booking, bool>> predicate, CancellationToken cancellationToken);

    }
}
