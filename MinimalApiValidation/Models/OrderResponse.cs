namespace Validation.Models;

public record OrderResponse(Guid Id, string Product, int Quantity, string CustomerEmail, Address Address);