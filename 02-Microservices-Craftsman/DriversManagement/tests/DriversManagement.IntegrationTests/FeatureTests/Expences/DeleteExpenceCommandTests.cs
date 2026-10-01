namespace DriversManagement.IntegrationTests.FeatureTests.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.Domain.Expences.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteExpenceCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_expence_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var expence = new FakeExpenceBuilder().Build();
        await testingServiceScope.InsertAsync(expence);

        // Act
        var command = new DeleteExpence.Command(expence.Id);
        await testingServiceScope.SendAsync(command);
        var expenceResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Expences
                .CountAsync(e => e.Id == expence.Id));

        // Assert
        expenceResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_expence_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteExpence.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_expence_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var expence = new FakeExpenceBuilder().Build();
        await testingServiceScope.InsertAsync(expence);

        // Act
        var command = new DeleteExpence.Command(expence.Id);
        await testingServiceScope.SendAsync(command);
        var deletedExpence = await testingServiceScope.ExecuteDbContextAsync(db => db.Expences
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == expence.Id));

        // Assert
        deletedExpence?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteExpence);

        // Act
        var command = new DeleteExpence.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}