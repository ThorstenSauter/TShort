using FastEndpoints;
using TShort.Api;
using TShort.Api.Data;

using var loggerFactory = LoggerFactory.Create(static builder => builder.AddConsole());
var bootStrapLogger = loggerFactory.CreateLogger<Program>();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();
    builder.AddSqlServerDbContext<AppDbContext>("tshort");

    builder.Services
        .AddApplicationServices()
        .AddMicrosoftIdentityPlatform(builder.Configuration)
        .ConfigureEndpoints();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        await MigrationRunner<AppDbContext>.RunAsync(app);
    }

    app.MapDefaultEndpoints();
    app.UseFastEndpoints(static c => c.Errors.UseProblemDetails());

    await app.RunAsync();
}
catch (Exception ex)
{
    bootStrapLogger.LogCritical(ex, "The application shut down unexpectedly");
}
