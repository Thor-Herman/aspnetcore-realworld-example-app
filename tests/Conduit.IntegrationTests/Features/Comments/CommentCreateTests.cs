using System.Net;
using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Comments.Commands;
using Conduit.Application.Features.Comments.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Domain.Entities;
using Conduit.Presentation.Controllers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Comments;

public class CommentCreateTests : TestBase
{
    public CommentCreateTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> InvalidComments()
    {
        yield return new object[]
        {
            new NewCommentDto
            {
                Body = "",
            }
        };
    }

    [Theory, MemberData(nameof(InvalidComments))]
    public async Task Cannot_Create_Comment_With_Invalid_Data(NewCommentDto comment)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleCommand(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Act(HttpMethod.Post, "/articles/test-title/comments", new NewCommentRequest(comment));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cannot_Create_Comment_To_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Post, "/articles/test-title/comments", new NewCommentRequest(
            new NewCommentDto
            {
                Body = "Test Body",
            }
        ));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Guest_Cannot_Create_Comment()
    {
        var response = await Act(HttpMethod.Post, "/articles/test-title/comments", new NewCommentRequest(
            new NewCommentDto()
        ));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Create_Comment()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        await Mediator.Send(new NewArticleCommand(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Act<SingleCommentResponse>(HttpMethod.Post, "/articles/test-title/comments", new NewCommentRequest(new NewCommentDto
        {
            Body = "Thank you !",
        }));

        response.Comment.Should().BeEquivalentTo(new CommentDto
        {
            Body = "Thank you !",
            Author = new ProfileDto
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
        }, options => options.Excluding(x => x.Id).Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

        (await Context.Comments.AnyAsync()).Should().BeTrue();
    }
}