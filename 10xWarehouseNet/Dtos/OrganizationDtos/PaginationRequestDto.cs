using System.ComponentModel.DataAnnotations;

namespace _10xWarehouseNet.Dtos.OrganizationDtos;

/// <summary>
/// Validation attribute for pagination parameters
/// </summary>
public class PaginationValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is int intValue)
        {
            return intValue >= 1 && intValue <= 100;
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be between 1 and 100.";
    }
}

/// <summary>
/// Request DTO for pagination parameters
/// </summary>
public class PaginationRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [PaginationValidation]
    public int PageSize { get; set; } = 50;
}
