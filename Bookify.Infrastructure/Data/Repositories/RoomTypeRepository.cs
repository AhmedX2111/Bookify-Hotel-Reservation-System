using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data.Data.Repositories
{
	public class RoomTypeRepository : Repository<RoomType>, IRoomTypeRepository
	{
		public RoomTypeRepository(BookifyDbContext dbContext) : base(dbContext)
		{
		}
	}
}
