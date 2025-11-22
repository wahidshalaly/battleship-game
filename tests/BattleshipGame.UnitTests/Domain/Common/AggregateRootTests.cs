using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.SharedKernel;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.Common;

public class AggregateRootTests
{
    [Fact]
    public void Ctor_WhenCalledWithoutId_ShouldCreateAggregateWithNewId()
    {
        // Act
        var aggregate = new TestAggregateRoot();

        // Assert
        aggregate.Id.Should().NotBeNull();
        aggregate.Id.Value.Should().NotBeEmpty();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_WhenCalledWithId_ShouldCreateAggregateWithSpecifiedId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var aggregate = new TestAggregateRoot(id);

        // Assert
        aggregate.Id.Should().NotBeNull();
        aggregate.Id.Value.Should().Be(id);
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_WhenInitialized_ShouldBeEmpty()
    {
        // Arrange & Act
        var aggregate = new TestAggregateRoot();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
        aggregate.DomainEvents.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
    }

    [Fact]
    public void AddDomainEvent_WhenCalled_ShouldAddEventToDomainEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent = new TestDomainEvent();

        // Act
        aggregate.PublicAddDomainEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_WhenCalledMultipleTimes_ShouldAddAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent1 = new TestDomainEvent();
        var domainEvent2 = new TestDomainEvent();
        var domainEvent3 = new TestDomainEvent();

        // Act
        aggregate.PublicAddDomainEvent(domainEvent1);
        aggregate.PublicAddDomainEvent(domainEvent2);
        aggregate.PublicAddDomainEvent(domainEvent3);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
        aggregate.DomainEvents.Should().Contain(domainEvent1);
        aggregate.DomainEvents.Should().Contain(domainEvent2);
        aggregate.DomainEvents.Should().Contain(domainEvent3);
    }

    [Fact]
    public void AddDomainEvent_WhenSameEventAddedTwice_ShouldAddBothInstances()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent = new TestDomainEvent();

        // Act
        aggregate.PublicAddDomainEvent(domainEvent);
        aggregate.PublicAddDomainEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(2);
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_WhenCalled_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent1 = new TestDomainEvent();
        var domainEvent2 = new TestDomainEvent();

        aggregate.PublicAddDomainEvent(domainEvent1);
        aggregate.PublicAddDomainEvent(domainEvent2);
        aggregate.DomainEvents.Should().HaveCount(2); // Verify events were added

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WhenCalledOnEmptyCollection_ShouldNotThrow()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();

        // Act
        var act = () => aggregate.ClearDomainEvents();

        // Assert
        act.Should().NotThrow();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_WhenModified_ShouldMaintainOrder()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent1 = new TestDomainEvent { TestProperty = "First" };
        var domainEvent2 = new TestDomainEvent { TestProperty = "Second" };
        var domainEvent3 = new TestDomainEvent { TestProperty = "Third" };

        // Act
        aggregate.PublicAddDomainEvent(domainEvent1);
        aggregate.PublicAddDomainEvent(domainEvent2);
        aggregate.PublicAddDomainEvent(domainEvent3);

        // Assert
        var events = aggregate.DomainEvents.Cast<TestDomainEvent>().ToList();
        events[0].TestProperty.Should().Be("First");
        events[1].TestProperty.Should().Be("Second");
        events[2].TestProperty.Should().Be("Third");
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent = new TestDomainEvent();
        aggregate.PublicAddDomainEvent(domainEvent);

        // Act & Assert
        var domainEvents = aggregate.DomainEvents;
        domainEvents.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Equals_WhenSameInstanceOrNot_ShouldInheritEntityBehavior(
        bool areSame,
        bool expectedResult
    )
    {
        // Arrange
        var (aggregate1, aggregate2) = CreateAggregates(areSame);

        // Act & Assert
        aggregate1.Equals(aggregate2).Should().Be(expectedResult);
    }

    [Fact]
    public void GetHashCode_WhenSameInstance_ShouldInheritEntityBehavior()
    {
        // Arrange
        var (aggregate1, aggregate2) = CreateAggregates(areSame: true);

        // Act & Assert
        aggregate1.GetHashCode().Should().Be(aggregate2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenNotSameInstance_ShouldInheritEntityBehavior()
    {
        // Arrange
        var (aggregate1, aggregate2) = CreateAggregates(areSame: false);

        // Act & Assert
        aggregate1.GetHashCode().Should().NotBe(aggregate2.GetHashCode());
    }

    private static (TestAggregateRoot, TestAggregateRoot) CreateAggregates(bool areSame)
    {
        var id1 = Guid.NewGuid();
        var id2 = areSame ? id1 : Guid.NewGuid();
        var aggregate1 = new TestAggregateRoot(id1);
        var aggregate2 = new TestAggregateRoot(id2);
        return (aggregate1, aggregate2);
    }
}

// Test classes for testing AggregateRoot
internal class TestAggregateRoot : AggregateRoot<TestAggregateRootId>
{
    public TestAggregateRoot() { }

    public TestAggregateRoot(Guid id)
        : base(id) { }

    // Expose protected method for testing
    public void PublicAddDomainEvent(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }

    public string TestProperty { get; set; } = "TestValue";
}

internal record TestAggregateRootId(Guid Value) : EntityId(Value);

internal class TestDomainEvent : DomainEvent<TestDomainEvent>
{
    public string TestProperty { get; init; } = "TestEvent";
}
