# 🩺 05. Діагностика мікросервісів: Health Checks, Liveness та Readiness Probes

> **Розділ курсу:** `02-Microservices-Craftsman` • **Модуль:** `02-01-Aspire-Documentation`  
> **Стек:** .NET 10, ASP.NET Core Health Checks, Kubernetes Probes, Aspire `WithHttpHealthCheck` / Dashboard Health Status, WebApplicationFactory  
> **Цільова аудиторія:** Архітектори надійності (SRE), системні аналітики, Lead .NET інженери, розробники хмарної інфраструктури  
> **Офіційна специфікація та документація:**  
> - [ASP.NET Core Health Checks in .NET](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)  
> - [Kubernetes Documentation: Configure Liveness, Readiness and Startup Probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)  
> - [IETF Draft: Health Check Response Format for HTTP APIs](https://inadarei.github.io/rfc-healthcheck/)

---

## 📑 Академічний зміст лекції

- [1. Теоретичний базис: Активне зондування проти пасивного моніторингу](#section-1)
- [2. Модель відмов: Чому працюючий процес ОС може бути мертвим для бізнесу](#section-2)
- [3. Триєдина специфікація зондів: Startup, Liveness та Readiness Probes](#section-3)
- [4. Антипатерн "Спіраль смерті" (The Cascading Death Spiral)](#section-4)
- [5. Архітектурна реалізація у ServiceDefaults: Теги, предикати та конфігурація маршрутів](#section-5)
- [6. Проєктування перевірок для зовнішніх залежностей (БД, RabbitMQ, HTTP)](#section-6)
- [7. Стандартизація формату відповіді зондів: Простий текст проти IETF JSON](#section-7)
- [8. Діаграма станів життєвого циклу контейнера під контролем зондів](#section-8)
- [9. Автоматизоване тестування зондів у CargoDelivery.HttpAggregator.Tests](#section-9)
- [10. Архітектурна матриця: Антипатерни vs Еталонні практики проектування зондів](#section-10)
- [11. Як Aspire використовує /health: WithHttpHealthCheck, WaitFor та колонка Health](#section-11a)
- [12. Контрольні запитання та лабораторні вправи](#section-11)

---

<a id="section-1"></a>
## 1. Теоретичний базис: Активне зондування проти пасивного моніторингу

У надійних розподілених системах моніторинг стану здоров'я вузлів розділяється на дві принципово різні парадигми:

1. **Пасивний моніторинг (Passive Monitoring / Metrics & Telemetry):**
   * Система збирає метрики за фактом проходження реальних користувацьких запитів (кількість помилок HTTP 500, середній час затримки $p95/p99$, навантаження на CPU).
   * *Недолік:* Якщо на сервіс не надходить трафік (наприклад, у нічний час або під час очікування події), пасивний моніторинг не знає, чи здатний сервіс обробляти запити взагалі. Першим, хто дізнається про катастрофу, буде користувач.

2. **Активне зондування (Active Probing / Health Checking):**
   * Зовнішній спостерігач (Kubelet у Kubernetes, Orchestrator у .NET Aspire, Consul або балансувальник AWS ALB / Azure Application Gateway) з фіксованим періодом $T_{\text{probe}}$ (наприклад, кожні 5-10 секунд) надсилає стандартизований синтетичний HTTP-запит до виділеного ендпоінта діагностики.
   * Сервіс виконує самодіагностику (перевіряє зв'язок з БД, вільне місце на диску, працездатність черги) і повертає статус-код.

```mermaid
flowchart LR
    subgraph Orchestrator ["⚙️ Оркестратор (.NET Aspire / Kubelet)"]
        Timer["⏱️ Таймер опитування (кожні 5 сек)"]
    end
    
    subgraph Microservice ["📦 Контейнер Microservice"]
        Router["🔀 Kestrel HTTP Pipeline"]
        HC["🩺 ASP.NET Core HealthCheck Engine"]
        DBCheck["💾 SQL Server Ping (SELECT 1)"]
        RMQCheck["🐰 RabbitMQ Socket Ping"]
        SelfCheck["💻 Process CLR Ping"]
    end
    
    Timer -->|"1. GET /alive (Liveness)"| Router
    Timer -->|"2. GET /health (Readiness)"| Router
    Router --> HC
    HC --> SelfCheck
    HC --> DBCheck
    HC --> RMQCheck
    
    HC -.->|"200 OK / 503 Unhealthy"| Timer
```

---

<a id="section-2"></a>
## 2. Модель відмов: Чому працюючий процес ОС може бути мертвим для бізнесу

Типова ілюзія розробників-початківців: *"Якщо процес `dotnet.exe` або контейнер Docker запущений і PID існує в системі — сервіс працює"*.

У реальних високонавантажених системах існує цілий спектр станів так званої **«прихованої відмови» (Silent Failure)**:

1. **Виснаження пулу потоків (ThreadPool Starvation):**
   * Через використання блокуючих синхронних очікувань асинхронного коду (`.Result`, `.Wait()`, `Task.WaitAll()`) або надмірну кількість CPU-інтенсивних обчислень усі потоки пулу виявляються заблокованими.
   * Нові вхідні TCP-запити стають у чергу й очікують виділення потоку. Для зовнішнього світу сервіс виглядає завислим (тайм-аут 30 секунд).
2. **Взаємне блокування (Deadlock на Monitor/lock або синхронізаторах транзакцій):**
   * Два фонові потоки заблокували спільні ресурси у перехресному порядку. Процес споживає 0% CPU, але жоден запит не обробляється.
3. **Вичерпання пулу з'єднань з базою даних (Database Connection Pool Exhaustion):**
   * Розробники забули закрити підключення (`SqlConnection` / `DbContext` поза `using`). Пул вичерпав ліміт у 100 підключень (значення `Max Pool Size` за замовчуванням). Наступні бізнес-запити чекають на вільне з'єднання 15 секунд (`Connect Timeout`) і падають з помилкою `InvalidOperationException: Timeout expired... max pool size was reached`.
4. **Нескінченна пауза збирача сміття (Garbage Collection Pause / Heap Fragmentation):**
   * Через витік пам'яті в Large Object Heap (LOH) процес знаходиться у стані агресивного збирання Full GC Gen 2, блокуючи виконання коду на кілька секунд.

> 🎯 **Висновок:**  
> Перевірка працездатності процесу на рівні операційної системи (наявність PID) є абсолютно недостатньою. Необхідний прикладний зонд, що виконує реальний код всередині середовища виконання .NET CLR.

---

<a id="section-3"></a>
## 3. Триєдина специфікація зондів: Startup, Liveness та Readiness Probes

Сучасний хмарний стандарт оркестрації вимагає чіткого розмежування трьох зондів з принципово різною семантикою:

| Критерій | 1. Startup Probe | 2. Liveness Probe (`/alive`) | 3. Readiness Probe (`/health`) |
| :--- | :--- | :--- | :--- |
| **Семантичне питання** | *"Чи завершилась ініціалізація застосунку?"* | *"Чи живий внутрішній цикл виконання процесу CLR?"* | *"Чи готовий сервіс обробляти реальні бізнес-запити клієнтів?"* |
| **Що перевіряє?** | Міграції бази даних, розігрів кешів, JIT-компільовані структури. | Виключно стан процесу, пам'ять та цикл потоків (перевірка `"self"`). | Доступність бази даних, брокера RabbitMQ, обов'язкових зовнішніх API. |
| **Реакція оркестратора при збої** | Чекає до закінчення ліміту `failureThreshold`. При перевищенні — рестарт. | **Негайне примусове вбивство контейнера (SIGKILL / Container Restart).** | **Зняття контейнера з балансувальника навантаження (Remove from Ingress).** Контейнер НЕ вбивають! |
| **Тег у коді .NET** | `tags: ["startup"]` (у нашому коді окремий ендпоінт не замаплено) | `tags: ["live"]` | Усі зареєстровані перевірки (`/health` без предиката). |
| **Вплив на інфраструктуру** | Тимчасовий на етапі старту. | Захищає від безвихідних Deadlock. | Захищає користувачів від отримання помилок 502/503. |

---

<a id="section-4"></a>
## 4. Антипатерн "Спіраль смерті" (The Cascading Death Spiral)

Найнебезпечнішою помилкою інженерів є включення зовнішніх залежностей (бази даних SQL Server або черги RabbitMQ) до зонда **Liveness (`/alive`)**.

### Сценарій виникнення катастрофічної спіралі смерті:

```mermaid
sequenceDiagram
    autonumber
    participant DB as 💾 База даних SQL Server (Під високим навантаженням)
    participant Pod1 as 📦 Pod 1 (Driver Service)
    participant Pod2 as 📦 Pod 2 (Driver Service)
    participant Kube as ⚙️ Оркестратор (Kubelet)

    Note over DB: База перевантажена, CPU 100%, запити сповільнюються
    Kube->>Pod1: GET /alive (Неправильно: перевіряє БД!)
    Pod1->>DB: Ping DB (Таймаут!)
    Pod1-->>Kube: 503 Service Unavailable
    
    Note over Kube: Зонд /alive впав! Kubelet вбиває Pod 1!
    Kube->>Pod1: SIGKILL & Restart Pod 1
    
    Note over Pod2,DB: Трафік з вбитого Pod 1 падає на вцілілий Pod 2!
    Note over DB: Навантаження на БД зростає ще більше!
    Kube->>Pod2: GET /alive (Таймаут БД!)
    Pod2-->>Kube: 503 Service Unavailable
    
    Note over Kube: Зонд /alive впав! Kubelet вбиває Pod 2!
    Kube->>Pod2: SIGKILL & Restart Pod 2
    
    Note over Pod1,Pod2: Катастрофа: ВСІ поди безперервно рестартують у циклі! Кластер ліг!
```

### Чому це катастрофа?
1. Перезапуск процесу C# не може полагодити перевантажену базу даних!
2. Навпаки, коли 20 щойно перезапущених контейнерів одночасно піднімаються, вони ініціалізують пули з'єднань, надсилають важкі запити перевірки міграцій та JIT-компіляції, остаточно вбиваючи базу даних.

> 🛑 **Золоте правило архітектури:**  
> **Liveness Probe (`/alive`) ніколи, за жодних обставин не повинен перевіряти зовнішні мережеві ресурси!**  
> Доступність бази даних перевіряється **виключно у Readiness Probe (`/health`)**. При збої бази даних контейнер просто перестає приймати трафік, але спокійно очікує відновлення БД без самознищення.

---

<a id="section-5"></a>
## 5. Архітектурна реалізація у ServiceDefaults: Теги, предикати та конфігурація маршрутів

Усі перевірки централізовані у спільному проєкті `CargoDelivery.ServiceDefaults`.

> 📍 **Вихідний код:** [Extensions.cs#L214](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs#L214) (реєстрація) • [Extensions.cs#L248](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs#L248) (маршрути)

### 1. Реєстрація базового зонда життя процесу

```csharp
public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) 
    where TBuilder : IHostApplicationBuilder
{
    builder.Services.AddHealthChecks()
        // Реєструємо легковажну перевірку "self" з тегом "live".
        // Вона повертає Healthy(), якщо внутрішній цикл Kestrel здатний прийняти та обробити запит.
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

    return builder;
}
```

### 2. Маршрутизація з фільтрацією за предикатами

```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    // У Development-середовищі мапимо ендпоінти для моніторингу через Aspire Dashboard
    if (app.Environment.IsDevelopment())
    {
        // 1. Ендпоінт /health (Readiness Probe):
        // За замовчуванням виконує ВСІ зареєстровані перевірки (self + БД + RabbitMQ + зовнішні API)
        app.MapHealthChecks("/health");

        // 2. Ендпоінт /alive (Liveness Probe):
        // Використовує Predicate для запуску ВИКЛЮЧНО перевірок з тегом "live".
        // Жодна зовнішня залежність сюди не потрапляє!
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }

    return app;
}
```

---

<a id="section-6"></a>
## 6. Проєктування перевірок для зовнішніх залежностей (БД, RabbitMQ, HTTP)

Для повноцінної перевірки готовності мікросервісів обробляти бізнес-запити до конвеєра `builder.Services.AddHealthChecks()` додаються спеціалізовані перевірки.

> ⚖️ **Поточний стан проєкту.** Зараз у сервісах зареєстрована лише перевірка `self` (з ServiceDefaults), а Craftsman-сервіси додатково викликають порожній `builder.Services.AddHealthChecks()` ([DriversManagement WebAppServiceConfiguration.cs#L45](../DriversManagement/src/DriversManagement/Extensions/Services/WebAppServiceConfiguration.cs#L45)) і мапять старий маршрут `api/health` ([Program.cs#L56](../DriversManagement/src/DriversManagement/Program.cs#L56)). Отже, `/health` поки **не** перевіряє БД і брокер. Приклади нижче — це рекомендоване розширення, яке ви можете додати як практичне завдання.

### Варіант А: перевірка через EF Core DbContext (без додаткових пакетів, крім одного)

```csharp
// NuGet: Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
builder.Services.AddHealthChecks()
    // Виконує DbContext.Database.CanConnectAsync() — реальне з'єднання з SQL Server.
    // Без тегу "live" → потрапить у /health (Readiness), але НЕ в /alive (Liveness).
    .AddDbContextCheck<DriversManagementContext>(
        name: "sqlserver-driversmanagement",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready", "db"]);
```

### Варіант Б: спеціалізовані перевірки з бібліотеки AspNetCore.HealthChecks.*

```csharp
// NuGet: AspNetCore.HealthChecks.SqlServer, AspNetCore.HealthChecks.Rabbitmq
builder.Services.AddHealthChecks()
    // Відкриває з'єднання та виконує легкий SELECT 1
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DriversManagement")!,
        name: "sqlserver_check",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready", "db"],
        timeout: TimeSpan.FromSeconds(3))
    
    // Перевірка доступності брокера повідомлень RabbitMQ
    // (точна сигнатура залежить від версії пакета; у нових версіях приймає фабрику IConnection)
    .AddRabbitMQ(
        name: "rabbitmq_check",
        failureStatus: HealthStatus.Degraded, // Якщо брокер впав, HTTP API все ще може обслуговувати читання!
        tags: ["ready", "broker"],
        timeout: TimeSpan.FromSeconds(2));
```

### Варіант В: клієнтські інтеграції Aspire
Пакети на кшталт `Aspire.Microsoft.EntityFrameworkCore.SqlServer` (`builder.AddSqlServerDbContext<T>("name")`) або `Aspire.RabbitMQ.Client` реєструють DbContext/клієнт **разом** із health check, OpenTelemetry-інструментацією та повторами. У нашому проєкті DbContext реєструється «по-Craftsman-ському» (`AddDbContext` у `InfrastructureServiceExtension`), тому перевірки треба додавати вручну (варіанти А/Б).

> 💡 **Чому для RabbitMQ — `Degraded`, а не `Unhealthy`?** Якщо брокер недоступний, сервіс не зможе публікувати події, але може відповідати на запити читання. Позначка `Degraded` залишає інстанс у балансуванні (HTTP 200) і водночас підсвічує проблему в дашборді. Рішення залежить від домену: для сервісу, весь сенс якого — обробка черги, правильніше `Unhealthy`.

### Три статуси перевірки здоров'я (`HealthStatus`):
* `HealthStatus.Healthy` (HTTP 200) — підсистема повністю функціональна.
* `HealthStatus.Degraded` (HTTP 200) — підсистема працює з підвищеними затримками або відмовою другорядного компонента (наприклад, кешу Redis). Балансувальник не відключає сервіс, але інженери бачать попередження в дашборді.
* `HealthStatus.Unhealthy` (HTTP 503) — критична підсистема недоступна. Сервіс негайно ізолюється від клієнтського трафіку.

---

<a id="section-7"></a>
## 7. Стандартизація формату відповіді зондів: Простий текст проти IETF JSON

За замовчуванням виклик `MapHealthChecks("/health")` повертає клієнту плоский текстовий рядок: `Healthy` або `Unhealthy` з відповідним статус-кодом.

Однак для великих розподілених систем стандартом де-факто є повернення структурованого JSON-звіту згідно з чернеткою стандарту IETF (*Health Check Response Format for HTTP APIs*):

```csharp
app.MapHealthChecks("/health/details", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                data = e.Value.Data,
                exception = e.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsJsonAsync(payload);
    }
});
```

### Приклад згенерованого детального звіту JSON:

```json
{
  "status": "Unhealthy",
  "totalDurationMs": 312.4,
  "entries": [
    {
      "name": "self",
      "status": "Healthy",
      "durationMs": 0.2,
      "description": null,
      "exception": null
    },
    {
      "name": "sqlserver_check",
      "status": "Unhealthy",
      "durationMs": 310.1,
      "description": "A network-related or instance-specific error occurred while establishing a connection to SQL Server.",
      "exception": "Microsoft.Data.SqlClient.SqlException: ... (provider: TCP Provider, error: 40 - Could not open a connection to SQL Server)"
    }
  ]
}
```

---

<a id="section-8"></a>
## 8. Діаграма станів життєвого циклу контейнера під контролем зондів

```mermaid
stateDiagram-v2
    [*] --> Init : Старт процесу dotnet
    
    state StartupPhase {
        Init --> WarmingUp : Запуск Kestrel & DI
        WarmingUp --> StartupPassed : Startup Probe 200 OK
        WarmingUp --> StartupFailed : Startup Probe Timeout
    }
    
    StartupFailed --> ContainerKilled : Kubelet видаляє процес
    ContainerKilled --> [*]

    state RunningPhase {
        StartupPassed --> LivenessChecking
        
        state LivenessChecking {
            Alive : Процес відповідає на /alive (200 OK)
            Deadlocked : Потік завис у Deadlock / Starvation
        }

        state ReadinessChecking {
            Ready : Залежності доступні (/health 200 OK)
            Unready : БД або брокер лежить (/health 503)
        }
    }

    Deadlocked --> ContainerRestarted : /alive впав -> Рестарт контейнера
    ContainerRestarted --> [*]

    Ready --> IngressActive : Маршрутизація користувацького трафіку включена
    Unready --> IngressDrained : Трафік знято з поду (Drain Traffic)
    IngressDrained --> Ready : БД ожила -> Трафік повернено автоматично
```

---

<a id="section-9"></a>
## 9. Автоматизоване тестування зондів у CargoDelivery.HttpAggregator.Tests

Для гарантії того, що зонди здоров'я завжди сконфігуровані коректно і не зламаються при зміні залежностей або рефакторингу, у тестовому проєкті реалізовано автоматизовані функціональні тести:

> 📍 **Вихідний код:** [HttpAggregatorEndpointTests.cs](../CargoDelivery.HttpAggregator.Tests/FunctionalTests/HttpAggregatorEndpointTests.cs) — `/health` [#L18](../CargoDelivery.HttpAggregator.Tests/FunctionalTests/HttpAggregatorEndpointTests.cs#L18), `/alive` [#L30](../CargoDelivery.HttpAggregator.Tests/FunctionalTests/HttpAggregatorEndpointTests.cs#L30), `/api/simulation/ok` [#L42](../CargoDelivery.HttpAggregator.Tests/FunctionalTests/HttpAggregatorEndpointTests.cs#L42)

```csharp
public sealed class HttpAggregatorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HttpAggregatorEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Створює тестовий клієнт з конфігурацією інфраструктури в пам'яті (In-Memory TestServer)
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReadinessProbe_Returns200Ok()
    {
        // Act: запит до readiness зонда
        var response = await _client.GetAsync("/health");

        // Assert: перевірка, що статус-код 200 OK та контент містить 'Healthy'
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task AliveEndpoint_LivenessProbe_Returns200Ok()
    {
        // Act: запит до liveness зонда
        var response = await _client.GetAsync("/alive");

        // Assert: перевірка базової працездатності процесу
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }
}
```

### Чому ці тести критичні?
* Вони виконуються за частки секунди (увесь набір `CargoDelivery.HttpAggregator.Tests` з 6 тестів — ≈0.4 с), бо `WebApplicationFactory` піднімає застосунок у пам'яті (`TestServer`) без реальних сокетів.
* `WebApplicationFactory<Program>` за замовчуванням запускає застосунок у середовищі **Development** — саме тому `MapDefaultEndpoints()` мапить `/health` і `/alive` (умова `IsDevelopment()`), і тести їх бачать.
* `OTEL_EXPORTER_OTLP_ENDPOINT` у тестах не задано, тому OTLP-експортер не реєструється ([гайд 02, §7](02_ServiceDefaults_Architecture_And_Parameters.md#section-7)) і тести не намагаються з'єднатися з дашбордом.
* Якщо будь-який розробник помилково видалить `.AddDefaultHealthChecks()` або зламає `MapDefaultEndpoints()`, білд негайно впаде ще до потрапляння в репозиторій.

---

<a id="section-10"></a>
## 10. Архітектурна матриця: Антипатерни vs Еталонні практики проектування зондів

| Область проектування | ❌ Небезпечний антипатерн | 🟢 Еталонна хмарна практика |
| :--- | :--- | :--- |
| **Склад зонда Liveness** | Перевірка бази даних або зовнішнього шлюзу всередині `/alive`. | Зонд `/alive` перевіряє **тільки локальний стан процесу** (перевірка `"self"`). |
| **Реакція на відмову бази даних** | Сервіс падає з перезапуском контейнера. | Зонд `/health` повертає 503, балансувальник знімає трафік, процес залишається живим. |
| **Таймаути зондів (Probe Timeout)** | Зонд очікує відповіді БД 30 секунд (зависання черги опитування Kubelet). | Жорсткий таймаут зонда: 1-3 секунди за допомогою `timeout: TimeSpan.FromSeconds(2)`. |
| **Фільтрація перевірок** | Всі типи перевірок злиті в єдиний ендпоінт без використання тегів. | Чітка сегрегація за тегами: `tags: ["live"]` для Liveness, `tags: ["ready"]` для Readiness. |
| **Формат звітності** | Неінформативний рядок тексту, що не дає зрозуміти, яка саме з 5 баз даних упала. | Структурований JSON з деталями кожної перевірки (`name`, `duration`, `exception`). |
| **Режим Degraded** | Будь-яка некритична помилка (наприклад, недоступність кешу Redis) призводить до статусу Unhealthy. | Використання `HealthStatus.Degraded`, що дозволяє сервісу продовжувати обслуговування з дещо меншою швидкістю. |

---

<a id="section-11a"></a>
## 11. Як Aspire використовує /health: WithHttpHealthCheck, WaitFor та колонка Health

Aspire сам по собі **не знає**, що всередині .NET-проєкту є ендпоінт `/health`. Зв'язок задається в AppHost:

```csharp
var driversApi = builder.AddProject<Projects.DriversManagement>("driversmanagement")
    // ...
    .WithHttpHealthCheck("/health")   // 👈 Aspire періодично викликає GET /health цього ресурсу
    .WaitFor(driversDb);
```
> 📍 [AppHost.cs#L101](../Aspire/CargoDelivery.AppHost/AppHost.cs#L101) (а також [#L66](../Aspire/CargoDelivery.AppHost/AppHost.cs#L66), [#L81](../Aspire/CargoDelivery.AppHost/AppHost.cs#L81), [#L120](../Aspire/CargoDelivery.AppHost/AppHost.cs#L120))

| Що відбувається | Де це видно |
| :--- | :--- |
| Aspire реєструє health check з іменем на кшталт `driversmanagement_https_/health_200_check` (очікує код 200 від HTTPS-ендпоінта) | Деталі ресурсу → секція *Health checks* |
| Результат перевірки визначає стан здоров'я ресурсу (`Healthy` / `Unhealthy`) | Іконка стану в колонці *State* вкладки *Resources* |
| Залежні ресурси з `.WaitFor(driversApi)` стартують лише після `Healthy` | `httpaggregator` перебуває в стані `Waiting`, доки всі три сервіси не стануть Healthy |

```mermaid
sequenceDiagram
    autonumber
    participant Host as 🎯 Aspire (DCP + AppHost)
    participant DM as 🚚 DriversManagement
    participant Agg as 🛡️ HttpAggregator

    Host->>DM: Старт процесу
    Note over Agg: State = Waiting (WaitFor driversmanagement)
    loop Кожні кілька секунд
        Host->>DM: GET https://localhost:5001/health
        DM-->>Host: 503 / немає відповіді (ще стартує, міграції)
    end
    Host->>DM: GET /health
    DM-->>Host: 200 Healthy
    Host->>Agg: Старт процесу (WaitFor знято)
```

> ⚠️ **Без `WithHttpHealthCheck`** проєкт вважається здоровим, щойно процес запущено, — тобто `WaitFor` захищав би лише від «процес ще не створено», але не від «Kestrel ще не слухає порт» чи «міграції ще йдуть». Раніше AppHost у проєкті був саме таким, хоча документація обіцяла очікування `/health`.

> 🔁 **Два маршрути здоров'я в Craftsman-сервісах.** Окрім `/health` і `/alive` із ServiceDefaults, шаблон Craftsman мапить ще `api/health` ([Program.cs#L56](../DriversManagement/src/DriversManagement/Program.cs#L56)). Він виконує ті самі перевірки, що й `/health`, але відкритий у будь-якому середовищі. Aspire використовує саме `/health`.

---

<a id="section-11"></a>
## 12. Контрольні запитання та лабораторні вправи

### 🎯 Контрольні запитання:
1. Опишіть фізичний механізм виникнення «Спіралі смерті» (Cascading Death Spiral) при додаванні перевірки SQL Server до зонда Liveness.
2. Чим відрізняється поведінка балансувальника трафіку (Ingress/Kube-Proxy) при отриманні статусу `Unhealthy` від зонда Readiness порівняно зі статусом `Degraded`?
3. Чому ендпоінти діагностики здоров'я у файлі `Extensions.cs` мапляться виключно за умови `if (app.Environment.IsDevelopment())`, або за наявності захисту вхідного периметра в продакшні?
4. Які переваги надає використання класу `WebApplicationFactory<Program>` для тестування Health Checks порівняно з ручним запуском сервісу через консоль?
5. Чому `.WaitFor(project)` без `.WithHttpHealthCheck("/health")` не захищає від помилки `Connection refused` на старті?

### 🛠️ Практичне лабораторне завдання:
1. Запустіть проєкт `CargoDelivery.AppHost`.
2. Відкрийте браузер (або [CargoDelivery.HttpAggregator.http](../CargoDelivery.HttpAggregator/CargoDelivery.HttpAggregator.http), запит 1️⃣3️⃣) і надішліть GET-запити на:
   * `https://localhost:57390/alive`
   * `https://localhost:57390/health`
3. Перевірте стан здоров'я ресурсів у вкладці **Resources** та секцію *Health checks* у деталях ресурсу `httpaggregator` в **Aspire Dashboard**.
4. Додайте в DriversManagement перевірку `AddDbContextCheck<DriversManagementContext>()` (варіант А з §6), перезапустіть AppHost і переконайтеся, що вона з'явилась у `/health`, але не в `/alive`.
5. Створіть власну перевірку здоров'я `SlowDependencyHealthCheck`, яка штучно затримує відповідь на 4 секунди, та протестуйте, як це впливає на стан ресурсу в дашборді і на старт залежних сервісів.
