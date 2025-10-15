using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Data
{
	public interface IBookingRepository : IRepository<Booking>
	{
        Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
        Task<IEnumerable<Booking>> GetOverlappingBookingsAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<Booking> GetByIdAsync(int id);
    }
}
