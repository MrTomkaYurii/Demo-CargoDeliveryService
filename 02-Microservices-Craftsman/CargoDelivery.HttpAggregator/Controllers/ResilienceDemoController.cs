namespace CargoDelivery.HttpAggregator.Controllers;

using System.Diagnostics;
using CargoDelivery.HttpAggregator.Domain.Clients;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.Timeout;

/// <summary>
/// ====================================================================================
/// 🛡️ RESILIENCE DEMO CONTROLLER: ІНТЕРАКТИВНА ДЕМОНСТРАЦІЯ POLLY У ASPIRE DASHBOARD
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Надає набір ендпоінтів для живої демонстрації політик стійкості. Кожен ендпоінт
/// робить РЕАЛЬНИЙ HTTP-виклик (через ISimulationApiClient) до SimulationController
/// цього ж агрегатора, тож поведінку Polly видно і у відповіді, і в Aspire Dashboard → Traces:
/// 1. /retry           — автоматичні повтори при тимчасових помилках 503.
/// 2. /timeout         — спрацювання таймауту спроби та повернення fallback-відповіді.
/// 3. /circuit-breaker — розмикання ланцюга при стабільній відмові (Fail-Fast).
///
/// ⚠️ Стан Circuit Breaker належить конвеєру Polly конкретного HttpClient-а.
/// Тому кожен сценарій має ВЛАСНИЙ іменований клієнт (SimulationClients.*): після
/// /circuit-breaker ланцюг розімкнений 15 с лише для цього сценарію, а /retry та
/// /timeout працюють незалежно. У реальній системі розмикач зазвичай один на
/// адресата (сервіс) — і тоді він захищає ВСІ виклики до проблемного сервісу.
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/v1/resilience-demo")]
public sealed class ResilienceDemoController : ControllerBase
{
    private readonly ISimulationApiClient _simulationClient;
    private readonly ILogger<ResilienceDemoController> _logger;

    public ResilienceDemoController(
        ISimulationApiClient simulationClient,
        ILogger<ResilienceDemoController> logger)
    {
        _simulationClient = simulationClient;
        _logger = logger;
    }

    // Адреса самого агрегатора, взята з поточного вхідного запиту (наприклад, https://localhost:57390/)
    private Uri SelfBaseAddress => new($"{Request.Scheme}://{Request.Host}/");

    /// <summary>
    /// Демонстрація політики Polly Retry:
    /// /flaky перші 2 рази повертає 503, а на 3-й спробі — 200 OK.
    /// Клієнт не бачить жодної помилки: повтори відпрацьовують "під капотом".
    /// </summary>
    [HttpGet("retry")]
    public async Task<ActionResult<ResilienceDemoResultDto>> DemonstrateRetry(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Початок тесту Polly Retry Policy...");

        try
        {
            var content = await _simulationClient.GetAsync(SimulationClients.Retry, SelfBaseAddress, "flaky?failUntil=2", cancellationToken);

            return Ok(new ResilienceDemoResultDto
            {
                Scenario = "Polly Automatic Retry (Exponential Backoff + Jitter)",
                Status = "Success",
                Message = "Запит успішно виконано після відпрацювання повторних спроб Polly!",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = content
            });
        }
        catch (Exception ex) when (ex is HttpRequestException or BrokenCircuitException or TimeoutRejectedException)
        {
            _logger.LogError(ex, "Polly Retry вичерпав ліміт спроб.");

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ResilienceDemoResultDto
            {
                Scenario = "Polly Automatic Retry",
                Status = "Exhausted",
                Message = "Політика Polly Retry вичерпала ліміт спроб (або ланцюг розімкнено) і повернула збій.",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = $"{ex.GetType().Name}: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Демонстрація політики Polly Timeout:
    /// /slow відповідає за 5 с, а AttemptTimeout демо-клієнта — 2 с.
    /// Кожна спроба обривається, Retry пробує знову, і загальний бюджет (8 с) закінчується.
    /// Замість зависання ми повертаємо контрольовану fallback-відповідь.
    /// </summary>
    [HttpGet("timeout")]
    public async Task<ActionResult<ResilienceDemoResultDto>> DemonstrateTimeout(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Початок тесту Polly Timeout Policy...");

        try
        {
            var content = await _simulationClient.GetAsync(SimulationClients.Timeout, SelfBaseAddress, "slow?delaySeconds=5", cancellationToken);

            return Ok(new ResilienceDemoResultDto
            {
                Scenario = "Polly Timeout Check",
                Status = "Success",
                Message = "Запит виконано вчасно.",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = content
            });
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogWarning(ex, "Запит перервано політикою Polly Timeout. Застосовано захисний Fallback.");

            // Fallback: повертаємо 200 з поясненням — клієнт отримав контрольовану відповідь
            // за ~8 с замість того, щоб чекати нескінченно на завислий сокет.
            return Ok(new ResilienceDemoResultDto
            {
                Scenario = "Polly Timeout -> Fallback Activated",
                Status = "TimeoutHandled",
                Message = "Спрацював таймаут Polly. Запит перехоплено, запобігши зависанню сокетів.",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = $"Виняток: {ex.GetType().Name} - {ex.Message}"
            });
        }
        catch (BrokenCircuitException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ResilienceDemoResultDto
            {
                Scenario = "Polly Timeout",
                Status = "CircuitOpen",
                Message = "Ланцюг розімкнений після попередніх збоїв — запит відхилено без виходу в мережу.",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Демонстрація Circuit Breaker:
    /// робимо серію логічних викликів до /error (завжди 503). Перші виклики реально
    /// йдуть у мережу (з повторами), а щойно частка збоїв перевищує поріг — розмикач
    /// переходить у стан Open, і наступні виклики падають МИТТЄВО з BrokenCircuitException.
    /// </summary>
    [HttpGet("circuit-breaker")]
    public async Task<ActionResult<ResilienceDemoResultDto>> DemonstrateCircuitBreaker(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var log = new List<string>();

        for (var call = 1; call <= 5; call++)
        {
            var callWatch = Stopwatch.StartNew();
            try
            {
                await _simulationClient.GetAsync(SimulationClients.CircuitBreaker, SelfBaseAddress, "error", cancellationToken);
                log.Add($"#{call}: 200 OK ({callWatch.ElapsedMilliseconds} мс)");
            }
            catch (BrokenCircuitException)
            {
                // Розмикач відхилив виклик без мережевого запиту — тому це займає ~0 мс
                log.Add($"#{call}: BrokenCircuitException — Fail-Fast ({callWatch.ElapsedMilliseconds} мс)");
            }
            catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException)
            {
                log.Add($"#{call}: {ex.GetType().Name} після повторів ({callWatch.ElapsedMilliseconds} мс)");
            }
        }

        return Ok(new ResilienceDemoResultDto
        {
            Scenario = "Polly Circuit Breaker (Closed -> Open)",
            Status = "Demonstrated",
            Message = "Перші виклики йдуть у мережу, наступні відсікаються розімкненим ланцюгом за ~0 мс. " +
                      "Через 15 с ланцюг перейде в Half-Open і пропустить пробний запит.",
            AttemptCount = log.Count,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            Details = string.Join(" | ", log)
        });
    }
}
