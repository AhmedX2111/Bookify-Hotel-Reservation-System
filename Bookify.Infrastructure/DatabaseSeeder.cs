using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data
{
	public class DatabaseSeeder : IHostedService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<DatabaseSeeder> _logger;

		public DatabaseSeeder(IServiceProvider serviceProvider, ILogger<DatabaseSeeder> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<BookifyDbContext>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			// Apply pending migrations
			await context.Database.MigrateAsync(cancellationToken);

			// Seed data
			await SeedRolesAsync(roleManager);
			await SeedRoomTypesAsync(context);
			await SeedRoomsAsync(context);
			var admin = await SeedAdminUserAsync(userManager);
			await SeedBookingsAsync(context, admin);

			_logger.LogInformation("Database seeding completed successfully.");
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

		private async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
		{
			var roles = new[] { "Admin", "Customer", "Manager" };

			foreach (var roleName in roles)
			{
				if (!await roleManager.RoleExistsAsync(roleName))
				{
					await roleManager.CreateAsync(new IdentityRole(roleName));
					_logger.LogInformation("Created role: {RoleName}", roleName);
				}
			}
		}

		private async Task SeedRoomTypesAsync(BookifyDbContext context)
		{
			if (await context.RoomTypes.AnyAsync()) return;

			var roomTypes = new[]
			{
				new RoomType { Name = "Standard Room", Description = "Comfortable room with basic amenities", PricePerNight = 99.99m, Capacity = 2 },
				new RoomType { Name = "Deluxe Room", Description = "Spacious room with premium amenities", PricePerNight = 149.99m, Capacity = 3 },
				new RoomType { Name = "Suite", Description = "Luxurious suite with separate living area", PricePerNight = 249.99m, Capacity = 4 },
				new RoomType { Name = "Family Room", Description = "Perfect for families with children", PricePerNight = 199.99m, Capacity = 5 },
				new RoomType { Name = "Executive Suite", Description = "Premium suite for business travelers", PricePerNight = 299.99m, Capacity = 2 }
			};

			await context.RoomTypes.AddRangeAsync(roomTypes);
			await context.SaveChangesAsync();
			_logger.LogInformation("Seeded {Count} room types.", roomTypes.Length);
		}

		private async Task SeedRoomsAsync(BookifyDbContext context)
		{
			if (await context.Rooms.AnyAsync()) return;

			var rooms = new[]
			{
				new Room { RoomNumber = "101", RoomTypeId = 1, IsAvailable = true },
				new Room { RoomNumber = "102", RoomTypeId = 1, IsAvailable = true },
				new Room { RoomNumber = "103", RoomTypeId = 1, IsAvailable = true },
				new Room { RoomNumber = "104", RoomTypeId = 1, IsAvailable = true },

				new Room { RoomNumber = "201", RoomTypeId = 2, IsAvailable = true },
				new Room { RoomNumber = "202", RoomTypeId = 2, IsAvailable = true },

				new Room { RoomNumber = "301", RoomTypeId = 3, IsAvailable = true },
				new Room { RoomNumber = "302", RoomTypeId = 3, IsAvailable = true },

				new Room { RoomNumber = "401", RoomTypeId = 4, IsAvailable = true },
				new Room { RoomNumber = "402", RoomTypeId = 4, IsAvailable = true },

				new Room { RoomNumber = "501", RoomTypeId = 5, IsAvailable = true },
				new Room { RoomNumber = "502", RoomTypeId = 5, IsAvailable = true }
			};

			await context.Rooms.AddRangeAsync(rooms);
			await context.SaveChangesAsync();
			_logger.LogInformation("Seeded {Count} rooms.", rooms.Length);
		}

		private async Task<User> SeedAdminUserAsync(UserManager<User> userManager)
		{
			var admin = await userManager.FindByEmailAsync("admin@bookify.com");
			if (admin != null) return admin;

			var adminUser = new User
			{
				UserName = "admin",
				Email = "admin@bookify.com",
				FirstName = "System",
				LastName = "Administrator",
				EmailConfirmed = true
			};

			var result = await userManager.CreateAsync(adminUser, "Admin123!");

			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(adminUser, "Admin");
				_logger.LogInformation("Created admin user: admin@bookify.com");
			}
			else
			{
				_logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
			}

			return adminUser;
		}

		private async Task SeedBookingsAsync(BookifyDbContext context, User admin)
		{
			if (await context.Bookings.AnyAsync()) return;

			var room = await context.Rooms.FirstOrDefaultAsync(); // get any existing room
			if (room == null) return; // no rooms exist

			var booking = new Booking
			{
				RoomId = room.Id,  // use the actual Id from DB
				UserId = admin.Id,
				CheckInDate = DateTime.UtcNow.Date.AddDays(1),
				CheckOutDate = DateTime.UtcNow.Date.AddDays(3),
				NumberOfNights = 2,
				Status = "Confirmed",
				TotalCost = 199.98m,
				CreatedAt = DateTime.UtcNow
			};

			await context.Bookings.AddAsync(booking);
			await context.SaveChangesAsync();
		}

	}
}
