using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Validators
{
    /// <summary>
    /// Validates that a value is one of the defined values of the provided enum type.
    /// Use like: [ValidEnumValue(typeof(LineType))]
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidEnumValueAttribute : ValidationAttribute
    {
        private readonly Type _enumType;

        public ValidEnumValueAttribute(Type enumType)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            if (!enumType.IsEnum) throw new ArgumentException("type must be an enum", nameof(enumType));
            _enumType = enumType;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // If no value provided, let [Required] handle that if needed.
            if (value == null) return ValidationResult.Success;

            // For underlying numeric values, Enum.IsDefined works with boxed values (int, byte, etc.)
            if (Enum.IsDefined(_enumType, value))
            {
                return ValidationResult.Success;
            }

            // Also allow string names (in case JSON sent string)
            if (value is string s && Enum.TryParse(_enumType, s, ignoreCase: true, out var _))
            {
                return ValidationResult.Success;
            }

            var memberName = validationContext.MemberName ?? "value";
            var msg = ErrorMessage ?? $"The {memberName} field has an invalid value.";
            return new ValidationResult(msg);
        }
    }
}
