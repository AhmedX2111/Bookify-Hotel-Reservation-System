using System;

namespace Bookify.Shared.Exceptions
{
    public class NotFoundException : Exception
    {
        // Parameterless constructor (optional, but good practice)
        public NotFoundException() : base() { }

        // REQUIRED FIX: Constructor that takes a message
        public NotFoundException(string message) : base(message) { }

        // Optional: Constructor for inner exceptions
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}