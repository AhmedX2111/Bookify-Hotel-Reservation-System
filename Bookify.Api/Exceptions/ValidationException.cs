using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Shared.Exceptions
{
    public class ValidationException : Exception
    {
        // 1. Parameterless constructor (good practice)
        public ValidationException() : base() { }

        // 2. REQUIRED FIX: Constructor that takes a message
        public ValidationException(string message) : base(message) { }

        // 3. Optional: Constructor for inner exceptions
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
