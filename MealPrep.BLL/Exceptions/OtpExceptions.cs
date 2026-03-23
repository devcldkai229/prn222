using System;

namespace MealPrep.BLL.Exceptions
{
    public class InvalidOtpException : Exception
    {
        public InvalidOtpException()
            : base("OTP code is invalid or expired.")
        {
        }
    }

    public class OtpNotSentException : Exception
    {
        public OtpNotSentException(string email)
            : base($"OTP has not been sent to '{email}'. Please request OTP first.")
        {
        }
    }
}
