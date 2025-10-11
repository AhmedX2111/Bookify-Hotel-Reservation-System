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

            // Create test customer users
            var customer1 = new User
            {
                UserName = "customer1@bookify.com",
                Email = "customer1@bookify.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true
            };

            var customer2 = new User
            {
                UserName = "customer2@bookify.com",
                Email = "customer2@bookify.com",
                FirstName = "Jane",
                LastName = "Smith",
                EmailConfirmed = true
            };

            var customer1Exists = await userManager.FindByEmailAsync(customer1.Email);
            if (customer1Exists == null)
            {
                var createCustomer1 = await userManager.CreateAsync(customer1, "Customer123!");
                if (createCustomer1.Succeeded)
                {
                    await userManager.AddToRoleAsync(customer1, "Customer");
                }
            }

            var customer2Exists = await userManager.FindByEmailAsync(customer2.Email);
            if (customer2Exists == null)
            {
                var createCustomer2 = await userManager.CreateAsync(customer2, "Customer123!");
                if (createCustomer2.Succeeded)
                {
                    await userManager.AddToRoleAsync(customer2, "Customer");
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

            // Seed comprehensive test bookings
            if (!context.Bookings.Any())
            {
                var rooms = await context.Rooms.Take(10).ToListAsync();
                var users = await userManager.Users.ToListAsync();
                var adminUserObj = await userManager.FindByEmailAsync("admin@bookify.com");
                var customer1Obj = await userManager.FindByEmailAsync("customer1@bookify.com");
                var customer2Obj = await userManager.FindByEmailAsync("customer2@bookify.com");

                var testBookings = new List<Booking>
                {
					// Pending Bookings (for admin confirmation testing)
					new Booking
                    {
                        UserId = customer1Obj.Id,
                        RoomId = rooms[0].Id, // Room 101
						CheckInDate = DateTime.Today.AddDays(2),
                        CheckOutDate = DateTime.Today.AddDays(5),
                        TotalCost = 299.97m, // 3 nights * 99.99
						Status = "Pending", // Changed from enum to string
						CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new Booking
                    {
                        UserId = customer2Obj.Id,
                        RoomId = rooms[1].Id, // Room 102
						CheckInDate = DateTime.Today.AddDays(3),
                        CheckOutDate = DateTime.Today.AddDays(6),
                        TotalCost = 449.97m, // 3 nights * 149.99
						Status = "Pending", // Changed from enum to string
						CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },

					// Confirmed Bookings (for cancellation testing)
					new Booking
                    {
                        UserId = customer1Obj.Id,
                        RoomId = rooms[2].Id, // Room 103
						CheckInDate = DateTime.Today.AddDays(7),
                        CheckOutDate = DateTime.Today.AddDays(10),
                        TotalCost = 749.97m, // 3 nights * 249.99
						Status = "Confirmed", // Changed from enum to string
						ConfirmedAt = DateTime.UtcNow.AddDays(-1),
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new Booking
                    {
                        UserId = customer2Obj.Id,
                        RoomId = rooms[3].Id, // Room 104
						CheckInDate = DateTime.Today.AddDays(14),
                        CheckOutDate = DateTime.Today.AddDays(17),
                        TotalCost = 599.97m, // 3 nights * 199.99
						Status = "Confirmed", // Changed from enum to string
						ConfirmedAt = DateTime.UtcNow.AddDays(-2),
                        CreatedAt = DateTime.UtcNow.AddDays(-4)
                    },

					// Active Bookings (currently ongoing)
					new Booking
                    {
                        UserId = customer1Obj.Id,
                        RoomId = rooms[4].Id, // Room 105
						CheckInDate = DateTime.Today.AddDays(-1),
                        CheckOutDate = DateTime.Today.AddDays(2),
                        TotalCost = 899.97m, // 3 nights * 299.99
						Status = "Active", // Changed from enum to string
						ConfirmedAt = DateTime.UtcNow.AddDays(-5),
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },

					// Completed Bookings (past bookings)
					new Booking
                    {
                        UserId = customer2Obj.Id,
                        RoomId = rooms[5].Id, // Room 106
						CheckInDate = DateTime.Today.AddDays(-10),
                        CheckOutDate = DateTime.Today.AddDays(-7),
                        TotalCost = 299.97m, // 3 nights * 99.99
						Status = "Completed", // Changed from enum to string
						ConfirmedAt = DateTime.Today.AddDays(-15),
                        CreatedAt = DateTime.Today.AddDays(-20)
                    },

					// Cancelled Bookings (for testing cancellation flow)
					new Booking
                    {
                        UserId = customer1Obj.Id,
                        RoomId = rooms[6].Id, // Room 107
						CheckInDate = DateTime.Today.AddDays(5),
                        CheckOutDate = DateTime.Today.AddDays(8),
                        TotalCost = 449.97m, // 3 nights * 149.99
						Status = "Cancelled", // Changed from enum to string
						CancelledAt = DateTime.UtcNow.AddDays(-1),
                        CancellationReason = "Change of plans",
                        RefundAmount = 404.97m,
                        CancellationFee = 45.00m,
                        ConfirmedAt = DateTime.UtcNow.AddDays(-3),
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },

					// Rejected Bookings (for admin rejection testing)
					new Booking
                    {
                        UserId = customer2Obj.Id,
                        RoomId = rooms[7].Id, // Room 108
						CheckInDate = DateTime.Today.AddDays(1),
                        CheckOutDate = DateTime.Today.AddDays(4),
                        TotalCost = 749.97m, // 3 nights * 249.99
						Status = "Rejected", // Changed from enum to string
						RejectedAt = DateTime.UtcNow.AddDays(-1),
                        RejectionReason = "Room under maintenance",
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },

					// Overlapping Bookings (for availability testing)
					new Booking
                    {
                        UserId = customer1Obj.Id,
                        RoomId = rooms[8].Id, // Room 109
						CheckInDate = DateTime.Today.AddDays(10),
                        CheckOutDate = DateTime.Today.AddDays(15),
                        TotalCost = 1249.95m, // 5 nights * 249.99
						Status = "Confirmed", // Changed from enum to string
						ConfirmedAt = DateTime.UtcNow.AddDays(-2),
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new Booking
                    {
                        UserId = customer2Obj.Id,
                        RoomId = rooms[9].Id, // Room 110
						CheckInDate = DateTime.Today.AddDays(20),
                        CheckOutDate = DateTime.Today.AddDays(25),
                        TotalCost = 999.95m, // 5 nights * 199.99
						Status = "Confirmed", // Changed from enum to string
						ConfirmedAt = DateTime.UtcNow.AddDays(-1),
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    }
                };

                context.Bookings.AddRange(testBookings);
                await context.SaveChangesAsync();
            }

            // Add some bookings that create availability conflicts for testing
            if (context.Bookings.Count() < 15)
            {
                var rooms = await context.Rooms.Skip(10).Take(3).ToListAsync();
                var customer1Obj = await userManager.FindByEmailAsync("customer1@bookify.com");

                var conflictBookings = new List<Booking>
                {
					// Same room, overlapping dates
					new Booking
                    {
                        UserId = customer1Obj.Id,
                        RoomId = rooms[0].Id, // Room 111
						CheckInDate = DateTime.Today.AddDays(5),
                        CheckOutDate = DateTime.Today.AddDays(8),
                        TotalCost = 299.97m,
                        Status = "Confirmed", // Changed from enum to string
						ConfirmedAt = DateTime.UtcNow.AddDays(-1),
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new Booking
                    {
                        UserId = customer1Obj.Id,
                        RoomId = rooms[0].Id, // Same room 111
						CheckInDate = DateTime.Today.AddDays(7), // Overlaps with previous
						CheckOutDate = DateTime.Today.AddDays(10),
                        TotalCost = 299.97m,
                        Status = "Confirmed", // Changed from enum to string
						ConfirmedAt = DateTime.UtcNow.AddDays(-1),
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                };

                context.Bookings.AddRange(conflictBookings);
                await context.SaveChangesAsync();
            }
        }
    }
}