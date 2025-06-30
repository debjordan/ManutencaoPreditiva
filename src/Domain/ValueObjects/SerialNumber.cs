using System;

namespace ManutencaoPreditiva.Domain.ValueObjects
{
    public class SerialNumber
    {
        public string Value { get; private set; }

        private SerialNumber() { } // Para EF Core

        public SerialNumber(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            if (!IsValid(value))
                throw new ArgumentException("Invalid serial number format. Must contain only letters, numbers, and hyphens, with 3-50 characters.");
        }

        private bool IsValid(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Length is >= 3 and <= 50 &&
                   System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Za-z0-9-]+$");
        }

        public override bool Equals(object obj)
        {
            if (obj is SerialNumber other)
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}
