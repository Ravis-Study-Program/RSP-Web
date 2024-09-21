using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;

namespace RSPWebAPI.Database;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        var retryPolicy = Policy.Handle<NpgsqlException>()
            .Or<IOException>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        retryPolicy.Execute(() => 
        {
            dbContext.Database.Migrate();
        });
    }
}