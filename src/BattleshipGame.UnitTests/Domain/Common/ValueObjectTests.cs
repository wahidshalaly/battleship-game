using System;
using System.Collections.Generic;
using BattleshipGame.Domain.DomainModel.Common;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.Common;

public class ValueObjectTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Equals_WhenSameInstanceOrNot(bool areSame, bool expectedResult)
    {
        var (subject, subject2) = CreateSubject(areSame);

        subject.Equals(subject2).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void EqualOperator_WhenSameInstanceOrNot(bool areSame, bool expectedResult)
    {
        var (subject, subject2) = CreateSubject(areSame);

        (subject == subject2).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void NotEqualOperator_WhenSameInstanceOrNot(bool areSame, bool expectedResult)
    {
        var (subject, subject2) = CreateSubject(areSame);

        (subject != subject2).Should().Be(expectedResult);
    }

    private static (TestValueObject, TestValueObject) CreateSubject(bool areSame)
    {
        var subject = new TestValueObject();
        var subject2 = areSame ? subject : new TestValueObject();
        return (subject, subject2);
    }
}

internal class TestValueObject : ValueObject
{
    private static readonly Guid uuid = Guid.NewGuid();

    public Guid Value1 { get; } = uuid;
    public int Value2 { get; } = Random.Shared.Next();
    public string Value3 { get; } = uuid.ToString("N");
    public DateTime Value4 { get; } = DateTime.UtcNow;
    public object Value5 { get; } = new();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value1;
        yield return Value2;
        yield return Value3;
        yield return Value4;
        yield return Value5;
    }
}
