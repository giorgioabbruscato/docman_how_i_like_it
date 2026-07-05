using FluentAssertions;
using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.Tenancy;

namespace HrPortal.UnitTests.Tenancy;

public sealed class TenantQueryExtensionsTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void ApplyTenantScope_SingleMode_ReturnsQueryUnchanged()
    {
        var entities = CreateEntities();
        var ctx = TenantContext.CreateSingleTenantDefault(TenantA, "demo");

        var result = entities.AsQueryable().ApplyTenantScope(ctx).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void ApplyTenantScope_MultiModeUnresolved_ThrowsTenantNotResolvedException()
    {
        var entities = CreateEntities();
        var ctx = TenantContext.Empty;

        var act = () => entities.AsQueryable().ApplyTenantScope(ctx).ToList();

        act.Should().Throw<TenantNotResolvedException>();
    }

    [Fact]
    public void ApplyTenantScope_MultiModeResolved_FiltersByTenantId()
    {
        var entities = CreateEntities();
        var ctx = TenantContext.CreateTenantOnly(TenantA, "tenant-a");

        var result = entities.AsQueryable().ApplyTenantScope(ctx).ToList();

        result.Should().HaveCount(1);
        result[0].TenantId.Should().Be(TenantA);
    }

    private static List<TestTenantEntity> CreateEntities() =>
    [
        new TestTenantEntity { Id = Guid.NewGuid(), TenantId = TenantA },
        new TestTenantEntity { Id = Guid.NewGuid(), TenantId = TenantB }
    ];

    private sealed class TestTenantEntity : ITenantEntity
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
    }
}
