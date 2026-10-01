namespace DriversManagement.Domain.Drivers.Features;

using DriversManagement.Domain.Drivers.Dtos;
using DriversManagement.Domain.Drivers.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;
using DriversBlogManagement;

public static class GetDriver
{
    public sealed record Query(Guid DriverId) : IRequest<FullDataAboutDriverAndPostsDto>;

    public sealed class Handler : IRequestHandler<Query, FullDataAboutDriverAndPostsDto>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IHeimGuardClient _heimGuard;
        private readonly DriverRpc.DriverRpcClient _client;

        public Handler(IDriverRepository driverRepository, 
            IHeimGuardClient heimGuard,
            DriverRpc.DriverRpcClient client)
        {
            _driverRepository = driverRepository;
            _heimGuard = heimGuard;
            _client = client;
        }

        public async Task<FullDataAboutDriverAndPostsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadDriver);

            FullDataAboutDriverAndPostsDto result = new FullDataAboutDriverAndPostsDto();

            Driver drv = await _driverRepository.GetById(request.DriverId, cancellationToken: cancellationToken);
            DriverDto drvDTO = drv.ToDriverDto();

            result.Driver = drvDTO;

            // Отправка запроса через gRPC
            // var grpcRequest = new DriverWithPostsRequest { driver_id = request.DriverId.ToString() };
            DriverWithPostsResponse grpcResponse = await _client.GetDriverWithPostsAsync(grpcRequest);

            /// result.Posts = grpcResponse.Posts.ToList();

            return result;
        }
    }
}