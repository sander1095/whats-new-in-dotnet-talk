using System.ComponentModel.DataAnnotations;

namespace Validation.Models;

public class Address : IValidatableObject
{
    [Required, StringLength(120)]
    public string Street { get; init; } = default!;

    [Required, StringLength(80)]
    public string City { get; init; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Street.Any(char.IsDigit))
        {
            yield return new ValidationResult("Street must contain also a house number", new[] { nameof(Street) });
        }
    }
}