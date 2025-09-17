using Bookify.Application.Business.Interfaces.Data;
using Bookify.Infrastructure.Data.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data.Data.Repositories
{
	public class Repository<T> : IRepository<T> where T : class
	{
		protected readonly BookifyDbContext _dbContext;

		public Repository(BookifyDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
		}

		public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _dbContext.Set<T>().ToListAsync(cancellationToken);
		}

		public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
		{
			await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
		}

		public virtual void Update(T entity)
		{
			_dbContext.Set<T>().Update(entity);
		}

		public virtual void Delete(T entity)
		{
			_dbContext.Set<T>().Remove(entity);
		}
	}
}
