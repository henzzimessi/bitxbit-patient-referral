using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatientReferral.Infrastructure.Data;

namespace PatientReferral.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private string? _dbName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _dbName ??= $"{GetDatabaseNamePrefix()}_{Guid.NewGuid():N}";

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            ConfigureAdditionalServices(services);
        });
    }

    protected virtual string GetDatabaseNamePrefix() => "PatientReferral_TestDb";

    protected virtual void ConfigureAdditionalServices(IServiceCollection services)
    {
    }
}
