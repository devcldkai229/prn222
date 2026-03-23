using System;

namespace MealPrep.BLL.Exceptions
{
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException(string email)
            : base($"Email '{email}' is already registered.")
        {
        }
    }

    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
            : base("Invalid email or password.")
        {
        }
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(Guid userId)
            : base($"User with ID '{userId}' was not found.")
        {
        }
    }

    public class AccountDeactivatedException : Exception
    {
        public AccountDeactivatedException()
            : base("This account has been deactivated.")
        {
        }
    }

    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException()
            : base("Current password is incorrect.")
        {
        }
    }
}
