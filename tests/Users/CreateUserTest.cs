using ApiIntegrationLog.Api.Models;
using ApiIntegrationLog.Api.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationLog.Api.Tests.Users;

public class CreateUserTest : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CreateUserTest(ApiFactory apiFactory) => _client = apiFactory.CreateClient();

    [Fact]
    public async Task Create_User_WhenDataIsValid()
    {
        // Arrange
        var user = new CreateUserRequest("userName");

        // Act
        var result = await _client.PostAsJsonAsync("users", user);
        var userResponse = await result.Content.ReadFromJsonAsync<User>();

        // Assert
        var expectedUser = new User
        {
            Id = userResponse!.Id,
            Name = userResponse!.Name
        };

        userResponse.Should().BeEquivalentTo(expectedUser);
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Headers.Location!.ToString().Should()
            .Be($"/users/{userResponse!.Id}");
    }
}