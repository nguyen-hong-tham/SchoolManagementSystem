using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendMVC.Models
{
    public class PastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime > DateTime.UtcNow)
                {
                    return new ValidationResult(ErrorMessage ?? "Ngày không thể ở tương lai.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
