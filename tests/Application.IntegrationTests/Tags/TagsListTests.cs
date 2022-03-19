using Application.Features.Tags.Queries;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.IntegrationTests.Tags;

public class TagsListTests : TestBase
{
    [Test]
    public async Task Can_List_All_Tags()
    {
        await Context.Tags.AddRangeAsync(
            new Tag { Name = "Tag3" },
            new Tag { Name = "Tag2" },
            new Tag { Name = "Tag1" }
        );
        await Context.SaveChangesAsync();

        var response = await Act(new TagsListQuery());

        response.Tags.Should().Equal("Tag1", "Tag2", "Tag3");
    }
}