// Bookify-Hotel-Reservation-System-master/Bookify.Infrastructure/Data/Repositories/RoomTypeRepository.cs
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.Context;
using Microsoft.EntityFrameworkCore; // Added
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data.Data.Repositories
{
    public class RoomTypeRepository : Repository<RoomType>, IRoomTypeRepository
    {
        public RoomTypeRepository(BookifyDbContext dbContext) : base(dbContext)
        {
        }

        // New: Get RoomType by ID
        public async Task<RoomType?> GetRoomTypeByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
        }
    }
}
