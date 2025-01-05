using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TShort.Api.Data;

namespace TShort.Api.Tests.Integration.Extensions;

internal class DataExtension(string connectionString) : IAlbaExtension
{
    public void Dispose()
    {
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task Start(IAlbaHost host) => Task.CompletedTask;

    public IHostBuilder Configure(IHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;
            services.AddSingleton(options);
        });

        return builder;
    }
}
