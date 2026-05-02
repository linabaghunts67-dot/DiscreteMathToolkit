using DiscreteMathToolkit.Core.Logic;
using FluentAssertions;
using Xunit;

namespace DiscreteMathToolkit.Tests.Logic;

public class LogicTests
{
    [Theory]
    [InlineData("p AND q", "p", true, "q", true, true)]
    [InlineData("p AND q", "p", true, "q", false, false)]
    [InlineData("p OR q",  "p", false, "q", true, true)]
    [InlineData("p XOR q", "p", true, "q", true, false)]
    [InlineData("p XOR q", "p", true, "q", false, true)]
    [InlineData("p -> q",  "p", true, "q", false, false)]
    [InlineData("p -> q",  "p", false, "q", false, true)]
    [InlineData("p <-> q", "p", true, "q", true, true)]
    [InlineData("p <-> q", "p", true, "q", false, false)]
    public void Parser_BasicOperatorsEvaluateCorrectly(string expr, string varA, bool a, string varB, bool b, bool expected)
    {
        var node = LogicParser.Parse(expr);
        var env = new Dictionary<string, bool> { [varA] = a, [varB] = b };
        node.Evaluate(env).Should().Be(expected);
    }

    [Fact]
    public void Parser_RespectsPrecedence_NotBindsTighterThanAnd()
    {
        // !p AND q  must parse as  (!p) AND q
        var node = LogicParser.Parse("!p AND q");
        var env = new Dictionary<string, bool> { ["p"] = false, ["q"] = true };
        node.Evaluate(env).Should().BeTrue();
    }

    [Fact]
    public void Parser_AndBindsTighterThanOr()
    {
        // p OR q AND r === p OR (q AND r)
        var node = LogicParser.Parse("p OR q AND r");
        var env = new Dictionary<string, bool> { ["p"] = false, ["q"] = true, ["r"] = false };
        node.Evaluate(env).Should().BeFalse();
    }

    [Fact]
    public void Parser_ImpliesIsRightAssociative()
    {
        // p -> q -> r  =  p -> (q -> r)
        // With p=true, q=true, r=false: q->r = false, p->false = false
        var node = LogicParser.Parse("p -> q -> r");
        var env = new Dictionary<string, bool> { ["p"] = true, ["q"] = true, ["r"] = false };
        node.Evaluate(env).Should().BeFalse();
    }

    [Fact]
    public void Parser_AcceptsSymbolicAndUnicodeOperators()
    {
        var a = LogicParser.Parse("p ∧ q");
        var b = LogicParser.Parse("p && q");
        var env = new Dictionary<string, bool> { ["p"] = true, ["q"] = true };
        a.Evaluate(env).Should().BeTrue();
        b.Evaluate(env).Should().BeTrue();
    }

    [Fact]
    public void Parser_ThrowsOnMalformedInput()
    {
        Assert.Throws<FormatException>(() => LogicParser.Parse("p AND"));
        Assert.Throws<FormatException>(() => LogicParser.Parse("("));
        Assert.Throws<FormatException>(() => LogicParser.Parse("p $ q"));
    }

    [Fact]
    public void TruthTable_DetectsTautology()
    {
        var node = LogicParser.Parse("p OR !p");
        var t = TruthTableBuilder.Build(node);
        t.Classification.Should().Be(LogicClassification.Tautology);
        t.Rows.Should().AllSatisfy(r => r.Result.Should().BeTrue());
    }

    [Fact]
    public void TruthTable_DetectsContradiction()
    {
        var node = LogicParser.Parse("p AND !p");
        var t = TruthTableBuilder.Build(node);
        t.Classification.Should().Be(LogicClassification.Contradiction);
    }

    [Fact]
    public void TruthTable_DetectsContingency()
    {
        var node = LogicParser.Parse("p AND q");
        var t = TruthTableBuilder.Build(node);
        t.Classification.Should().Be(LogicClassification.Contingency);
        t.Rows.Should().HaveCount(4);
    }

    [Fact]
    public void Simplifier_DoubleNegation()
    {
        var node = LogicParser.Parse("!!p");
        var simplified = BooleanSimplifier.Simplify(node);
        simplified.ToInfix().Should().Be("p");
    }

    [Fact]
    public void Simplifier_AndWithFalseIsFalse()
    {
        var node = LogicParser.Parse("p AND 0");
        var simplified = BooleanSimplifier.Simplify(node);
        simplified.Should().BeOfType<ConstNode>().Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Simplifier_OrWithTrueIsTrue()
    {
        var node = LogicParser.Parse("p OR 1");
        var simplified = BooleanSimplifier.Simplify(node);
        simplified.Should().BeOfType<ConstNode>().Which.Value.Should().BeTrue();
    }

    [Fact]
    public void Simplifier_XWithItselfIdempotent()
    {
        var node = LogicParser.Parse("p AND p");
        var simplified = BooleanSimplifier.Simplify(node);
        simplified.ToInfix().Should().Be("p");
    }

    [Fact]
    public void Simplifier_XAndNotXIsFalse()
    {
        var node = LogicParser.Parse("p AND !p");
        var simplified = BooleanSimplifier.Simplify(node);
        simplified.Should().BeOfType<ConstNode>().Which.Value.Should().BeFalse();
    }
}
