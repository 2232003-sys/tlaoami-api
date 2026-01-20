using System;

namespace Tlaoami.Application.Exceptions
{
    /// <summary>
    /// Represents not found resource errors.
    /// </summary>
    public class NotFoundException : Exception
    {
        public string? Code { get; }

        public NotFoundException(string message, string? code = null) : base(message)
        {
            Code = code;
        }

        public NotFoundException(string message, Exception innerException, string? code = null) : base(message, innerException)
        {
            Code = code;
        }
    }
}