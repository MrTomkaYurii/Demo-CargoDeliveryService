using DriversBlogManagement;
using DriversBlogManagement.Domain.Drivers.Services;
using Grpc.Core;
using System.Threading.Tasks;

public class DriverRpcService : DriverRpc.DriverRpcBase
{
    private readonly DriverRepository _DriverRepository;

    public DriverRpcService(DriverRepository DriverRepository)
    {
        _DriverRepository = DriverRepository;
    }

    public override async Task<DriverWithPostsResponse> GetDriverWithPosts(DriverWithPostsRequest request, ServerCallContext context)
    {
        try
        {
            var driverId = Guid.Parse(request.DriverId);
            var driverWithPostsDto = await _DriverRepository.GetDriverWithPosts(driverId);

            var response = new DriverWithPostsResponse
            {
                DriverId = driverWithPostsDto.DriverId.ToString(),
                FirstName = driverWithPostsDto.FirstName,
                LastName = driverWithPostsDto.LastName
            };

            foreach (var post in driverWithPostsDto.Posts)
            {
                response.Posts.Add(new PostAboutDriverResponse
                {
                    Id = post.Id.ToString(),
                    Title = post.Title,
                    Content = post.Content
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }
}
