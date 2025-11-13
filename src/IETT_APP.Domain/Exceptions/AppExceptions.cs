namespace IETT_APP.Domain.Exceptions
{
    // Base exception
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }
    }

    // Validation hataları
    public class ValidationException : AppException
    {
        public ValidationException(string message) : base(message) { }
    }

    // Not found hataları
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message) { }
    }
}

