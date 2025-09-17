using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data
{
	public static class SeedData
	{
		public static async Task Initialize(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<BookifyDbContext>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			// Create only the allowed roles
			string[] allowedRoles = { "Customer", "Admin", "Manager" };
			foreach (var roleName in allowedRoles)
			{
				var roleExist = await roleManager.RoleExistsAsync(roleName);
				if (!roleExist)
				{
					await roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}

			// Create admin user
			var adminUser = new User
			{
				UserName = "admin@bookify.com",
				Email = "admin@bookify.com",
				FirstName = "Admin",
				LastName = "User",
				EmailConfirmed = true
			};

			var adminExists = await userManager.FindByEmailAsync(adminUser.Email);
			if (adminExists == null)
			{
				var createAdmin = await userManager.CreateAsync(adminUser, "Admin123!");
				if (createAdmin.Succeeded)
				{
					await userManager.AddToRoleAsync(adminUser, "Admin");
				}
			}

			// Seed room types if none exist
			if (!context.RoomTypes.Any())
			{
				var roomTypes = new List<RoomType>
				{
					new RoomType
					{
						Name = "Standard Room",
						Description = "Comfortable room with basic amenities",
						PricePerNight = 99.99m,
						Capacity = 2
					},
					new RoomType
					{
						Name = "Deluxe Room",
						Description = "Spacious room with premium amenities",
						PricePerNight = 149.99m,
						Capacity = 3
					},
					new RoomType
					{
						Name = "Suite",
						Description = "Luxurious suite with separate living area",
						PricePerNight = 249.99m,
						Capacity = 4
					},
					new RoomType
					{
						Name = "Family Room",
						Description = "Perfect for families with children",
						PricePerNight = 199.99m,
						Capacity = 5
					},
					new RoomType
					{
						Name = "Executive Suite",
						Description = "Premium suite for business travelers",
						PricePerNight = 299.99m,
						Capacity = 2
					}
				};

				context.RoomTypes.AddRange(roomTypes);
				await context.SaveChangesAsync();
			}

			// Seed rooms if none exist
			if (!context.Rooms.Any())
			{
				var roomTypes = await context.RoomTypes.ToListAsync();
				var rooms = new List<Room>();

				// Create 20 rooms with different types and availability
				for (int i = 1; i <= 20; i++)
				{
					var roomType = roomTypes[(i - 1) % roomTypes.Count];
					rooms.Add(new Room
					{
						RoomNumber = $"{100 + i}",
						RoomTypeId = roomType.Id,
						IsAvailable = true // All rooms are available for testing
					});
				}

				// Make a few rooms unavailable for testing
				rooms[2].IsAvailable = false; // Room 103
				rooms[7].IsAvailable = false; // Room 108
				rooms[15].IsAvailable = false; // Room 116

				context.Rooms.AddRange(rooms);
				await context.SaveChangesAsync();
			}

			// Seed some test bookings to see availability in action
			if (!context.Bookings.Any())
			{
				var rooms = await context.Rooms.Take(5).ToListAsync();
				var users = await userManager.Users.ToListAsync();

				var testBookings = new List<Booking>
				{
					new Booking
					{
						UserId = users.First().Id,
						RoomId = rooms[0].Id, // Room 101
                        CheckInDate = DateTime.Today.AddDays(2),
						CheckOutDate = DateTime.Today.AddDays(5),
						NumberOfNights = 3,
						TotalCost = 299.97m, // 3 nights * 99.99
                        Status = "Confirmed",
						CreatedAt = DateTime.UtcNow
					},
					new Booking
					{
						UserId = users.First().Id,
						RoomId = rooms[1].Id, // Room 102
                        CheckInDate = DateTime.Today.AddDays(1),
						CheckOutDate = DateTime.Today.AddDays(3),
						NumberOfNights = 2,
						TotalCost = 299.98m, // 2 nights * 149.99
                        Status = "Pending",
						CreatedAt = DateTime.UtcNow
					},
					new Booking
					{
						UserId = users.First().Id,
						RoomId = rooms[3].Id, // Room 104
                        CheckInDate = DateTime.Today.AddDays(10),
						CheckOutDate = DateTime.Today.AddDays(15),
						NumberOfNights = 5,
						TotalCost = 1249.95m, // 5 nights * 249.99
                        Status = "Confirmed",
						CreatedAt = DateTime.UtcNow
					}
				};

				context.Bookings.AddRange(testBookings);
				await context.SaveChangesAsync();
			}
		}
	}
}
