using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Domain.Common
{
	public class ValueObjects
	{
		public record DateRange(DateTime Start, DateTime End);
	}
}
