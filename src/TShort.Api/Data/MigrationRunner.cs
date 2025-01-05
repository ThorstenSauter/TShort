using Microsoft.EntityFrameworkCore;

namespace TShort.Api.Data;

internal static class MigrationRunner<T>
    where T : DbContext
{
    internal static async Task RunAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<T>();
        await dbContext.Database.MigrateAsync();
    }
}
