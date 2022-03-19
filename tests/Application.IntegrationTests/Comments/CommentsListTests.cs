using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.IntegrationTests.Comments;

public class CommentsListTests : TestBase
{
    [Test]
    public async Task Cannot_List_All_Comments_Of_Non_Existent_Article()
    {
        await this.Invoking(x => x.Act(new CommentsListQuery("test-title")))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Can_List_All_Comments_Of_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var comments = new List<string>();

        for (int i = 1; i <= 5; i++)
        {
            comments.Add($"Test Comment {i}");
        }

        foreach (var c in comments)
        {
            await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = $"This is John, {c} !",
            }));
        }

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        foreach (var c in comments)
        {
            await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = $"This is Jane, {c} !",
            }));
        }

        var response = await Act(new CommentsListQuery("test-title"));

        response.Comments.Count().Should().Be(10);

        response.Comments.First().Should().BeEquivalentTo(new CommentDTO
        {
            Body = "This is Jane, Test Comment 5 !",
            Author = new ProfileDTO
            {
                Username = "Jane Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
        }, options => options.Excluding(x => x.Id).Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));
    }
}