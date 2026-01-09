# ASP.NET Core 10 Minimal API Validation Sample

This sample demonstrates the new validation features in ASP.NET Core 10 for Minimal APIs.
It shows both:
- a composite DTO with nested class validation (`/orders`), and
- a positional record with validation attributes (`/orders/simple`).

We use the recommended approach for documentation:
- Swashbuckle (`Swashbuckle.AspNetCore`) to generate Swagger JSON + UI
- Microsoft OpenAPI helpers (`Microsoft.AspNetCore.OpenApi`) to annotate endpoints with `.WithOpenApi()`

Both packages are included so the endpoint helpers and the Swagger UI work together.

Run the app

```bash
# from repo root
ASPNETCORE_URLS=http://localhost:5001 dotnet run
```

Test using curl

Invalid composite DTO (validation errors -> ProblemDetails):

```bash
curl -i -X POST http://localhost:5001/orders \
  -H "Content-Type: application/json" \
  -d '{"Product":"","Quantity":0,"CustomerEmail":"not-an-email","Address":{"Street":"","City":""}}'
```

Valid composite DTO (201 Created):

```bash
curl -i -X POST http://localhost:5001/orders \
  -H "Content-Type: application/json" \
  -d '{"Product":"Valid Product","Quantity":10,"CustomerEmail":"customer@example.com","Address":{"Street":"1 Main St","City":"Town"}}'
```

Record-based endpoint (flat payload):

```bash
curl -i -X POST http://localhost:5001/orders/simple \
  -H "Content-Type: application/json" \
  -d '{"Product":"","Quantity":0,"CustomerEmail":"not-an-email"}'
```

Using the `.http` file

Open `test-invalid-order.http` in VS Code and use the REST Client extension (`humao.rest-client`) to send requests interactively.

Notes

- Keeping both Swashbuckle and Microsoft.OpenApi gives you the full Swagger UI plus the convenience of `.WithOpenApi()`.
- If you prefer a minimal footprint, you can remove Swashbuckle and use the `builder.AddOpenApi()` / `app.MapOpenApi()` helpers instead.

License: MIT
