using System;

namespace Reference
{
    /// <summary>
    /// Describes an error during the evaluation of the path of the reference.
    /// </summary>
    public class InvalidReferenceException : Exception
    {
        internal InvalidReferenceException()
        {
        }

        internal InvalidReferenceException(String message) : base(message)
        {
        }

        internal InvalidReferenceException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
