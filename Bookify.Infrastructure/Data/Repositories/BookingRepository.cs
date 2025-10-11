//using Bookify.Application.Business.Interfaces.Data;
//using Bookify.Domain.Entities;
//using Bookify.Infrastructure.Data.Data.Context;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Bookify.Infrastructure.Data.Data.Repositories
//{
//	public class BookingRepository : Repository<Booking>, IBookingRepository
//	{
//        private readonly BookifyDbContext _dbContext;

//        public BookingRepository(BookifyDbContext dbContext) : base(dbContext)
//		{
//            _dbContext = dbContext;
//        }

//		public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken = default)
//		{
//			return await _dbContext.Bookings
//				.Include(b => b.Room)
//				.ThenInclude(r => r.RoomType)
//				.Where(b => b.UserId == userId)
//				.OrderByDescending(b => b.CreatedAt)
//				.ToListAsync(cancellationToken);
//		}

//		public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, CancellationToken cancellationToken = default)
//		{
//			var conflictingBookings = await _dbContext.Bookings
//				.Where(b => b.RoomId == roomId &&
//						   (b.Status == "Confirmed" || b.Status == "Pending") &&
//						   b.CheckInDate <= checkOutDate && b.CheckOutDate >= checkInDate)
//				.ToListAsync(cancellationToken);

//			return !conflictingBookings.Any();
//		}
//	}
//}


using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.Context;
using Bookify.Infrastructure.Data.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Data.Data.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly BookifyDbContext _context;

        public BookingRepository(BookifyDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetOverlappingBookingsAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return await _context.Bookings
                .Where(b => b.RoomId == roomId &&
                           b.Status != "Cancelled " && // Use enum, not string
                           b.Status != "Rejected " &&   // Use enum, not string
                           b.CheckInDate < checkOut &&
                           b.CheckOutDate > checkIn)
                .ToListAsync();
        }

        public async Task<Booking> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}