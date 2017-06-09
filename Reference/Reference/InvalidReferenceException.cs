using System;

namespace Reference
{
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
