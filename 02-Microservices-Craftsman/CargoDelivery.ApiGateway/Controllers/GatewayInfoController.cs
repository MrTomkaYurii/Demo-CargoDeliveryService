using Microsoft.AspNetCore.Mvc;

namespace CargoDelivery.ApiGateway.Controllers;

/// <summary>
/// ====================================================================================
/// 🧭 КОНТРОЛЕР ОГЛЯДУ МАРШРУТІВ OCELOT API GATEWAY
/// ------------------------------------------------------------------------------------
/// Оскільки сам Ocelot працює як конвеєр проміжного ПЗ (Middleware), цей контролер
/// відображається у Swagger UI (/swagger) на порту :5000 і надає інтерактивну карту
/// всіх налаштованих маршрутів проксіювання (Routes) та агрегації (Aggregates).
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/gateway")]
public sealed class GatewayInfoController : ControllerBase
{
    /// <summary>
    /// Повертає повну карту маршрутів Ocelot API Gateway (Reverse Proxy + Request Aggregation).
    /// </summary>
    [HttpGet("routes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetGatewayRoutes()
    {
        return Ok(new
        {
            gatewayName = "CargoDelivery.ApiGateway (Ocelot .NET 10)",
            baseUrl = "https://localhost:5000",
            configFile = "ocelot.json",
            reverseProxyRoutes = new[]
            {
                new { upstream = "GET/POST /gateway/drivers", downstream = "https://localhost:5001/api/drivers", features = "RateLimiting (30 req/1m), Header Transform" },
                new { upstream = "GET/PUT/DELETE /gateway/drivers/{id}", downstream = "https://localhost:5001/api/drivers/{id}", features = "Reverse Proxy" },
                new { upstream = "GET/POST /gateway/deliveries", downstream = "https://localhost:5003/api/deliveries", features = "RoundRobin LoadBalancer" },
                new { upstream = "GET/POST /gateway/blog/posts", downstream = "https://localhost:5002/api/postaboutdrivers", features = "Reverse Proxy" },
                new { upstream = "GET /gateway/driver-dashboard/{driverId}", downstream = "https://localhost:5001/api/v1/driver-dashboard/{driverId}", features = "Проксіювання до вбудованого BFF" },
                new { upstream = "GET /gateway/shipment-summary/{driverId}", downstream = "https://localhost:5005/api/v1/shipment-summary/{driverId}", features = "Проксіювання до виділеного HttpAggregator" }
            },
            ocelotAggregationEndpoints = new[]
            {
                new
                {
                    upstream = "GET /gateway/ocelot-aggregate/{driverId}",
                    type = "Декларативна JSON-агрегація Ocelot (без C# коду)",
                    routeKeys = new[] { "DriverProfile (:5001)", "ActiveDeliveries (:5003)", "DriverBlogNotes (:5002)" }
                },
                new
                {
                    upstream = "GET /gateway/custom-aggregate/{driverId}",
                    type = "Кастомний C# агрегатор DriverSummaryDefinedAggregator (IDefinedAggregator)",
                    routeKeys = new[] { "DriverProfile (:5001)", "ActiveDeliveries (:5003)", "DriverBlogNotes (:5002)" }
                }
            }
        });
    }
}
