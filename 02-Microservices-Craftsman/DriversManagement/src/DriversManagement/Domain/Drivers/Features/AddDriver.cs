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

public static class AddDriver
{
    public sealed record Command(DriverForCreationDto DriverToAdd) : IRequest<DriverDto>;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddDriver);

            var driverToAdd = request.DriverToAdd.ToDriverForCreation();
            var driver = Driver.Create(driverToAdd);

            await _driverRepository.Add(driver, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            // ПАТЕРН EVENT-DRIVEN ARCHITECTURE (EDA / Pub-Sub через MassTransit):
            // Публікуємо подію в брокер повідомлень (RabbitMQ fanout exchange) 
            // для синхронізації з іншими мікросервісами (Ordering, Blog)
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
