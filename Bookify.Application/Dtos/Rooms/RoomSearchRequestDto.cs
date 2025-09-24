using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Rooms
{
	public class RoomSearchRequestDto
	{
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public int? RoomTypeId { get; set; }
		public int? MinCapacity { get; set; }
		public decimal? MaxPrice { get; set; }
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string? SortBy { get; set; } = "PricePerNight";
		public bool SortDescending { get; set; } = false;

		public bool IsValid()
		{
			return !GetValidationErrors().Any();
		}
		// New method that returns specific error messages
		public List<string> GetValidationErrors()
		{
			var errors = new List<string>();

			if (CheckInDate.Date < DateTime.Today)
				errors.Add("Check-in date cannot be in the past.");

			if (CheckInDate >= CheckOutDate)
				errors.Add("Check-out date must be later than check-in date.");

			if (PageNumber <= 0)
				errors.Add("Page number must be greater than 0.");

			if (PageSize <= 0 || PageSize > 100)
				errors.Add("Page size must be between 1 and 100.");

			if (MinCapacity.HasValue && MinCapacity <= 0)
				errors.Add("Minimum capacity must be at least 1.");

			if (MaxPrice.HasValue && MaxPrice <= 0)
				errors.Add("Maximum price must be greater than 0.");

			return errors;
		}
	}
}
