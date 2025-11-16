using Bookify.Infrastructure.Data.Data.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bookify.Api.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly BookifyDbContext _dbContext;

        public DatabaseHealthCheck(BookifyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Database is available");
                }
                return HealthCheckResult.Unhealthy("Database is not available");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Database check failed: {ex.Message}", ex);
            }
        }
    }
}

