using System.ComponentModel.DataAnnotations;

namespace Validation.Models;

/// <summary>
/// DTO for creating orders
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Product name
    /// </summary>
    [Required, StringLength(100)]
    public string Product { get; init; } = default!;

    /// <summary>
    /// Quantity of products
    /// </summary>
    [Range(1, 1000)]
    public int Quantity { get; init; }

    /// <summary>
    /// Customer email
    /// </summary>
    [Required, EmailAddress]
    public string CustomerEmail { get; init; } = default!;

    /// <summary>
    /// Delivery address
    /// </summary>
    [Required]
    public Address Address { get; init; } = default!;
}