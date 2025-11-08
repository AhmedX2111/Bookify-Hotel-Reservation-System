using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Bookings
{
    public class UserProfileDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public IEnumerable<BookingHistoryDto> Bookings { get; set; }
    }
}
