using System;

namespace Tlaoami.Application.Exceptions
{
    /// <summary>
    /// Represents a domain/business rule violation.
    /// </summary>
    public class BusinessException : Exception
    {
        public string? Code { get; }

        public BusinessException(string message, string? code = null) : base(message)
        {
            Code = code;
        }

        public BusinessException(string message, Exception innerException, string? code = null) : base(message, innerException)
        {
            Code = code;
        }
    }
}
