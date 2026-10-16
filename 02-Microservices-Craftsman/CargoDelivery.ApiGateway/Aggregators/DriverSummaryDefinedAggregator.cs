using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ocelot.Middleware;
using Ocelot.Multiplexer;

namespace CargoDelivery.ApiGateway.Aggregators;

/// <summary>
/// ====================================================================================
/// 🧩 КАСТОМНИЙ АГРЕГАТОР OCELOT: DriverSummaryDefinedAggregator (IDefinedAggregator)
/// ====================================================================================
/// ЧОМУ ПОТРІБЕН IDefinedAggregator?
/// У файлі ocelot.json ми налаштували два агреговані маршрути:
///   1. /gateway/ocelot-aggregate/{driverId} — стандартна декларативна агрегація Ocelot.
///      Вона просто склеює сирі JSON-відповіді трьох сервісів у словник за ключами RouteKeys:
///      { "DriverProfile": {...}, "ActiveDeliveries": [...], "DriverBlogNotes": [...] }.
///      Проте вона НЕ вміє перевіряти статус-коди окремих сервісів, рахувати статистику
///      або застосовувати Graceful Degradation.
///
///   2. /gateway/custom-aggregate/{driverId} — використовує цей клас DriverSummaryDefinedAggregator!
///      Коли клієнт викликає цей маршрут, Ocelot:
///        а) паралельно (Fan-Out) виконує всі 3 підзапити ("DriverProfile", "ActiveDeliveries", "DriverBlogNotes");
///        б) збирає їхні результати у список List&lt;HttpContext&gt; responses;
///        в) викликає метод Aggregate(...) цього класу для інтелектуального злиття (Fan-In).
///
/// ЩО РОБИТЬ ЦЕЙ АГРЕГАТОР:
///   • Крок 1: Витягує відповідь кожного мікросервісу за його ключем (Route.Key) із ocelot.json.
///   • Крок 2 (Guard Check): Якщо сервіс водіїв (DriverProfile) повернув 404 Not Found —
///     відразу повертає клієнту змістовну помилку 404 Not Found.
///   • Крок 3 (Критична залежність): Парсить список активних доставок із CargoOrderingService
///     та підраховує їхню кількість (totalActiveDeliveries).
///   • Крок 4 (Graceful Degradation — Плавна деградація): Якщо сервіс блогу (DriverBlogNotes)
///     впав або повернув помилку 5xx/timeout, шлюз НЕ валить весь запит! Замість цього він
///     виставляє прапорець isDegraded = true, повертає порожній масив нотаток і успішно
///     віддає клієнту основні дані про водія та доставки.
/// ====================================================================================
/// </summary>
public sealed class DriverSummaryDefinedAggregator : IDefinedAggregator
{
    private readonly ILogger<DriverSummaryDefinedAggregator> _logger;

