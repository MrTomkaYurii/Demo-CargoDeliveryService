using DriversBlogManagement.Domain.Users;
using DriversBlogManagement.Domain.PostAboutDrivers;
using DriversBlogManagement.Domain.Comments.Models;
using DriversBlogManagement.Domain.Comments.DomainEvents;

namespace DriversBlogManagement.Domain.Comments;
public class Comment : BaseEntity
{
    public string Text { get; private set; }

    public PostAboutDriver PostAboutDriver { get; private set; }

    public User User { get; private set; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Comment Create(CommentForCreation commentForCreation)
    {
        var newComment = new Comment();

        newComment.Text = commentForCreation.Text;

        newComment.QueueDomainEvent(new CommentCreated(){ Comment = newComment });
        
        return newComment;
    }

    public Comment Update(CommentForUpdate commentForUpdate)
    {
        Text = commentForUpdate.Text;

        QueueDomainEvent(new CommentUpdated(){ Id = Id });
        return this;
    }

    public Comment SetPostAboutDriver(PostAboutDriver postAboutDriver)
    {
        PostAboutDriver = postAboutDriver;
        return this;
    }

    public Comment SetUser(User user)
    {
        User = user;
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Comment() { } // For EF + Mocking
}
