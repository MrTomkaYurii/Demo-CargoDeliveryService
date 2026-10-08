namespace DriversManagement.Domain.Drivers.Features;

using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Drivers.Dtos;
using DriversManagement.Domain.Drivers.Services;
using DriversManagement.Services;
using DriversManagement.Domain.Drivers.Models;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;
using MassTransit;

public static class UpdateDriver
{
    public sealed record Command(Guid DriverId, DriverForUpdateDto UpdatedDriverData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDriverRepository driverRepository, 
            IUnitOfWork unitOfWork, 
            IHeimGuardClient heimGuard, 
            IPublishEndpoint publishEndpoint)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// ====================================================================================
        /// 📬 ОБРОБНИК КОМАНДИ ОНОВЛЕННЯ ВОДІЯ З АСИНХРОННОЮ ПУБЛІКАЦІЄЮ В RABBITMQ
        /// ------------------------------------------------------------------------------------
        /// 1. ВАЛІДАЦІЯ ТА АВТОРИЗАЦІЯ:
        ///    Перевіряємо дозвіл 'CanUpdateDriver' через HeimGuard.
        /// 2. ЛОКАЛЬНА ТРАНЗАКЦІЯ:
        ///    Оновлюємо агрегат Driver у локальній базі DriversManagement та фіксуємо
        ///    зміни через _unitOfWork.CommitChanges.
        /// 3. ПАТЕРН EVENT-DRIVEN NOTIFICATION (Асинхронне сповіщення підписників):
        ///    Публікуємо SharedKernel.Messages.IDriverUpdated через IPublishEndpoint.
        ///    MassTransit спрямовує повідомлення у RabbitMQ Fanout Exchange 'driver-updated',
        ///    який миттєво дублює його у черги:
        ///      - sync-updated-driver (DriversBlogManagement)
        ///      - ordering-sync-driver (CargoOrderingService)
        /// 4. EVENTUAL CONSISTENCY (Кінцева узгодженість):
        ///    Всі сервіси отримують новий стан водія асинхронно, без блокування клієнтського запиту.
        /// ====================================================================================
        /// </summary>
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateDriver);

            var driverToUpdate = await _driverRepository.GetById(request.DriverId, cancellationToken: cancellationToken);
            var driverToAdd = request.UpdatedDriverData.ToDriverForUpdate();
            driverToUpdate.Update(driverToAdd);

            _driverRepository.Update(driverToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);

            // Публікуємо подію оновлення після успішного збереження в БД
            var message = new SharedKernel.Messages.DriverUpdated
            {
                DriverId = request.DriverId,
                FirstName = driverToAdd.FirstName,
                LastName = driverToAdd.LastName
            };

            await _publishEndpoint.Publish<SharedKernel.Messages.IDriverUpdated>(message, cancellationToken);
        }
    }
}
