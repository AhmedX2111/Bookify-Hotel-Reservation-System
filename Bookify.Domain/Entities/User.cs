using Microsoft.AspNetCore.Identity;

namespace Bookify.Domain.Entities
{
	public class User : IdentityUser
	{
		public string FirstName { get; set; } = null!;
		public string LastName { get; set; } = null!;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
	}
}
