# Libra Reserved Call Binding Notes

The parser should continue to treat reserved function arguments as ordinary syntax expressions. It should **not** enforce function arity or argument types while parsing.

For example:

```text
;catEm(2, x, y, z)
```

should parse successfully into something conceptually equivalent to:

```text
ReservedCallSyntax
    Name: "catEm"
    Arguments:
        TextSyntax("2")
        TextSyntax("x")
        TextSyntax("y")
        TextSyntax("z")
```

The later binding/building phase is responsible for deciding whether the reserved call exists, whether it received the correct number of arguments, and whether each argument has an acceptable type.

## Proposed Reserved Call Binder

Introduce an interface representing the binding behavior for a particular reserved call:

```csharp
internal interface IReservedCallBinder
{
    LibraExpression Bind(
        ReservedCallSyntax syntax,
        LibraBinder binder);
}
```

Each reserved function can have an implementation of this interface.

The binder receives the complete `ReservedCallSyntax`, including all arguments supplied by the user. It may validate arity and interpret individual arguments according to the function's needs.

For example, `;catEm` conceptually wants:

```text
;catEm(gapInEm, expression1, expression2, ...)
```

Its binder could look approximately like:

```csharp
internal sealed class CatEmBinder : IReservedCallBinder
{
    public LibraExpression Bind(
        ReservedCallSyntax syntax,
        LibraBinder binder)
    {
        if (syntax.Arguments.Count < 2)
        {
            throw binder.CreateValidationException(
                syntax,
                ";catEm requires a gap followed by at least one expression.");
        }

        var gap = binder.BindNumber(syntax.Arguments[0]);

        var expressions = syntax.Arguments
            .Skip(1)
            .Select(binder.BindExpression)
            .ToArray();

        return LibraExpression.Concat(gap, expressions);
    }
}
```

Exact exception and factory APIs may differ from this example.

## Reusable Argument Binding Helpers

`LibraBinder` should provide helpers for interpreting syntax arguments as specific kinds of values.

Possible initial helpers:

```csharp
internal LibraExpression BindExpression(ExpressionSyntax syntax);

internal decimal BindNumber(ExpressionSyntax syntax);

internal int BindInteger(ExpressionSyntax syntax);

internal string BindString(ExpressionSyntax syntax);
```

Only add helpers when an actual reserved call requires them. Do not build a general-purpose type system in advance.

`BindExpression` recursively converts ordinary syntax into a `LibraExpression`.

`BindNumber` accepts syntax representing a numeric literal and returns its numeric value rather than first constructing a `TextExpression` and asking the reserved call to parse its text.

For example:

```text
;catEm(2, x, y)
```

should conceptually become:

```text
gap = 2
expressions = [
    TextExpression("x"),
    TextExpression("y")
]
```

The `CatEmBinder` should **not** receive a `TextExpression("2")` and then call `decimal.Parse` on the expression's text.

The parsing layer has already preserved the original syntax, so the binding layer should perform this conversion directly.

## Keep Numeric Binding Conservative Initially

Initially, `BindNumber` can recognize only syntax that clearly represents a numeric literal.

For example:

```text
2
2.5
```

may be valid numeric arguments.

More complicated forms such as:

```text
1 + 1
```

should not automatically require constant-expression evaluation. Supporting that can be added later if there is a concrete need.

Unary negative numbers such as:

```text
-2
```

can be supported if convenient by recognizing the corresponding prefix syntax around a numeric literal.

## Other Reserved Calls

A normal expression-only function remains simple.

For example, the binder for:

```text
;frac(x, y)
```

could do approximately:

```csharp
internal sealed class FractionBinder : IReservedCallBinder
{
    public LibraExpression Bind(
        ReservedCallSyntax syntax,
        LibraBinder binder)
    {
        if (syntax.Arguments.Count != 2)
        {
            throw binder.CreateValidationException(
                syntax,
                ";frac requires exactly two arguments.");
        }

        var numerator = binder.BindExpression(syntax.Arguments[0]);
        var denominator = binder.BindExpression(syntax.Arguments[1]);

        return LibraExpression.Fraction(
            numerator,
            denominator);
    }
}
```

This keeps function-specific rules in the function-specific binder.

## Registry

Reserved function names can map to their corresponding binders:

```csharp
internal static class ReservedCallRegistry
{
    private static readonly IReadOnlyDictionary<string, IReservedCallBinder> Binders =
        new Dictionary<string, IReservedCallBinder>
        {
            ["frac"] = new FractionBinder(),
            ["catEm"] = new CatEmBinder(),
            // ...
        };

    public static bool TryGetBinder(
        string name,
        out IReservedCallBinder? binder)
    {
        return Binders.TryGetValue(name, out binder);
    }
}
```

The exact registry design can follow the conventions already used elsewhere in Libra.

## Intended Pipeline

The overall pipeline should remain:

```text
source
    ↓
lexer
    ↓
tokens
    ↓
parser
    ↓
ExpressionSyntax tree
    ↓
LibraBinder / reserved-call binders
    ↓
LibraExpression tree
```

The responsibilities should remain separated:

```text
Lexer:
    Recognize tokens.

Parser:
    Determine syntactic structure.
    Parse all supplied reserved-call arguments.
    Do not enforce reserved-call arity or typed arguments.

Binder:
    Resolve syntax into concrete Libra expressions.
    Look up reserved names.
    Validate arity.
    Interpret typed arguments.

IReservedCallBinder:
    Own the function-specific rules for one reserved call.
```

## Main Design Goal

Avoid this:

```text
Reserved call receives TextExpression("2")
    ↓
extract text from rendered/expression object
    ↓
parse "2" back into a number
```

Prefer this:

```text
TextSyntax("2")
    ↓
LibraBinder.BindNumber(...)
    ↓
decimal/int/etc. value
    ↓
reserved-call implementation
```

This preserves the intended boundary between textual syntax and the final `LibraExpression` model while avoiding the need to design a large declarative argument-type system prematurely.
