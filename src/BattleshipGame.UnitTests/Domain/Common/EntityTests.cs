using System;
using BattleshipGame.SharedKernel;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.Common;

public class EntityTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Equals_WhenSameInstanceOrNot(bool areSame, bool expectedResult)
    {
        var (testEntity1, testEntity2) = CreateSubject(areSame);

        testEntity1.Equals(testEntity2).Should().Be(expectedResult);
    }

    [Fact]
    public void GetHashCode_WhenSameInstance_ReturnsSameHashCode()
    {
        var (testEntity1, testEntity2) = CreateSubject(areSame: true);

        testEntity1.GetHashCode().Should().Be(testEntity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenNotSameInstance_ReturnsDifferentHashCode()
    {
        var (testEntity1, testEntity2) = CreateSubject(areSame: false);

        testEntity1.GetHashCode().Should().NotBe(testEntity2.GetHashCode());
    }

    private static (TestEntity, TestEntity) CreateSubject(bool areSame)
    {
        var id1 = Guid.NewGuid();
        var id2 = areSame ? id1 : Guid.NewGuid();
        var testEntity1 = new TestEntity(id1);
        var testEntity2 = new TestEntity(id2);
        return (testEntity1, testEntity2);
    }
}

internal class TestEntity : Entity<TestEntityId>
{
    public TestEntity(Guid id)
        : base(id) { }

    public Guid Value1 { get; } = Guid.NewGuid();
    public int Value2 { get; } = Random.Shared.Next();
    public string Value3 { get; } = DateTime.UtcNow.ToString("O");
    public DateTime Value4 { get; } = DateTime.UtcNow;
    public object Value5 { get; } = new();
}

internal record TestEntityId(Guid Value) : EntityId(Value);
