using System;
using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class PastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime > DateTime.UtcNow)
                {
                    return new ValidationResult(ErrorMessage ?? "Date cannot be in the future.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
