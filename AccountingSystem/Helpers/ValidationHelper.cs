using System;
using System.Text.RegularExpressions;

namespace AccountingSystem.Helpers
{
    public static class ValidationHelper
    {
        public static bool Required(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsInteger(string value, out int result)
        {
            return int.TryParse(value, out result);
        }

        public static bool IsDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, out result);
        }

        public static bool IsPositive(decimal value)
        {
            return value > 0;
        }

        public static bool IsNonNegative(decimal value)
        {
            return value >= 0;
        }

        public static bool IsEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }

        public static bool IsPhone(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            return Regex.IsMatch(value, @"^[0-9+\-\s()]{6,20}$");
        }

        public static bool IsValidDate(string value, out DateTime dt)
        {
            return DateTime.TryParse(value, out dt);
        }
    }
}