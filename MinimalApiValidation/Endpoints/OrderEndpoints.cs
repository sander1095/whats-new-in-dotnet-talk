using Microsoft.AspNetCore.Http.HttpResults;
using Validation.Models;

namespace Validation.Endpoints;

public static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        // Group + tag endpoints
        var orders = app.MapGroup("/orders")
            .WithTags("Orders");

        // Create (rich DTO) — union results let OpenAPI infer 201/400
        orders.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .WithDescription("Creates an order and returns it with a Location header.");

        // Create (simple record)
        orders.MapPost("/simple", (CreateOrder order) =>
            {
                var id = Guid.NewGuid();
                var response = new { Id = id, order.Product, order.Quantity, order.CustomerEmail };
                return TypedResults.Created($"/orders/simple/{id}", response);
            })
            .WithName("CreateOrderSimple")
            .WithSummary("Create a new simple order");

        return app;
    }

    private static Created<OrderResponse> CreateOrder(OrderDto dto)
    {
        var id = Guid.NewGuid();
        var response = new OrderResponse(id, dto.Product, dto.Quantity, dto.CustomerEmail, dto.Address);
        return TypedResults.Created($"/orders/{id}", response);
    }

}