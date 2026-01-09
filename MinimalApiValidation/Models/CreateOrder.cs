using System.ComponentModel.DataAnnotations;

namespace Validation.Models;

public record CreateOrder(
    [Required, StringLength(100)] string Product,
    [Range(1, 1000)] int Quantity,
    [Required, EmailAddress] string CustomerEmail
);