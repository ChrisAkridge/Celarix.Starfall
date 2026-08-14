# Libra Pratt Parser Review

Date: 2026-08-14

Scope reviewed: `Celarix.Starfall.Libra.Parsing`, the current `Libra DSL Grammar.txt`, the public `LibraExpression.Parse(...).Build()` entry point, and nearby expression/ID/color behavior that the builder relies on.

## Executive Summary

The parser architecture is heading in a reasonable direction, but the current implementation is not yet reliable enough to treat as a working DSL parser. The biggest problems are not minor Pratt-parser tuning issues; they are tokenization/state-machine errors and span/registry bugs that prevent large parts of the language from parsing at all.

The highest priority fixes are:

1. Rewrite or heavily simplify the lexer cursor model.
2. Fix `TextSpan` so every span is based on the same source identity/string.
3. Assign `_reservedFunctions` and align reserved-name syntax between grammar and implementation.
4. Make suffix lexing actually emit `IdentifierBlock` and `PropertyBlock`.
5. Fix script parsing so `x^y_z`, `x_y^z`, and rejected chains mean what the rule says they mean.
6. Add parser tests before extending the DSL further.

## Build/Verification

`dotnet build Celarix.Starfall/Celarix.Starfall.csproj` succeeds, but it emits warnings that directly confirm two review findings:

- `OperatorRegistry._reservedFunctions` is never assigned and will always be null.
- `LibraParseException.Diagnostic` is non-nullable but is never assigned.

`dotnet build Celarix.Starfall.slnx` fails in this Linux environment because `Celarix.Starfall.Presentations` targets Windows and requires `EnableWindowsTargeting=true`. I did not treat that as a Libra parser failure.

## Critical Findings

### 1. The lexer looks one character ahead while deciding how to handle the current character

In `Lexer.Start`, the code calls `PeekNext()` at startup. `PeekNext()` returns `_source[_position + 1]`, not the current character. The state decision is therefore based on the second character while `MoveNext()` consumes the first character.

Relevant code:

- `PeekNext`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:55`
- `Start`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:199`

Consequences:

- A one-character input such as `x` produces no text token. `PeekNext()` returns null and the lexer ends immediately.
- An input starting with a quote, such as `"x"`, is not recognized as a string at startup because the lexer looks at `x`, consumes `"`, and enters text mode.
- An input like `a+b` sees `+` during startup, enters operator mode, and consumes `a` as if it were the beginning of an operator.
- An input like `;frac(x,y)` starts by looking at `f`, consumes `;` into a text token, and never becomes a reserved function.
- An input like `[[name]]` starts by looking at the second `[`, then `PeekAhead(2)` looks at `n`, so it is rejected as a property block.

This is the first thing I would fix. Most downstream parser behavior is hard to evaluate while the lexer is systematically misaligned.

### 2. Delimiter handling repeats the same off-by-one mistake

`ConsumeUntilDelimiter` consumes a character with `MoveNext()` and then asks `TryHandleDelimiter` to inspect it. But several delimiter checks then call `PeekNext()` or `MoveNextAndAppend()` as if the lexer had not already advanced.

Examples:

- For `[[...]]`, after consuming the first `[`, `TryHandleDelimiter` calls `PeekNext()`, which checks the character after the next character. It misses the second `[`.
- For `;name`, after consuming `;`, `TryHandleDelimiter` calls `MoveNextAndAppend()`, which appends the first letter of the reserved name but not the semicolon. The registry expects names like `;frac`.
- For strings, after consuming `"`, it calls `MoveNextAndAppend()` and appends the first string character before entering string mode, not the quote. That makes string token boundaries wrong.
- For escapes, `String()` checks `PeekNext()` after consuming `\`, so `\"` is not recognized correctly; it checks the character after the quote.

Relevant code:

- `TryHandleDelimiter`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:127`
- `String`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:265`
- `Reserved`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:289`
- `Substitution`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:314`

The lexer needs a single convention: either inspect before consuming, or consume and then only use the already-consumed character plus a true current-position peek.

### 3. Identifier and property suffix states are effectively unreachable

The grammar defines suffixes:

```text
identifier_suffix = "@", identifier, { identifier };
property_block_suffix = "[", property_list, "]";
```

But the lexer currently throws whenever it sees `@`, both at the start of input and in the middle of a token stream. It also throws for single `[` property blocks unless the sequence is interpreted as a substitution opener.

Relevant code:

