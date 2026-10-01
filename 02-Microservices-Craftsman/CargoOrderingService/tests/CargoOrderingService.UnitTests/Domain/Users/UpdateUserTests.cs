namespace CargoOrderingService.UnitTests.Domain.Users;

using CargoOrderingService.Domain.Users.DomainEvents;
using CargoOrderingService.Domain;
using CargoOrderingService.Domain.Users;
using CargoOrderingService.Resources;
using CargoOrderingService.Domain.Users.Models;
using CargoOrderingService.SharedTestHelpers.Fakes.User;
using Bogus;
using ValidationException = CargoOrderingService.Exceptions.ValidationException;

public class UpdateUserTests
{
    private readonly Faker _faker;

    public UpdateUserTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_user()
    {
        // Arrange
        var fakeUser = new FakeUserBuilder().Build();
        var updatedUser = new FakeUserForUpdate().Generate();
        
        // Act
        fakeUser.Update(updatedUser);

        // Assert
        fakeUser.Identifier.Should().Be(updatedUser.Identifier);
        fakeUser.FirstName.Should().Be(updatedUser.FirstName);
        fakeUser.LastName.Should().Be(updatedUser.LastName);
        fakeUser.Email.Value.Should().Be(updatedUser.Email);
        fakeUser.Username.Should().Be(updatedUser.Username);
    }
    
    [Fact]
    public void can_NOT_update_user_without_identifier()
    {
        // Arrange
        var fakeUser = new FakeUserBuilder().Build();
        var updatedUser = new FakeUserForUpdate().Generate();
        updatedUser.Identifier = null;
        var newUser = () => fakeUser.Update(updatedUser);

        // Act + Assert
        newUser.Should().Throw<ValidationException>();
    }
    
    [Fact]
    public void can_NOT_update_user_with_whitespace_identifier()
    {
        // Arrange
        var fakeUser = new FakeUserBuilder().Build();
        var updatedUser = new FakeUserForUpdate().Generate();
        updatedUser.Identifier = " ";
        var newUser = () => fakeUser.Update(updatedUser);

        // Act + Assert
        newUser.Should().Throw<ValidationException>();
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var fakeUser = new FakeUserBuilder().Build();
        var updatedUser = new FakeUserForUpdate().Generate();
        fakeUser.DomainEvents.Clear();
        
        // Act
        fakeUser.Update(updatedUser);

        // Assert
        fakeUser.DomainEvents.Count.Should().Be(1);
        fakeUser.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(UserUpdated));
    }
}