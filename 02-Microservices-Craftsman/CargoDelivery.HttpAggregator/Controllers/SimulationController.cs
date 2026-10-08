namespace CargoDelivery.HttpAggregator.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// ====================================================================================
/// 🧪 SIMULATION CONTROLLER: СИМУЛЯТОР МЕРЕЖЕВИХ ЗБОЇВ ТА ЗАТРИМОК ДЛЯ ASPIRE ДЕМО
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Надає керовані тестові ендпоінти для перевірки політик Polly у реальному часі:
/// 1. /slow — імітує мережевий таймаут (HttpResponseTimeout).
/// 2. /flaky — імітує тимчасовий збій (503 Service Unavailable), що виправляється на N-й спробі.
/// 3. /error — постійна помилка 500/503 для тестування розмикання Circuit Breaker.
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/simulation")]
public sealed class SimulationController : ControllerBase
{
    private static int _flakyAttemptCounter = 0;

    [HttpGet("slow")]
    public async Task<IActionResult> SimulateSlowCall([FromQuery] int delaySeconds = 5, CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
        return Ok(new { status = "Recovered", delayedBySeconds = delaySeconds });
    }

    [HttpGet("flaky")]
    public IActionResult SimulateFlakyCall([FromQuery] int failUntil = 2)
    {
        var current = Interlocked.Increment(ref _flakyAttemptCounter);
        if (current <= failUntil)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                attempt = current,
                error = "Тимчасовий збій сервісу (503 Service Unavailable). Очікується повтор від Polly."
            });
        }

        // Скидаємо лічильник для наступних тестів
        Interlocked.Exchange(ref _flakyAttemptCounter, 0);
        return Ok(new
        {
            attempt = current,
            message = "Успіх після повторних спроб Polly!"
        });
    }

    [HttpGet("error")]
    public IActionResult SimulatePersistentError()
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            error = "Критична помилка сервера (503 Service Unavailable) для розмикання Circuit Breaker."
        });
    }

    [HttpGet("ok")]
    public IActionResult SimulateSuccess()
    {
        return Ok("Healthy");
    }
}