    public DriverSummaryDefinedAggregator(ILogger<DriverSummaryDefinedAggregator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Головний метод інтерфейсу IDefinedAggregator, який викликається мультиплексором Ocelot
    /// після завершення всіх паралельних Downstream-підзапитів.
    /// </summary>
    /// <param name="responses">
    /// Список контекстів підзапитів (по одному HttpContext на кожен ключ у масиві "RouteKeys" із ocelot.json).
    /// </param>
    public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        // --------------------------------------------------------------------------------
        // КРОК 1: Розбираємо список HttpContext у зручний словник за ключем маршруту ("Key" з ocelot.json)
        // --------------------------------------------------------------------------------
        // У кожному HttpContext Ocelot зберігає метадані підзапиту у словнику ctx.Items:
        //   • ctx.Items.DownstreamRoute()    -> містить об'єкт маршруту, зокрема властивість .Key
        //                                       ("DriverProfile", "ActiveDeliveries", "DriverBlogNotes")
        //   • ctx.Items.DownstreamResponse() -> містить HTTP-відповідь мікросервісу (StatusCode, Content, Headers)
        var responseMap = new Dictionary<string, DownstreamResponse>(StringComparer.OrdinalIgnoreCase);

        foreach (var ctx in responses)
        {
            var route = ctx.Items.DownstreamRoute();
            var downstreamResponse = ctx.Items.DownstreamResponse();
            if (route?.Key != null && downstreamResponse != null)
            {
                responseMap[route.Key] = downstreamResponse;
            }
        }

        // --------------------------------------------------------------------------------
        // КРОК 2: Перевірка кореневої сутності водія ("DriverProfile" -> DriversManagement :5001)
        // --------------------------------------------------------------------------------
        // Якщо водія з таким ID не існує в базі, повертаємо 404 Not Found.
        if (!responseMap.TryGetValue("DriverProfile", out var driverResp) ||
            driverResp.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Ocelot IDefinedAggregator: Водія не знайдено у DriversManagement (404).");
            var notFoundJson = JsonSerializer.Serialize(new
            {
                gateway = "CargoDelivery.ApiGateway (Ocelot IDefinedAggregator)",
                status = 404,
                message = "Водія із вказаним ідентифікатором не знайдено у сервісі DriversManagement."
            });
            return CreateJsonResponse(notFoundJson, HttpStatusCode.NotFound);
        }

        // Якщо сервіс DriversManagement недоступний (500/502/503), повертаємо 503 Service Unavailable
        if (driverResp.StatusCode != HttpStatusCode.OK)
        {
            return CreateJsonResponse(
                JsonSerializer.Serialize(new { error = "Критичний сервіс DriversManagement недоступний." }),
                HttpStatusCode.ServiceUnavailable);
        }

        var driverElement = await ParseJsonOrDefaultAsync(driverResp);

        // --------------------------------------------------------------------------------
        // КРОК 3: Обробка доставок водія ("ActiveDeliveries" -> CargoOrderingService :5003)
        // --------------------------------------------------------------------------------
        JsonElement deliveriesElement = default;
        int deliveriesCount = 0;
        if (responseMap.TryGetValue("ActiveDeliveries", out var delivResp) &&
            delivResp.StatusCode == HttpStatusCode.OK)
        {
            deliveriesElement = await ParseJsonOrDefaultAsync(delivResp);
            if (deliveriesElement.ValueKind == JsonValueKind.Array)
            {
                // Підраховуємо загальну кількість замовлень/доставок для зведення метрик
                deliveriesCount = deliveriesElement.GetArrayLength();
            }
        }

        // --------------------------------------------------------------------------------
        // КРОК 4: Обробка блогу водія ("DriverBlogNotes" -> DriversBlogManagement :5002)
        // Із застосуванням патерна GRACEFUL DEGRADATION (Плавна деградація)
        // --------------------------------------------------------------------------------
        // Блог водіїв є некритичною (другорядною) залежністю для логістичного екрана.
        // Якщо сервіс блогу тимчасово недоступний, ми НЕ повертаємо 500 клієнту, а лише
        // позначаємо відповідь прапорцем isDegraded = true.
        bool isBlogDegraded = false;
        JsonElement blogElement = default;
        int blogNotesCount = 0;

        if (responseMap.TryGetValue("DriverBlogNotes", out var blogResp) &&
            blogResp.StatusCode == HttpStatusCode.OK)
        {
            blogElement = await ParseJsonOrDefaultAsync(blogResp);
            if (blogElement.ValueKind == JsonValueKind.Array)
            {
                blogNotesCount = blogElement.GetArrayLength();
            }
        }
        else
        {
            isBlogDegraded = true;
            _logger.LogWarning("Ocelot IDefinedAggregator: Сервіс блогу недоступний — застосовано Graceful Degradation.");
        }

        // --------------------------------------------------------------------------------
        // КРОК 5: Формування єдиного збагаченого JSON-конверта для клієнта (Fan-In)
        // --------------------------------------------------------------------------------
        var aggregatedPayload = new
        {
            gatewayEngine = "Ocelot IDefinedAggregator (DriverSummaryDefinedAggregator)",
            aggregatedAtUtc = DateTime.UtcNow,
            isDegraded = isBlogDegraded,
            aggregationStatus = isBlogDegraded ? "Partial (Blog Degraded)" : "Complete",
            metrics = new
            {
                totalActiveDeliveries = deliveriesCount,
                totalBlogNotes = blogNotesCount
            },
            driver = driverElement.ValueKind != JsonValueKind.Undefined ? driverElement : (object?)null,
            activeDeliveries = deliveriesElement.ValueKind == JsonValueKind.Array ? deliveriesElement : (object)Array.Empty<object>(),
            blogNotes = (!isBlogDegraded && blogElement.ValueKind == JsonValueKind.Array) ? blogElement : (object)Array.Empty<object>()
        };

        var json = JsonSerializer.Serialize(aggregatedPayload, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return CreateJsonResponse(json, HttpStatusCode.OK);
    }

    /// <summary>
    /// Безпечно читає тіло відповіді DownstreamResponse та парсить його у JsonElement.
    /// </summary>
    private static async Task<JsonElement> ParseJsonOrDefaultAsync(DownstreamResponse response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return default;
            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.Clone();
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Створює об'єкт DownstreamResponse із JSON-контентом та діагностичним заголовком X-Aggregated-By.
    /// </summary>
    private static DownstreamResponse CreateJsonResponse(string json, HttpStatusCode statusCode)
    {
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        var headers = new List<Header>
        {
            new("X-Aggregated-By", new[] { "Ocelot-DriverSummaryDefinedAggregator" })
        };
        return new DownstreamResponse(stringContent, statusCode, headers, statusCode.ToString());
    }
}
