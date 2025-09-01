using System.ComponentModel.DataAnnotations;

namespace trucki.Attributes
{
    public class DotNumberValidationAttribute : ValidationAttribute
    {
        public string CountryProperty { get; set; }

        public DotNumberValidationAttribute(string countryProperty)
        {
            CountryProperty = countryProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the country property value
            var countryProperty = validationContext.ObjectType.GetProperty(CountryProperty);
            if (countryProperty == null)
            {
                return new ValidationResult($"Unknown property: {CountryProperty}");
            }

            var countryValue = countryProperty.GetValue(validationContext.ObjectInstance)?.ToString();
            var dotNumber = value?.ToString();

            // For US drivers, DOT number is required
            if (string.Equals(countryValue, "US", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(dotNumber))
                {
                    return new ValidationResult("DOT number is required for US drivers");
                }

                // Validate DOT number format (7-12 digits)
                if (!System.Text.RegularExpressions.Regex.IsMatch(dotNumber, @"^\d{7,12}$"))
                {
                    return new ValidationResult("DOT number must be 7-12 digits for US drivers");
                }
            }
            // For Nigerian drivers, DOT number is optional
            else if (string.Equals(countryValue, "NG", StringComparison.OrdinalIgnoreCase))
            {
                // If provided, still validate format
                if (!string.IsNullOrEmpty(dotNumber) && 
                    !System.Text.RegularExpressions.Regex.IsMatch(dotNumber, @"^\d{7,12}$"))
                {
                    return new ValidationResult("DOT number must be 7-12 digits if provided");
                }
            }

            return ValidationResult.Success;
        }
    }
}