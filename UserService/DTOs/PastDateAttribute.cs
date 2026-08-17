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
                // Cho phép ngày hôm nay, chỉ báo lỗi nếu ngày thực sự vượt quá ngày hiện tại
                if (dateTime.Date > DateTime.Today && dateTime.Date > DateTime.UtcNow.Date)
                {
                    return new ValidationResult(ErrorMessage ?? "Date cannot be in the future.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
