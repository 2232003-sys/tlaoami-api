using System;

namespace Tlaoami.Application.Exceptions
{
    /// <summary>
    /// Represents input validation errors.
    /// </summary>
    public class ValidationException : Exception
    {
        public string? Code { get; }

        public ValidationException(string message, string? code = null) : base(message)
        {
            Code = code;
        }

        public ValidationException(string message, Exception innerException, string? code = null) : base(message, innerException)
        {
            Code = code;
        }
    }
}