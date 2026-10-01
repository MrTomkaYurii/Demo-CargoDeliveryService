namespace DriversManagement.Domain.Drivers.Features;

using DriversManagement.Domain.Drivers.Dtos;
using DriversManagement.Domain.Drivers.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;
using DriversBlogManagement;
using Grpc.Core;
using System.Linq;

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

            // Виклик віддаленого мікросервісу через синхронний gRPC контракт
            try
            {
                var grpcRequest = new DriverWithPostsRequest { DriverId = request.DriverId.ToString() };
                DriverWithPostsResponse grpcResponse = await _client.GetDriverWithPostsAsync(grpcRequest, cancellationToken: cancellationToken);

                if (grpcResponse?.Posts != null)
                {
                    result.Posts = grpcResponse.Posts.Select(p => new DriversBlogManagement.Domain.PostAboutDrivers.Dtos.PostAboutDriverDto
                    {
                        Id = Guid.TryParse(p.Id, out var postId) ? postId : Guid.Empty,
                        Title = p.Title,
                        Content = p.Content
                    }).ToList();
                }
            }
            catch (RpcException)
            {
                // Патерн стійкості (Graceful Degradation): якщо мікросервіс блогів недоступний,
                // повертаємо дані водія з порожнім списком постів, уникаючи відмови всієї операції
                result.Posts = new List<DriversBlogManagement.Domain.PostAboutDrivers.Dtos.PostAboutDriverDto>();
            }

            return result;
        }
    }
}
