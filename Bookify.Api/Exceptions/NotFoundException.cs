using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Shared.Exceptions
{
    public class NotFoundException : Exception
    {
        // 1. Parameterless constructor (good practice)
        public NotFoundException() : base() { }

        // 2. REQUIRED FIX: Constructor that takes a message
        public NotFoundException(string message) : base(message) { }

        // 3. Optional: Constructor for inner exceptions
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
