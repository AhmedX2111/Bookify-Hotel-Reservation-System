using System;

namespace Bookify.Shared.Exceptions
{
    public class ValidationException : Exception
    {
        // Parameterless constructor (optional, but good practice)
        public ValidationException() : base() { }

        // REQUIRED FIX: Constructor that takes a message
        public ValidationException(string message) : base(message) { }

        // Optional: Constructor for inner exceptions
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}