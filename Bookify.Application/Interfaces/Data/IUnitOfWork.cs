using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Data
{
	public interface IUnitOfWork
	{
		IRoomRepository Rooms { get; }
		IRoomTypeRepository RoomTypes { get; }
		IBookingRepository Bookings { get; }
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
