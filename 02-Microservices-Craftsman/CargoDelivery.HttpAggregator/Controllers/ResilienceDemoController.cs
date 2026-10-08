namespace CargoDelivery.HttpAggregator.Controllers;

using System.Diagnostics;
using CargoDelivery.HttpAggregator.Domain.Clients;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// ====================================================================================
/// 🛡️ RESILIENCE DEMO CONTROLLER: ІНТЕРАКТИВНА ДЕМОНСТРАЦІЯ POLLY У ASPIRE DASHBOARD
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Надає набір ендпоінтів для живої демонстрації політик стійкості у вебінтерфейсі
/// .NET Aspire Dashboard:
/// 1. /retry — демонстрація автоматичних повторів при тимчасових мережевих помилках 503.
/// 2. /timeout — демонстрація спрацьовування таймауту Polly та повернення fallback-значення.
/// 3. /circuit-breaker — перевірка поведінки відсікання навантаження при каскадних аваріях.
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/v1/resilience-demo")]
public sealed class ResilienceDemoController : ControllerBase
{
    private readonly IDriverBlogApiClient _blogApiClient;
    private readonly ILogger<ResilienceDemoController> _logger;

    public ResilienceDemoController(
        IDriverBlogApiClient blogApiClient,
        ILogger<ResilienceDemoController> logger)
    {
        _blogApiClient = blogApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Демонстрація політики Polly Retry:
    /// Звертається до тестового ресурсу, який навмисно падає з кодом 503,
    /// і показує, як клієнт автоматично повторює запити до досягнення успіху.
    /// </summary>
    [HttpGet("retry")]
    public async Task<ActionResult<ResilienceDemoResultDto>> DemonstrateRetry(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Початок тесту Polly Retry Policy...");

        try
        {
            var content = await _blogApiClient.ExecuteResilienceProbeAsync("flaky", cancellationToken);
            stopwatch.Stop();

            return Ok(new ResilienceDemoResultDto
            {
                Scenario = "Polly Automatic Retry (Exponential Backoff)",
                Status = "Success",
                Message = "Запит успішно виконано після відпрацювання повторних спроб Polly!",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = content
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Polly Retry вичерпав ліміт спроб.");

            return StatusCode(StatusCodes.Status504GatewayTimeout, new ResilienceDemoResultDto
            {
                Scenario = "Polly Automatic Retry",
                Status = "Exhausted",
                Message = "Політика Polly Retry вичерпала ліміт спроб і повернула збій.",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Демонстрація політики Polly Timeout:
    /// Звертається до ресурсу з затримкою, яка перевищує допустимий ліміт.
    /// </summary>
    [HttpGet("timeout")]
    public async Task<ActionResult<ResilienceDemoResultDto>> DemonstrateTimeout(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Початок тесту Polly Timeout Policy...");

        try
        {
            var content = await _blogApiClient.ExecuteResilienceProbeAsync("slow", cancellationToken);
            stopwatch.Stop();

            return Ok(new ResilienceDemoResultDto
            {
                Scenario = "Polly Timeout Check",
                Status = "Success",
                Message = "Запит виконано вчасно.",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = content
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Запит перервано політикою Polly Timeout. Застосовано захисний Fallback.");

            return Ok(new ResilienceDemoResultDto
            {
                Scenario = "Polly Timeout -> Fallback Activated",
                Status = "TimeoutHandled",
                Message = "Спрацював таймаут Polly. Запит успішно перехоплено, запобігши зависанню сокетів.",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Details = $"Виняток: {ex.GetType().Name} - {ex.Message}"
            });
        }
    }
}
