using System.Text.RegularExpressions;

namespace JournalApp.Services
{
    public class ValidationService
    {
        // Username: Only letters and numbers, no symbols
        public static (bool IsValid, string Error) ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required.");

            if (username.Length < 3)
                return (false, "Username must be at least 3 characters long.");

            if (username.Length > 20)
                return (false, "Username must be less than 20 characters.");

            // Only letters and numbers allowed
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
                return (false, "Username can only contain letters and numbers (no symbols).");

            return (true, "");
        }

        // Email: Must end with @gmail.com
        public static (bool IsValid, string Error) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required.");

            if (!email.ToLower().EndsWith("@gmail.com"))
                return (false, "Email must end with @gmail.com.");

            // Basic email format validation
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
                return (false, "Please enter a valid Gmail address (e.g., username@gmail.com).");

            return (true, "");
        }

        // Password: At least 8 characters, one symbol, one number, one capital letter
        public static (bool IsValid, string Error) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required.");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters long.");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return (false, "Password must contain at least one capital letter (A-Z).");

            if (!Regex.IsMatch(password, @"[0-9]"))
                return (false, "Password must contain at least one number (0-9).");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                return (false, "Password must contain at least one symbol (!@#$%^&* etc).");

            return (true, "");
        }
    }
}
