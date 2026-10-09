namespace DriversManagement.Domain.Drivers.Features;

using DriversManagement.Domain.Drivers.Services;
using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Drivers.Dtos;
using DriversManagement.Domain.Drivers.Models;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;
using MassTransit;
using SharedKernel.Messages;

/// <summary>
/// ====================================================================================
/// 📬 CQRS FEATURE: ADD DRIVER (EVENT-DRIVEN ARCHITECTURE PUBLISHER)
/// ------------------------------------------------------------------------------------
/// АРХІТЕКТУРНА КОНЦЕПЦІЯ:
///
/// 1. ВЕРТИКАЛЬНИЙ ЗРІЗ (VERTICAL SLICE ARCHITECTURE):
///    Вся логіка створення водія (команда Command, обробник Handler, валідація,
///    робота з БД та публікація в шину) зібрана в одному ізольованому файлі фічі.
///
/// 2. ЧОМУ "_publishEndpoint.Publish" А НЕ "_sendEndpoint.Send"?
///    - Send (Point-to-Point): відправляє повідомлення в ОДНУ конкретну чергу.
///      Вимагає, щоб видавець знав точну чергу отримувача (жорсткий зв'язок).
///    - Publish (Publish/Subscribe): публікує подію в Fanout Exchange (SharedKernel.Messages:IDriverCreated).
///      RabbitMQ автоматично робить копію повідомлення для ВСІХ черг сервісів,
///      які підписалися на цей контракт (DriversBlogManagement, CargoOrderingService, тощо).
///      Видавець взагалі не знає, хто саме і скільки споживачів слухають цю подію!
///
/// 3. ПОСЛІДОВНІСТЬ: СПОЧАТКУ БАЗА ДАНИХ, ПОТІМ ШИНА (ACID + EVENTUAL CONSISTENCY):
///    Рядок 1: await _unitOfWork.CommitChanges(cancellationToken) -> зберігає водія в БД.
///    Рядок 2: await _publishEndpoint.Publish(...) -> відправляє факт створення в брокер.
///    Це гарантує: ми ніколи не повідомимо інші сервіси про сутність, яка не змогла
///    зберегтися в нашій первинній базі.
/// ====================================================================================
/// </summary>
public static class AddDriver
{
    /// <summary>
    /// Незмінна CQRS-команда на створення водія.
    /// </summary>
    public sealed record Command(DriverForCreationDto DriverToAdd) : IRequest<DriverDto>;

    /// <summary>
    /// Обробник команди зі збереженням агрегату та публікацією події через MassTransit.
    /// </summary>
    public sealed class Handler : IRequestHandler<Command, DriverDto>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;
        private readonly IPublishEndpoint _publishEndpoint;

        public Handler(
            IDriverRepository driverRepository, 
            IUnitOfWork unitOfWork, 
            IHeimGuardClient heimGuard,
            IPublishEndpoint publishEndpoint)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<DriverDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // Перевірка прав користувача (RBAC) через HeimGuard
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddDriver);

            // КРОК 1: Створення багатого доменного агрегату (Rich Domain Model)
            var driverToAdd = request.DriverToAdd.ToDriverForCreation();
            var driver = Driver.Create(driverToAdd);

            // КРОК 2: Збереження сутності в локальній базі даних через Unit of Work (ACID транзакція)
            await _driverRepository.Add(driver, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            // КРОК 3: Публікація доменної події в шину повідомлень (MassTransit + RabbitMQ)
            // Використовуємо спільний контракт IDriverCreated із SharedKernel
            await _publishEndpoint.Publish<IDriverCreated>(new DriverCreated
            {
                DriverId = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                DriverLicenseNumber = driver.DriverLicenseNumber,
                Phone = driver.Phone
            }, cancellationToken);

            return driver.ToDriverDto();
        }
    }
}
