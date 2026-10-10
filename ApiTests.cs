using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public sealed class ApiTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public ApiTests(RaceDayApiFactory factory) => _factory = factory;

    [Fact]
    public async Task ProfileRequiresAuthentication()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserCanRegisterLoginAndViewOwnProfile()
    {
        using var client = _factory.CreateClient();
        var email = $"participant.{Guid.NewGuid():N}@example.co.za";
        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Test Participant",
            email,
            password = "SecurePass123!",
            role = "Participant",
            phoneNumber = "0820000000"
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "SecurePass123!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var profile = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        var body = await profile.Content.ReadAsStringAsync();
        Assert.Contains(email, body);
    }

    [Fact]
    public async Task ParticipantCannotCreateEvent()
    {
        using var client = _factory.CreateClient();
        await Login(client, "sipho.participant@raceday.co.za", "RaceDay123!");
        var response = await client.PostAsJsonAsync("/api/events", new
        {
            eventName = "Should Be Forbidden",
            description = "A participant must not create events.",
            eventDate = "2027-01-20",
            startTime = "06:30:00",
            location = "Johannesburg, South Africa",
            eventType = "Running",
            status = "Open"
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrganiserCanCreateEvent()
    {
        using var client = _factory.CreateClient();
        await Login(client, "thabo.organiser@raceday.co.za", "RaceDay123!");
        var response = await client.PostAsJsonAsync("/api/events", new
        {
            eventName = "Test City Run",
            description = "Created by an organiser integration test.",
            eventDate = "2027-02-01",
            startTime = "07:00:00",
            location = "Pretoria, South Africa",
            eventType = "Run",
            status = "Open"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var eventId = payload.RootElement.GetProperty("eventId").GetInt32();

        var update = await client.PutAsJsonAsync($"/api/events/{eventId}", new
        {
            eventName = "Test City Run Updated",
            description = "Updated by an organiser integration test.",
            eventDate = "2027-02-02",
            startTime = "07:15:00",
            location = "Pretoria, South Africa",
            eventType = "Running",
            status = "Open"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var delete = await client.DeleteAsync($"/api/events/{eventId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task ParticipantCanEnrolInCategoryAndSeeTheirEnrolments()
    {
        using var client = _factory.CreateClient();
        await Login(client, "sipho.participant@raceday.co.za", "RaceDay123!");
        var enrolment = await client.PostAsJsonAsync("/api/events/3/enrolments", new { categoryId = 5 });
        Assert.Equal(HttpStatusCode.Created, enrolment.StatusCode);

        var mine = await client.GetAsync("/api/enrolments/me");
        Assert.Equal(HttpStatusCode.OK, mine.StatusCode);
        var body = await mine.Content.ReadAsStringAsync();
        Assert.Contains("Cape Town Coastal Cycle", body);
    }


    [Fact]
    public async Task LoginRejectsAnIncorrectPassword()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "sipho.participant@raceday.co.za",
            password = "DefinitelyWrong123!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrganiserCannotUseParticipantOnlyEnrolmentHistory()
    {
        using var client = _factory.CreateClient();
        await Login(client, "thabo.organiser@raceday.co.za", "RaceDay123!");
        var response = await client.GetAsync("/api/enrolments/me");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrganiserCannotEnrolInAnEventAsAParticipant()
    {
        using var client = _factory.CreateClient();
        await Login(client, "thabo.organiser@raceday.co.za", "RaceDay123!");
        var response = await client.PostAsJsonAsync("/api/events/3/enrolments", new { categoryId = 5 });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task Login(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
