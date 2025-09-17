using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data.Data.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork, IDisposable
	{
		private readonly BookifyDbContext _context;

		public UnitOfWork(
			BookifyDbContext context,
			IRoomRepository rooms,
			IRoomTypeRepository roomTypes,
			IBookingRepository bookings)
		{
			_context = context;
			Rooms = rooms;
			RoomTypes = roomTypes;
			Bookings = bookings;
		}

		public IRoomRepository Rooms { get; }
		public IRoomTypeRepository RoomTypes { get; }
		public IBookingRepository Bookings { get; }

		public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return await _context.SaveChangesAsync(cancellationToken);
		}

		public void Dispose()
		{
			_context?.Dispose();
		}
	}
}
