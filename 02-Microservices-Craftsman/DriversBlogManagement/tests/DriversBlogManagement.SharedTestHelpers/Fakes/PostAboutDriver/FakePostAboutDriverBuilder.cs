namespace DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;

using DriversBlogManagement.Domain.PostAboutDrivers;
using DriversBlogManagement.Domain.PostAboutDrivers.Models;

public class FakePostAboutDriverBuilder
{
    private PostAboutDriverForCreation _creationData = new FakePostAboutDriverForCreation().Generate();

    public FakePostAboutDriverBuilder WithModel(PostAboutDriverForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakePostAboutDriverBuilder WithTitle(string title)
    {
        _creationData.Title = title;
        return this;
    }
    
    public FakePostAboutDriverBuilder WithContent(string content)
    {
        _creationData.Content = content;
        return this;
    }
    
    public PostAboutDriver Build()
    {
        var result = PostAboutDriver.Create(_creationData);
        return result;
    }
}