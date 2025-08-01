using System;
using BattleshipGame.Domain.DomainModel.Common;
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
        var (subject, subject2) = CreateSubject(areSame);

        subject.Equals(subject2).Should().Be(expectedResult);
    }

    [Fact]
    public void GetHashCode_WhenSameInstance_ReturnsSameHashCode()
    {
        var (subject, subject2) = CreateSubject(areSame: true);

        subject.GetHashCode().Should().Be(subject2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenNotSameInstance_ReturnsDifferentHashCode()
    {
        var (subject, subject2) = CreateSubject(areSame: false);

        subject.GetHashCode().Should().NotBe(subject2.GetHashCode());
    }

    private static (TestEntity, TestEntity) CreateSubject(bool areSame)
    {
        var id1 = Guid.NewGuid();
        var id2 = areSame ? id1 : Guid.NewGuid();
        var subject = new TestEntity(id1);
        var subject2 = new TestEntity(id2);
        return (subject, subject2);
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
