using ApiIntegrationLog.Api.Models;
using ApiIntegrationLog.Api.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationLog.Api.Tests.Users;

public class GetUserTest : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public GetUserTest(ApiFactory apiFactory) => _client = apiFactory.CreateClient();

    [Fact]
    public async Task Get_User_WhenUserExists()
    {
        // Arrange
        var user = new CreateUserRequest("userName");
        var createUserResponse = await _client.PostAsJsonAsync("users", user);
        var createdUser = await createUserResponse.Content.ReadFromJsonAsync<User>();

        // Act
        var result = await _client.GetAsync($"users/{createdUser!.Id}");
        var existingUser = await result.Content.ReadFromJsonAsync<User>();

        // Assert
        existingUser.Should().BeEquivalentTo(createdUser);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}