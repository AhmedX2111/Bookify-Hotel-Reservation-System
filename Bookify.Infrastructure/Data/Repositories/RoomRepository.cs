using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data.Data.Repositories
{
	public class RoomRepository : Repository<Room>, IRoomRepository
	{
		private readonly BookifyDbContext _dbContext;

		public RoomRepository(BookifyDbContext dbContext) : base(dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(
			DateTime checkInDate,
			DateTime checkOutDate,
			CancellationToken cancellationToken = default)
		{
			// Get rooms that are not booked for the given date range
			var bookedRoomIds = await _dbContext.Bookings
				.Where(b =>
					b.CheckInDate <= checkOutDate && b.CheckOutDate >= checkInDate &&
					(b.Status == "Confirmed" || b.Status == "Pending"))
				.Select(b => b.RoomId)
				.Distinct()
				.ToListAsync(cancellationToken);

			return await _dbContext.Rooms
				.Include(r => r.RoomType)
				.Where(r => !bookedRoomIds.Contains(r.Id) && r.IsAvailable)
				.ToListAsync(cancellationToken);
		}

		public async Task<(IEnumerable<Room> Rooms, int TotalCount)> SearchAvailableRoomsAsync(
			DateTime checkInDate,
			DateTime checkOutDate,
			int? roomTypeId = null,
			int? minCapacity = null,
			decimal? maxPrice = null,
			string? sortBy = "PricePerNight",
			bool sortDescending = false,
			int pageNumber = 1,
			int pageSize = 10,
			CancellationToken cancellationToken = default)
		{
			// Get booked room IDs for the date range
			var bookedRoomIds = await _dbContext.Bookings
				.Where(b => b.CheckInDate <= checkOutDate && b.CheckOutDate >= checkInDate &&
						   (b.Status == "Confirmed" || b.Status == "Pending"))
				.Select(b => b.RoomId)
				.Distinct()
				.ToListAsync(cancellationToken);

			// Base query
			var query = _dbContext.Rooms
				.Include(r => r.RoomType)
				.Where(r => !bookedRoomIds.Contains(r.Id) && r.IsAvailable);

			// Apply filters
			if (roomTypeId.HasValue)
				query = query.Where(r => r.RoomTypeId == roomTypeId.Value);

			if (minCapacity.HasValue)
				query = query.Where(r => r.RoomType.Capacity >= minCapacity.Value);

			if (maxPrice.HasValue)
				query = query.Where(r => r.RoomType.PricePerNight <= maxPrice.Value);


			// Get total count
			var totalCount = await query.CountAsync(cancellationToken);

			// Apply sorting
			query = sortBy?.ToLower() switch
			{
				"pricepernight" => sortDescending
					? query.OrderByDescending(r => r.RoomType.PricePerNight)
					: query.OrderBy(r => r.RoomType.PricePerNight),
				"capacity" => sortDescending
					? query.OrderByDescending(r => r.RoomType.Capacity)
					: query.OrderBy(r => r.RoomType.Capacity),
				"roomnumber" => sortDescending
					? query.OrderByDescending(r => r.RoomNumber)
					: query.OrderBy(r => r.RoomNumber),
				_ => query.OrderBy(r => r.RoomType.PricePerNight)
			};

			// Apply pagination
			var rooms = await query
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync(cancellationToken);

			return (rooms, totalCount);
		}

		public async Task<IEnumerable<RoomType>> GetRoomTypesAsync(CancellationToken cancellationToken = default)
		{
			return await _dbContext.RoomTypes
				.OrderBy(rt => rt.Name)
				.ToListAsync(cancellationToken);
		}

	}
}
