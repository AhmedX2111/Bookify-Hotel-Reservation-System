using Bookify.Application.Business.Interfaces.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Business.Services.Background
{
    public class BookingStatusUpdater : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingStatusUpdater> _logger;

        public BookingStatusUpdater(IServiceProvider serviceProvider, ILogger<BookingStatusUpdater> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingStatusUpdater service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();

                    var now = DateTime.UtcNow;

                    // Get all confirmed bookings whose checkout date has passed
                    var expiredBookings = await bookingRepository
                        .GetAllAsync(b => b.Status == "Confirmed" && b.CheckOutDate < now, stoppingToken);

                    foreach (var booking in expiredBookings)
                    {
                        booking.Status = "Completed";

                        // Free up the room
                        var room = await roomRepository.GetByIdAsync(booking.RoomId, stoppingToken);
                        if (room != null)
                        {
                            room.IsAvailable = true;
                        }

                        _logger.LogInformation($"Booking {booking.Id} completed; room {room?.RoomNumber} is now available.");
                    }

                    await unitOfWork.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating booking statuses.");
                }

                // Run once every day at midnight
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }


    }
}
