using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public sealed class RaceDayApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "RaceDayTests_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<RaceDayDbContext>();
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            // Remove the provider-specific AddDbContext configuration registered by Program
            // without depending on EF Core's internal configuration-interface type.
            var sqlProviderConfigurations = services
                .Where(descriptor => descriptor.ServiceType.IsGenericType
                    && descriptor.ServiceType.GetGenericTypeDefinition().Name == "IDbContextOptionsConfiguration`1"
                    && descriptor.ServiceType.GenericTypeArguments[0] == typeof(RaceDayDbContext))
                .ToList();
            foreach (var descriptor in sqlProviderConfigurations) services.Remove(descriptor);
            services.AddDbContext<RaceDayDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
