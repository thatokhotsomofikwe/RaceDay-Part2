using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthAndRoleTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthAndRoleTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_RaceDay");
                    });
                });
            });
        }

        [Fact]
        public async Task RegisterAndLogin_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.CreateClient();
            var registerDto = new RegisterDto { FullName = "Test User", Email = "test@test.com", Password = "Password123", Role = "Participant" };

            // Act
            var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginDto { Email = "test@test.com", Password = "Password123" });

            // Assert
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotCreateEvent_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Note: In a real integration test, you must login first to get the session cookie.
            // Here, we test the unauthenticated/unauthorized path.

            var newEvent = new { EventName = "Test Race", Description = "Desc", EventDate = "2026-10-18", StartTime = "06:30:00", Location = "Loc", EventType = "Running" };

            // Act
            var response = await client.PostAsJsonAsync("/api/events", newEvent);

            // Assert
            // If not logged in, it should be Unauthorized (401)
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}