- `@` in `TryHandleDelimiter`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:141`
- `[` in `TryHandleDelimiter`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:144`
- `@` and `[` in `Start`: `Celarix.Starfall/Libra/Parsing/Lexer.cs:220`

This means the parser has `IdentifierRule` and `PropertyBlockRule`, and the validator/builder support those syntax nodes, but the lexer never produces the corresponding tokens in normal DSL input. As written, IDs/classes and property blocks are dead features.

### 4. `TextSpan.FromBounds` is incompatible with how token spans are created

`TextSpan.FromBounds` requires `left.Text.Equals(right.Text)`. That only works if `TextSpan.Text` means "the full source text" for every span. But most lexer-created spans store the token text in `TextSpan.Text`, while the EOF span stores the full source.

Relevant code:

- Token span creation from token text: `Celarix.Starfall/Libra/Parsing/Lexer.cs:94`
- Single-character spans from token text: `Celarix.Starfall/Libra/Parsing/Lexer.cs:187`
- EOF span from full source: `Celarix.Starfall/Libra/Parsing/Lexer.cs:51`
- Span merge equality check: `Celarix.Starfall/Libra/Parsing/TextSpan.cs:13`

Consequences:

- `a+b` eventually tries to combine spans whose `Text` values are `a` and `b`, then throws `ArgumentException`.
- `;frac(a,b)` would combine the reserved-name span and close-paren span, then throw for the same reason.
- `x@.id` or `x[foreground=red]`, once suffix lexing is fixed, would hit this too.

I would change `TextSpan` to store only `StartIndex` and `Length`, or store a stable `Source` reference/string consistently. The current hybrid makes AST span composition unusable.

### 5. Reserved functions cannot work because `_reservedFunctions` is never assigned

`OperatorRegistry` creates `funcInfo` with `;frac`, but never assigns it to `_reservedFunctions`.

Relevant code:

- Field: `Celarix.Starfall/Libra/Parsing/OperatorRegistry.cs:21`
- Local list: `Celarix.Starfall/Libra/Parsing/OperatorRegistry.cs:46`
- Lookups against unassigned field: `Celarix.Starfall/Libra/Parsing/OperatorRegistry.cs:77`, `Celarix.Starfall/Libra/Parsing/OperatorRegistry.cs:99`

Any path that calls `TryGetKnownReservedFunction` or `TryGetWhenNone` for a reserved function risks a `NullReferenceException`. The compiler already warns about this.

### 6. Reserved names/functions disagree with the grammar and with prior notes

The grammar says reserved names use `__name`:

- `Libra DSL Grammar.txt:54`
- `Libra DSL Grammar.txt:55`

The parser/registry uses semicolon-prefixed names such as `;frac`:

- `Celarix.Starfall/Libra/Parsing/OperatorRegistry.cs:48`
- `AI Notes/Libra Reserved Call Binding Notes.md` also discusses `;frac(...)` and `;catEm(...)`.

The playground uses `;catEm(...)`:

- `Celarix.Starfall.Playground/DelphinusTests/DelphinusSlide.cs:39`

Pick one reserved-call syntax and update every layer: grammar, lexer, registry, examples, notes, and tests. Right now a reader cannot tell whether `__frac(...)`, `;frac(...)`, or both are intended.

### 7. `LibraParseException.Diagnostic` is never assigned

The exception constructor accepts a diagnostic but does not store it.

Relevant code:

- `Celarix.Starfall/Libra/Parsing/LibraParseException.cs:9`
- `Celarix.Starfall/Libra/Parsing/LibraParseException.cs:11`

That means any caller catching `LibraParseException` loses span information unless it parses the message string. This undercuts the whole diagnostic model.

## Parser and Pratt-Rule Findings

### 8. Script parsing does not implement its own intended grouping rules

`ScriptOperatorRule` parses the superscript/subscript operand with `ParseExpression(OperatorRegistry.ScriptBindingPower)`. Since the Pratt loop continues while `rule.LeftBindingPower >= minimumBindingPower`, another script operator at the same precedence is consumed inside the operand before `ScriptOperatorRule` can inspect it.

Relevant code:

- Pratt loop condition: `Celarix.Starfall/Libra/Parsing/LibraParser.cs:72`
- Script operand parse: `Celarix.Starfall/Libra/Parsing/Rules/ScriptOperatorRule.cs:19`, `Celarix.Starfall/Libra/Parsing/Rules/ScriptOperatorRule.cs:36`
- Intended chain rejection/checking: `Celarix.Starfall/Libra/Parsing/Rules/ScriptOperatorRule.cs:23`, `Celarix.Starfall/Libra/Parsing/Rules/ScriptOperatorRule.cs:28`, `Celarix.Starfall/Libra/Parsing/Rules/ScriptOperatorRule.cs:40`, `Celarix.Starfall/Libra/Parsing/Rules/ScriptOperatorRule.cs:45`

Likely actual behavior after lexer/span fixes:

- `x^y_z` becomes `x^(y_z)`, not `x` with both superscript `y` and subscript `z`.
- `x_y^z` becomes `x_(y^z)`, not `x` with both subscript `y` and superscript `z`.
- `x^y^z` is not rejected by the explicit "Cannot chain superscripts" branch, because the second `^` is consumed inside the right operand first.

If the intent is TeX-like scripts where `x^y_z` attaches both scripts to `x`, parse script operands at a binding power above script, or parse only a primary/braced operand for scripts. If the intent is expression-style exponentiation, do not special-case the later `_`/`^` in this rule.

### 9. Parenthesized and braced syntax nodes do not span the opening delimiter

`ParseWhenNone` returns:

```csharp
new ParenthesizedExpressionSyntax(ParseExpression(), Expect(TokenKind.CloseParen).Span)
new BracedExpressionSyntax(ParseExpression(), Expect(TokenKind.CloseBrace).Span)
```

Relevant code:

- `Celarix.Starfall/Libra/Parsing/LibraParser.cs:92`
- `Celarix.Starfall/Libra/Parsing/LibraParser.cs:93`

The syntax node span is only the close delimiter span, not the full parenthesized/braced expression. That will make diagnostics and later source mapping misleading. It also bypasses `TextSpan.FromBounds`, which is probably why this particular path does not hit the span-composition bug.

### 10. The grammar has only `expr = primary, { suffix }`, but the implementation has binary/prefix/script operators

`Libra DSL Grammar.txt` does not describe `+`, `-`, `*`, `/`, comparisons, equality, prefix operators, script operators, braces, or operator precedence. The implementation supports all of those.

This is a language maintenance problem: there is no canonical contract for users, tests, or future parser work. The grammar should become the source of truth for:

- prefix operators,
- infix operators and precedence,
- script operators,
- grouping with parentheses versus non-rendering braces,
- reserved functions,
- suffix precedence,
- text/bare-word token boundaries,
- whitespace rules.

### 11. `ReservedFunctionWhenNoneRule.BindingPower` is unused

`IWhenNoneRule` exposes `BindingPower`, and `ReservedFunctionWhenNoneRule` returns `100`, but `LibraParser.ParseWhenNone` ignores binding power for all prefix/null-denotation rules except through each rule's own implementation.

Relevant code:

- Interface: `Celarix.Starfall/Libra/Parsing/Rules/IWhenNoneRule.cs`
- Reserved function binding power: `Celarix.Starfall/Libra/Parsing/Rules/ReservedFunctionWhenNoneRule.cs:10`
- Prefix uses its own binding power internally: `Celarix.Starfall/Libra/Parsing/Rules/PrefixOperatorRule.cs:17`

This is not breaking by itself, but it is misleading API shape. Either remove `BindingPower` from `IWhenNoneRule`, or make the parser architecture consistently use it.

### 12. Comma is globally tokenized but only meaningful inside reserved calls

The parser only handles comma in `ReservedFunctionWhenNoneRule`. A top-level `a,b` will parse `a`, then fail at end-of-input expecting EOF. That may be fine, but the grammar has `sequence = expr, { ",", expr }`, and the implementation has no general sequence syntax node.

Question: should comma sequences exist only as reserved-call argument lists, or should Libra have a row/sequence expression? There is already a `RowExpression`, so this deserves a deliberate decision.

## Lexer and Token Model Findings

### 13. Text tokens are too permissive and too implicit

Anything that is not a delimiter/operator becomes `Text`. That includes whitespace, dots, hashes, extra brackets, semicolons in some positions, and possibly Unicode/control characters.

The grammar says `bare_word = ascii_letter | ascii_digit`, but the implementation accepts much more. That may be intentional for display text, but it conflicts with a math DSL where `mt = 1 - t` should probably ignore or normalize layout whitespace around operators.

Question: is bare text meant to be literal text, identifier-like math symbols, or both? If both, the language probably needs separate token kinds for bare symbol identifiers versus quoted text.

### 14. `LibraToken` trims every token's text

`LibraToken` does `Text = text.Trim()`.

Relevant code:

- `Celarix.Starfall/Libra/Parsing/LibraToken.cs:18`

That silently changes string literals and text content. A DSL for a text layout library probably cannot throw away leading/trailing spaces inside quoted strings. For example, `" x "` should not become `"x"` if literal text matters.

Whitespace handling should be token-kind-specific. Operators and delimiters can ignore surrounding whitespace; string literals should preserve content; bare math identifiers probably should not include spaces at all.

### 15. The lexer has states for property/identifier blocks but no correct transition into them

`State.PropertyBlock` and `State.IdentifierBlock` exist, and their methods both call `ConsumeUntilDelimiter(...)`.

Relevant code:

- States: `Celarix.Starfall/Libra/Parsing/Lexer.cs:15`, `Celarix.Starfall/Libra/Parsing/Lexer.cs:18`
- Methods: `Celarix.Starfall/Libra/Parsing/Lexer.cs:390`, `Celarix.Starfall/Libra/Parsing/Lexer.cs:391`

But the transition code throws instead of entering those states. This is a sign the state machine evolved without a locking test suite.

### 16. Unterminated/invalid diagnostics often have misleading spans

`CreateException` accepts a `length` parameter but ignores it.

Relevant code:

- `Celarix.Starfall/Libra/Parsing/Lexer.cs:192`

It also creates spans from `_tokenBuilder`, which may not contain the offending delimiter or may contain already-trimmed/misaligned text. If good editor diagnostics are a goal, this needs a pass after the lexer cursor model is fixed.

### 17. Trie search can index with `-1`

`Trie.Search` calls `IndexOf` on the operator alphabet and immediately indexes `node.Children[index]`.

Relevant code:

- `Celarix.Starfall/Libra/Parsing/Trie.cs:31`
- `Celarix.Starfall/Extensions/IReadOnlyListExtensions.cs:8`

If `OperatorRegistry.IsOperatorStart` is accidentally called with a string containing a non-operator-start character, `IndexOf` returns `-1` and the trie indexes out of range. The current lexer misalignment can plausibly do this. Even after lexer fixes, this should defensively return false when `index < 0`.

### 18. The operator tokenizer does not clearly implement longest-match behavior

The trie contains complete operator symbols and prefix nodes, so `IsOperatorStart("<")` and `IsOperatorStart("<=")` both return true. That supports longest-match scanning for `<=`.

However, `TryHandleDelimiter` immediately emits a single-character operator token for any operator-start character it sees in text mode:

- `Celarix.Starfall/Libra/Parsing/Lexer.cs:175`

That means `a<=b` can become `<` followed by `=`, not `<=`, depending on path. Operator mode can build multi-character operators, but delimiter handling bypasses it.

### 19. Reserved-name token text probably drops the reserved sigil

When `TryHandleDelimiter` sees `;`, it calls `MoveNextAndAppend()` after already consuming `;`, so the token builder begins with the first character after the semicolon. `Reserved()` then appends the rest of the name. The resulting token text is likely `frac`, while the registry stores `;frac`.

Relevant code:

- `Celarix.Starfall/Libra/Parsing/Lexer.cs:136`
- `Celarix.Starfall/Libra/Parsing/OperatorRegistry.cs:48`

Decide whether token text includes the sigil. Either way can work, but the lexer and registry must agree.

## Validator Findings

### 20. Property value validation rejects values the grammar and builder need

The grammar allows `.` and `-` in property values:

- `Libra DSL Grammar.txt:25`
- `Libra DSL Grammar.txt:26`

The validator allows only identifier-continuation characters:

- `Celarix.Starfall/Libra/Parsing/LibraSyntaxValidator.cs:193`

The builder's color parser accepts HTML-style values and trims a leading `#`:

- `Celarix.Starfall/Rendering/Models/SColor.cs`
- `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:110`

So `[foreground=#ff0000]` is rejected by the validator, `[some=-1.5]` is rejected despite the grammar, and `[foreground=red]` passes validation but falls back to white because `SColor.FromHtmlAttribute` does not parse color names. The layers are not enforcing the same property language.

### 21. Property validation is syntactic, but property building is semantic and throws different exception types

`ValidatePropertyBlock` checks only `key=value` shape and identifier-like values. The builder later checks known keys and `FenceType`, then throws `InvalidOperationException` for unknown/invalid properties.

Relevant code:

- Validator: `Celarix.Starfall/Libra/Parsing/LibraSyntaxValidator.cs:132`
- Builder: `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:99`

For a user-facing DSL, all source-authored failures should probably become `LibraParseException` or a later `LibraBindException` carrying source spans. `InvalidOperationException` is a programmer-error signal, not a good parse/bind diagnostic.

### 22. Identifier validation parses classes/IDs but discards the parsed result

`ValidateIdentifierBlock` creates `results` but never returns it or attaches it to syntax.

Relevant code:

- `Celarix.Starfall/Libra/Parsing/LibraSyntaxValidator.cs:94`
- `Celarix.Starfall/Libra/Parsing/LibraSyntaxValidator.cs:128`

The builder reparses the original string later through `LibraId.Parse`. That duplicates parsing rules and risks drift. Prefer producing a structured ID/class representation during binding, or share the same identifier parser between validation and `LibraId.Parse`.

### 23. Duplicate ID policy inside one identifier block may not be enough

The validator catches duplicate `.class` or duplicate `#id` inside a single suffix. It does not enforce uniqueness of IDs across the built expression tree. There is a helper `LibraExpression.IdsAreUniqueOrThrow`, but the parse/build pipeline does not call it.

Relevant code:

- Duplicate check in one block: `Celarix.Starfall/Libra/Parsing/LibraSyntaxValidator.cs:123`
- Helper not called by build: `Celarix.Starfall/Libra/Expressions/LibraExpression.cs`

If duplicate IDs are invalid in Libra, `Build()` should call the uniqueness check before returning.

## Builder Findings

### 24. `Opacity` exists in `LibraBuildContext` but is not applied

`LibraBuildContext` carries `Opacity`, but `LibraExpressionBuilder` never applies it to text, operators, fences, fractions, or property blocks.

Relevant code:

- Context property: `Celarix.Starfall/Libra/Parsing/LibraBuildContext.cs:13`
- Builder color use without opacity: `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:16`, `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:35`, `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:85`

Either remove it for now or define an `opacity` property and consistently apply `SColor.WithOpacity(...)` at render/build time.

### 25. Substitution resolvers ignore surrounding context and ID

For substitutions, the builder returns `resolver()` directly:

- `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:58`

That means a substitution inside `[foreground=red]` will not inherit the foreground unless the resolver manually bakes it in. It also cannot receive the suffix ID being applied. This might be intended for complete prebuilt expression injection, but it is surprising in a layout DSL where property blocks are lexical/contextual.

Question: should substitutions produce fully styled expression trees, or should they be syntax-level macros/bound expressions that still receive the current `LibraBuildContext`?

### 26. Property block application order should be specified

The builder handles `PropertyBlockSyntax` by parsing the property block and building its expression under the new context:

- `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:53`

With postfix syntax, `x[foreground=red][background=blue]` should probably apply both properties. With the current AST shape, this likely works if parsed left-associatively, but it needs tests and a spec statement. More importantly, `[fencetype=SquareBrackets]` only has an effect when the property block wraps a parenthesized expression; on plain text it silently survives in context and has no visible effect. That may be acceptable, but it should be documented.

### 27. `FenceType` behavior is clever but surprising

The builder intentionally consumes `FenceType` only at the next parenthesized expression and clears it for nested parentheses:

- `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:77`

That is a reasonable model, but syntax like `{(x)}[fencetype=SquareBrackets]` versus `(x)[fencetype=SquareBrackets]` may not behave the way users expect unless suffix binding is precisely specified. Consider whether fence type should be a property on `ParenthesizedExpressionSyntax` specifically rather than an ambient context value.

### 28. Reserved-call binding is too expression-only for future built-ins

`ReservedFunctionInfo.Resolver` receives already-built `LibraExpression[]`.

Relevant code:

- `Celarix.Starfall/Libra/Parsing/ReservedFunctionInfo.cs:9`
- `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:68`

Your existing `AI Notes/Libra Reserved Call Binding Notes.md` already identifies why this becomes limiting for functions such as `;catEm(2, x, y)`: the first argument is a number, not a rendered text expression. I agree with that note. Reserved functions should bind from `ReservedCallSyntax`, using helpers for expression, number, integer, string, etc.

### 29. `;catEm` is used but not registered

The playground uses:

```text
;catEm(2, mt = 1 - t, ...)
```

But only `;frac` is present in `OperatorRegistry`.

Relevant code:

- Use: `Celarix.Starfall.Playground/DelphinusTests/DelphinusSlide.cs:39`
- Registry: `Celarix.Starfall/Libra/Parsing/OperatorRegistry.cs:46`

Even after the reserved-function list assignment is fixed, that sample expression cannot bind unless `;catEm` is implemented.

## Language/Syntax Concerns

### 30. The DSL is mixing display text and math identifiers without a clear boundary

The sample expression `mt = 1 - t` suggests math-like identifiers and operators. The parser's `TextSyntax` model suggests literal rendered text. The lexer currently treats runs of almost anything as text.

This creates ambiguous questions:

- Is `sin` one text token or a reserved/function-like operator?
- Is `mt` a multi-letter variable, two variables `m` and `t`, or literal text?
- Should `2x` be text, implicit multiplication, or invalid?
- Should whitespace affect rendered output or only separate tokens?

For a text layout library, literal text is necessary. For a math DSL, identifier semantics matter. I would explicitly separate:

- quoted literal text,
- bare math symbols/identifiers,
- reserved calls,
- substitutions/macros.

### 31. Reserved sigils should avoid collision with ordinary text

If semicolon remains the reserved-call sigil, text containing semicolons becomes syntactically active unless quoted. That may be acceptable, but it is not obvious for a text layout DSL.

If double-underscore remains the grammar choice, it is less punctuation-heavy but collides with identifier-looking text. Either choice is defensible. The important thing is to choose it deliberately and make escaping/quoting rules obvious.

### 32. Property syntax cannot currently represent enough real values

Sooner or later properties will want colors, dimensions, fonts, booleans, numbers, enum names, maybe strings. `key=value,key=value` with unquoted identifier-like values is a tight corner.

Possible direction:

- Keep simple values for now, but allow `#`, `.`, `-`, `%`, and maybe quoted strings.
- Parse property values into syntax/value tokens instead of raw strings.
- Bind properties through a registry with source-span-aware errors.

### 33. Braces as non-rendering grouping need to be in the grammar

The parser supports braces and the builder treats them as non-rendering grouping:

- Parser: `Celarix.Starfall/Libra/Parsing/LibraParser.cs:93`
- Builder: `Celarix.Starfall/Libra/Parsing/LibraExpressionBuilder.cs:87`

That is useful, especially for scripts and grouped property/ID suffixes. It just needs to be specified. In particular, script examples in error messages recommend `x^{y^z}`, but the grammar does not currently mention braces.

## Testing Gaps

I did not find a test project. Given the lexer state-machine issues, the next investment should be a compact parser test suite. Start with tokenization tests before AST/build tests.

Minimum lexer cases:

- `x`
- `xy`
- `"x"`
- `" x "`
- `"a\"b"`
- `a+b`
- `a<=b`
- `;frac(x,y)` or the final chosen reserved syntax
- `[[name]]`
- `x@.class#id`
- `x[foreground=#ff0000]`
- `(x)`
- `{x}`

Minimum parser/AST cases:

- `a+b*c`
- `a*b+c`
- `-x^2`
- `x^y_z`
- `x_y^z`
- `x^y^z`
- `;frac(a,b)`
- too many/few reserved-call args
- property blocks and identifier suffixes chained in both orders

Minimum builder cases:

- text/string preserve expected whitespace,
- property colors actually apply,
- fence type affects only the intended fenced expression,
- duplicate IDs are rejected if that is the intended policy,
- substitutions either inherit context or explicitly do not.

## Suggested Fix Order

1. Fix `LibraParseException` to store `Diagnostic`.
2. Fix `OperatorRegistry._reservedFunctions = [.. funcInfo];`.
3. Replace lexer peeking with a clear `Current`, `Peek(offset)`, `Advance()` model and retest every token kind.
4. Make `TextSpan` source-stable and update all token creation to use the same convention.
5. Align grammar and code on reserved syntax.
6. Implement suffix tokenization for `@...` and `[...]`.
7. Fix script parsing with explicit tests for same-precedence scripts.
8. Move reserved-call binding toward syntax-aware binders, as already described in the reserved-call note.
9. Expand property validation/binding into one source-span-aware layer.
10. Add a small test project before adding more operators/functions.

## Questions

1. Should reserved functions be spelled `;frac(...)` or `__frac(...)`?
2. Are bare words intended to be literal rendered text, math identifiers, or both?
3. Should whitespace outside quoted strings render, be ignored, or act only as a separator?
4. Should substitutions inherit the current property context and postfix ID/class suffixes?
5. Are IDs required to be unique across a built Libra expression tree?
6. Should `x^y_z` attach both scripts to `x`, or should it mean `x^(y_z)`?
7. Should property blocks be general object-style annotations, or only rendering-style context changes?

