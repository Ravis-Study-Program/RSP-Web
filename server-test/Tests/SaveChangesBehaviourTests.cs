using Bogus;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Tests.Shared;
using System.Text.Json;
using Xunit;

namespace RSPWebAPI.Tests.Tests;

public class SaveChangesBehaviourTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory), IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory = factory;
    private readonly Faker _faker = new();
    public async Task InitializeAsync()
    {
        await _factory.ResetDatabase();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SaveChangesWithAuditAsync_ShouldCreateAuditEvent_WhenAddingEntity()
    {
        var user = new UserEntity
        {
            UserId = Constants.GeneratePrimaryKeyId(),
            Email = _faker.Internet.Email().ToLower(),
            Name = _faker.Name.FullName(),
            Slug = _faker.Random.AlphaNumeric(8),
            IsAdmin = false,
            IsTestUser = false,
        };

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var auditEvent = await DbContext.AuditEvents.FirstOrDefaultAsync();

        Assert.NotNull(auditEvent);
        Assert.Equal(_factory.UserId, auditEvent.ModifiedByUserId);
        Assert.Equal("User", auditEvent.TableName);

        var changes = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(auditEvent.ChangeState);

        Assert.NotNull(changes);
        Assert.Equal(10, changes.Count);

        Assert.Equal(user.Email, changes.First(c => c["Column"]!.ToString() == "Email")["NewValue"]!.ToString());
        Assert.Equal(user.Name, changes.First(c => c["Column"]!.ToString() == "Name")["NewValue"]!.ToString());
        Assert.Equal(user.Slug, changes.First(c => c["Column"]!.ToString() == "Slug")["NewValue"]!.ToString());
        Assert.Equal(user.IsAdmin.ToString(), changes.First(c => c["Column"]!.ToString() == "IsAdmin")["NewValue"]!.ToString(), ignoreCase: true);
        Assert.Equal(user.IsTestUser.ToString(), changes.First(c => c["Column"]!.ToString() == "IsTestUser")["NewValue"]!.ToString(), ignoreCase: true);
    }

    [Fact]
    public async Task SaveChangesWithAuditAsync_ShouldCreateAuditEvent_WhenUpdatingEntity()
    {
        var user = new UserEntity
        {
            UserId = Constants.GeneratePrimaryKeyId(),
            Email = _faker.Internet.Email().ToLower(),
            Name = _faker.Name.FullName(),
            Slug = _faker.Random.AlphaNumeric(8),
            IsAdmin = false,
            IsTestUser = false,
        };

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        user.IsAdmin = true;

        DbContext.Users.Update(user);
        await DbContext.SaveChangesAsync();

        var auditEvent = await DbContext.AuditEvents.OrderByDescending(e => e.AuditedAtUtc).FirstOrDefaultAsync();

        Assert.NotNull(auditEvent);
        Assert.Equal(_factory.UserId, auditEvent.ModifiedByUserId);
        Assert.Equal("User", auditEvent.TableName);

        var changes = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(auditEvent.ChangeState);

        Assert.NotNull(changes);

        var adminChange = changes.FirstOrDefault();

        Assert.NotNull(adminChange);
        Assert.Equal("False", adminChange["OldValue"]!.ToString());
        Assert.Equal("True", adminChange["NewValue"]!.ToString());
    }
}
