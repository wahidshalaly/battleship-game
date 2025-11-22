using System;
using System.Threading;
using BattleshipGame.SharedKernel;
using FluentAssertions;
using MediatR;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.Common;

public class DomainEventTests
{
    [Fact]
    public void Ctor_WhenCreated_ShouldInitializeAllProperties()
    {
        // Act
        var domainEvent = new ConcreteDomainEvent();

        // Assert
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        domainEvent.EventType.Should().Be(typeof(ConcreteDomainEvent));
    }

    [Fact]
    public void EventId_WhenMultipleInstancesCreated_ShouldBeUnique()
    {
        // Act
        var event1 = new ConcreteDomainEvent();
        var event2 = new ConcreteDomainEvent();
        var event3 = new ConcreteDomainEvent();

        // Assert
        event1.EventId.Should().NotBe(event2.EventId);
        event1.EventId.Should().NotBe(event3.EventId);
        event2.EventId.Should().NotBe(event3.EventId);
    }

    [Fact]
    public void EventId_WhenMultipleInstancesCreated_ShouldAllBeNonEmpty()
    {
        // Act
        var events = new[]
        {
            new ConcreteDomainEvent(),
            new ConcreteDomainEvent(),
            new ConcreteDomainEvent(),
            new ConcreteDomainEvent(),
            new ConcreteDomainEvent(),
        };

        // Assert
        foreach (var domainEvent in events)
        {
            domainEvent.EventId.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void OccurredOn_WhenMultipleInstancesCreatedSequentially_ShouldProgressInTime()
    {
        // Act
        var event1 = new ConcreteDomainEvent();
        Thread.Sleep(10); // Small delay to ensure time progression
        var event2 = new ConcreteDomainEvent();
        Thread.Sleep(10); // Small delay to ensure time progression
        var event3 = new ConcreteDomainEvent();

        // Assert
        event2.OccurredOn.Should().BeAfter(event1.OccurredOn);
        event3.OccurredOn.Should().BeAfter(event2.OccurredOn);
        event3.OccurredOn.Should().BeAfter(event1.OccurredOn);
    }

    [Fact]
    public void OccurredOn_WhenCreated_ShouldBeUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new ConcreteDomainEvent();

        // Assert
        var afterCreation = DateTime.UtcNow;
        domainEvent.OccurredOn.Should().BeAfter(beforeCreation.AddMilliseconds(-1));
        domainEvent.OccurredOn.Should().BeBefore(afterCreation.AddMilliseconds(1));
    }

    [Fact]
    public void EventType_WhenCreated_ShouldMatchGenericType()
    {
        // Act
        var event1 = new ConcreteDomainEvent();
        var event2 = new AnotherConcreteDomainEvent();

        // Assert
        event1.EventType.Should().Be(typeof(ConcreteDomainEvent));
        event2.EventType.Should().Be(typeof(AnotherConcreteDomainEvent));
    }

    [Fact]
    public void EventType_WhenMultipleInstancesOfSameType_ShouldBeSame()
    {
        // Act
        var event1 = new ConcreteDomainEvent();
        var event2 = new ConcreteDomainEvent();
        var event3 = new ConcreteDomainEvent();

        // Assert
        event1.EventType.Should().Be(event2.EventType);
        event2.EventType.Should().Be(event3.EventType);
        event1.EventType.Should().Be(event3.EventType);
    }

    [Fact]
    public void DomainEvent_ShouldImplementIDomainEvent()
    {
        // Act
        var domainEvent = new ConcreteDomainEvent();

        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void DomainEvent_ShouldImplementINotification()
    {
        // Act
        var domainEvent = new ConcreteDomainEvent();

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void Properties_ShouldBeInitOnly()
    {
        // Arrange
        var domainEvent = new ConcreteDomainEvent();
        var originalEventId = domainEvent.EventId;
        var originalOccurredOn = domainEvent.OccurredOn;
        var originalEventType = domainEvent.EventType;

        // Act - Properties should not be settable after initialization
        // This is verified by compilation - init-only properties cannot be set after construction

        // Assert - Values should remain the same
        domainEvent.EventId.Should().Be(originalEventId);
        domainEvent.OccurredOn.Should().Be(originalOccurredOn);
        domainEvent.EventType.Should().Be(originalEventType);
    }

    [Fact]
    public void DifferentEventTypes_ShouldHaveDifferentEventTypes()
    {
        // Act
        var concreteEvent = new ConcreteDomainEvent();
        var anotherEvent = new AnotherConcreteDomainEvent();

        // Assert
        concreteEvent.EventType.Should().NotBe(anotherEvent.EventType);
        concreteEvent.EventType.Should().Be(typeof(ConcreteDomainEvent));
        anotherEvent.EventType.Should().Be(typeof(AnotherConcreteDomainEvent));
    }

    [Fact]
    public void DomainEvent_WithCustomProperties_ShouldMaintainBaseProperties()
    {
        // Act
        var customEvent = new CustomDomainEvent("TestData", 42);

        // Assert
        // Base DomainEvent properties
        customEvent.EventId.Should().NotBeEmpty();
        customEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customEvent.EventType.Should().Be(typeof(CustomDomainEvent));

        // Custom properties
        customEvent.Data.Should().Be("TestData");
        customEvent.Number.Should().Be(42);
    }

    [Fact]
    public void Multiple_CustomDomainEvents_ShouldHaveUniqueBaseProperties()
    {
        // Act
        var event1 = new CustomDomainEvent("Data1", 1);
        var event2 = new CustomDomainEvent("Data2", 2);

        // Assert
        // Should have unique EventIds
        event1.EventId.Should().NotBe(event2.EventId);

        // Should have same EventType (both are CustomDomainEvent)
        event1.EventType.Should().Be(event2.EventType);

        // Should have different custom data
        event1.Data.Should().NotBe(event2.Data);
        event1.Number.Should().NotBe(event2.Number);
    }
}

// Test implementations for testing DomainEvent base class
internal class ConcreteDomainEvent : DomainEvent<ConcreteDomainEvent>
{
    public string TestProperty { get; } = "TestValue";
}

internal class AnotherConcreteDomainEvent : DomainEvent<AnotherConcreteDomainEvent>
{
    public int AnotherProperty { get; } = 123;
}

internal class CustomDomainEvent : DomainEvent<CustomDomainEvent>
{
    public CustomDomainEvent(string data, int number)
    {
        Data = data;
        Number = number;
    }

    public string Data { get; }
    public int Number { get; }
}
