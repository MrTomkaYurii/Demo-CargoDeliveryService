# 🛠️ 02. Анатомія ServiceDefaults: Параметри, Опції та Альтернативи

> **Розділ:** `02-Microservices-Craftsman` • **Модуль:** `02-01-Aspire-Documentation`  
> **Стек:** .NET 10, OpenTelemetry, Polly Resilience, Health Checks, Service Discovery.  
> **Офіційна документація Microsoft:** [.NET Aspire Service Defaults Overview](https://learn.microsoft.com/dotnet/aspire/fundamentals/service-defaults)

---

## 📑 Зміст модуля

- [Слайд 1. Філософія ServiceDefaults: Єдиний стандарт замість дублювання конфігурацій](#slide-1)
- [Слайд 2. Дженерик-обмеження IHostApplicationBuilder: Чому це працює всюди](#slide-2)
- [Слайд 3. Розбір методу ConfigureOpenTelemetry: Параметри логів, метрик та трейсів](#slide-3)
- [Слайд 4. Фільтрація трасування (options.Filter): Чому не можна трейсити Health Checks](#slide-4)
- [Слайд 5. Змінні оточення OTLP: OTEL_EXPORTER_OTLP_ENDPOINT та самоналаштування](#slide-5)
- [Слайд 6. Анатомія AddDefaultHealthChecks: Теги ("live" vs "ready") та зонди](#slide-6)
- [Слайд 7. Глобальний конфігуратор ConfigureHttpClientDefaults: Polly та Discovery](#slide-7)
- [Слайд 8. Таблиця тюнінгу: Дефолтні значення проти Продакшн-оптимізації](#slide-8)

---

<a id="slide-1"></a>
### 🎬 Слайд 1. Філософія ServiceDefaults: Єдиний стандарт замість дублювання конфігурацій

#### Проблема класичного підходу
Коли система розростається до 10–50 мікросервісів, у кожному з них у файлі `Program.cs` починають копіювати однаковий код налаштування OpenTelemetry, Health Checks, Resilience та логування. З часом:
* У половині сервісів забувають підключити `AddHttpClientInstrumentation`.
* В іншій половині забувають налаштувати фільтрацію Health Checks, і база трейсів забивається гігабайтами шуму.
* При переході на новий експортер доводиться змінювати десятки репозиторіїв.

#### Рішення: Проєкт `CargoDelivery.ServiceDefaults`
> 📍 **Вихідний файл у рішенні:**  
> [Extensions.cs](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/02-Microservices-Craftsman/Aspire/CargoDelivery.ServiceDefaults/Extensions.cs)

Проєкт `CargoDelivery.ServiceDefaults` компілюється як звичайна бібліотека класів і підключається до кожного сервісу. У `Program.cs` викликається лише один метод:

```csharp
// Program.cs будь-якого мікросервісу:
var builder = WebApplication.CreateBuilder(args);

// Один рядок коду активує весь золотий стандарт хмарної розробки!
builder.AddServiceDefaults();
```

---

<a id="slide-2"></a>
### 🧬 Слайд 2. Дженерик-обмеження IHostApplicationBuilder: Чому це працює всюди

У файлі `Extensions.cs` сигнатура головного методу оголошена так:

```csharp
public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) 
    where TBuilder : IHostApplicationBuilder
```

#### Чому не `WebApplicationBuilder`?
Інтерфейс `IHostApplicationBuilder` з'явився у .NET 8 як спільний знаменник для всіх типів додатків .NET:
1. `WebApplicationBuilder` — для класичних REST/gRPC API.
2. `HostApplicationBuilder` — для фонових воркерів (Background Worker Services, що слухають лише RabbitMQ).
3. `Aspire.Hosting.DistributedApplicationBuilder` — для оркестратора AppHost.

Завдяки дженерик-параметру `TBuilder` метод `AddServiceDefaults()` можна викликати як у Web API, так і у фоновому сервісі обробки подій без приведення типів!

---

<a id="slide-3"></a>
### 📊 Слайд 3. Розбір методу ConfigureOpenTelemetry: Параметри логів, метрик та трейсів

> 📍 **Фрагмент коду в рішенні:**  
> [Extensions.cs (рядки 60–100)](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/02-Microservices-Craftsman/Aspire/CargoDelivery.ServiceDefaults/Extensions.cs#L60-L100)

```csharp
public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) 
    where TBuilder : IHostApplicationBuilder
{
    // 1. СТРУКТУРОВАНЕ ЛОГУВАННЯ
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });

    // 2. МЕТРИКИ РАНТАЙМУ ТА МЕРЕЖІ
    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddRuntimeInstrumentation();
        })
        // 3. НАКРІЗНЕ ТРАСУВАННЯ (TRACING)
        .WithTracing(tracing =>
        {
            tracing.AddSource(builder.Environment.ApplicationName)
                   .AddSource("MassTransit")
                   .AddSource("Microsoft.EntityFrameworkCore.SqlServer")
                   .AddAspNetCoreInstrumentation(options =>
                   {
                       options.Filter = context =>
                           !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                           && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);
                   })
                   .AddHttpClientInstrumentation();
        });

    builder.AddOpenTelemetryExporters();
    return builder;
}
```

#### Детальний опис кожного параметра:
1. `logging.IncludeFormattedMessage = true`:
   * Зберігає повний людський текст повідомлення логу на додачу до параметрів `{DriverId}`.
   * *Альтернатива:* Встановити `false` на екстремально високих навантаженнях (high-throughput), щоб зменшити виділення пам'яті (LOH/GC).
2. `logging.IncludeScopes = true`:
   * Вмикає скоупи `ILogger.BeginScope(...)`. Завдяки цьому кожен запис логу автоматично містить `TraceId` та `SpanId`, зв'язуючи логи з трасуванням в один клік у дашборді.
3. `AddRuntimeInstrumentation()`:
   * Збирає життєві показники процесу .NET: розміри пам'яті Gen0/Gen1/Gen2, затримки збирача сміття GC, кількість вільних потоків у ThreadPool.
4. `tracing.AddSource(...)`:
   * Реєструє провайдери подій:
     * `builder.Environment.ApplicationName` — для власних бізнес-спанів вашого сервісу.
     * `"MassTransit"` — автоматично трейсить відправку та споживання повідомлень через черги RabbitMQ.
     * `"Microsoft.EntityFrameworkCore.SqlServer"` — фіксує кожен SQL-запит із точним часом виконання.

---

<a id="slide-4"></a>
### 🚫 Слайд 4. Фільтрація трасування (options.Filter): Чому не можна трейсити Health Checks

Зверніть увагу на цей критичний блок:

```csharp
options.Filter = context =>
    !context.Request.Path.StartsWithSegments(HealthEndpointPath)
    && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);
```

#### Чому це життєво необхідно?
У мікросервісних кластерах (Kubernetes, Docker, Aspire Dashboard) агенти моніторингу опитують зонди `/health` та `/alive` кожні **2–5 секунд**.
* При 10 сервісах за добу генерується **понад 300 000 штучних HTTP-запитів**.
* Якщо не відфільтрувати ці запити, база трасування (OTLP backend) буде на 90% заповнена порожніми пінгами здоров'я.
* Розробнику стає неможливо знайти реальні призначені для користувача трейси.
* `options.Filter` відсікає цей шум ще до створення спана в пам'яті, заощаджуючи ресурси CPU та мережі.

---

<a id="slide-5"></a>
### 📡 Слайд 5. Змінні оточення OTLP: OTEL_EXPORTER_OTLP_ENDPOINT та самоналаштування

```csharp
private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) 
    where TBuilder : IHostApplicationBuilder
{
    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (useOtlpExporter)
    {
        builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    return builder;
}
```

#### Принцип самостійної активації (Graceful Fallback):
1. **При запуску через Aspire AppHost:** Aspire автоматично передає в процес змінну оточення `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:18889` (внутрішній gRPC/HTTP OTLP-приймач дашборду). Експортер активується автоматично.
2. **При запуску в ізольованому Unit/Integration-тесті:** Змінна оточення відсутня. Метод `AddOpenTelemetryExporters` розумно **пропускає** реєстрацію експортера. Процес не намагається підключитися до неіснуючого сервера OTLP і не засмічує тести винятками `HttpRequestException: Connection Refused`!

---

<a id="slide-6"></a>
### 🩺 Слайд 6. Анатомія AddDefaultHealthChecks: Теги ("live" vs "ready") та зонди

> 📍 **Фрагмент коду в рішенні:**  
> [Extensions.cs (рядки 118–149)](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/02-Microservices-Craftsman/Aspire/CargoDelivery.ServiceDefaults/Extensions.cs#L118-L149)

```csharp
public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) 
    where TBuilder : IHostApplicationBuilder
{
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

    return builder;
}
```

#### Мапінг ендпоінтів у `MapDefaultEndpoints()`:
```csharp
// 1. READINESS PROBE: Сервіс готовий обслуговувати клієнтські запити
app.MapHealthChecks("/health");

// 2. LIVENESS PROBE: Сервіс не завис і його процес CLR живий
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});
```

* **`/alive` (Liveness):** Опитує лише перевірки з тегом `live` (наприклад, стан самого процесу). Якщо база даних недоступна, `/alive` все одно повертає 200 OK. Контейнер НЕ перезапускається.
* **`/health` (Readiness):** Викликає всі перевірки без фільтра (БД, RabbitMQ, сторонні API). Якщо база даних офлайн, `/health` повертає 503 Service Unavailable, і балансувальник не пускає трафік на цей інстанс.

---

<a id="slide-7"></a>
### 🛡️ Слайд 7. Глобальний конфігуратор ConfigureHttpClientDefaults: Polly та Discovery

```csharp
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
    http.AddServiceDiscovery();
});
```

#### Що робить `ConfigureHttpClientDefaults`?
Це одна з найпотужніших можливостей .NET Aspire:
1. Замість того, щоб налаштовувати кожен окремий `HttpClient` вручну, конфігурація застосовується **до всіх клієнтів**, зареєстрованих через `IHttpClientFactory`.
2. **`AddServiceDiscovery()`**: автоматично дозволяє всім клієнтам резолвити логічні адреси на зразок `http://cargoorderingservice`.
3. **`AddStandardResilienceHandler()`**: додає вбудований конвеєр Polly із 5 захисних політик (детально розібрано у файлі `06_ServiceDiscovery_Resilience_Polly_HttpClient.md`).

---

<a id="slide-8"></a>
### 🎛️ Слайд 8. Таблиця тюнінгу: Дефолтні значення проти Продакшн-оптимізації

| Параметр | Значення за замовчуванням у ServiceDefaults | Альтернативне налаштування під високе навантаження |
| :--- | :--- | :--- |
| **OpenTelemetry Sampling** | 100% трейсів (`AlwaysOnSampler`) | `ParentBasedSampler(new TraceIdRatioBasedSampler(0.05))` — збереження лише 5% трейсів для економії сховища |
| **IncludeFormattedMessage** | `true` (відформатований текст) | `false` (тільки шаблон і аргументи) для скорочення алокацій пам'яті |
| **Total Request Timeout** | 30 секунд | 5–10 секунд для критично швидких API |
| **Retry Attempt Count** | 3 повторні спроби | 1–2 спроби для запобігання каскадному перевантаженню (Retry Storm) |
| **Health Check Timeout** | 10 секунд за замовчуванням | 2 секунди для швидкої реакції Kubernetes Probe |
