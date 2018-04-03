using System;

namespace Refer
{
    /// <inheritdoc/>
    /// <summary>
    /// Describes an error during the evaluation of the path of the reference.
    /// </summary>
    public class InvalidReferenceException : Exception
    {
        internal InvalidReferenceException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
