# Libra Lexer Redesign Roadmap

Date: 2026-08-14

## Goal

Replace the current lexer with a simpler scanner whose job is only to recognize source tokens accurately. It should not try to decide whether a suffix is legal on the preceding expression, whether a property applies to the right expression kind, or whether a reserved call has valid arguments. Those are parser, validator, or binder responsibilities.

The current `MoveNext()` / `PeekNext()` design is too easy to misuse because it cannot cleanly distinguish current-character inspection from lookahead. The replacement should use an explicit cursor model and a small set of token readers.

## Language Rules To Preserve

- Bare text atoms are math-style text atoms, not semantic identifiers.
- Bare text atoms match `[A-Za-z0-9.]+`.
- Whitespace outside strings is syntactic separation and is discarded.
- Quoted strings preserve spaces and punctuation.
- Literal semicolons and other punctuation must appear in strings unless they are recognized Libra syntax.
- Semicolon starts reserved names/calls and is part of the reserved token text, for example `;frac`.
- Commas are tokens, but valid only inside reserved-call argument lists.
- Substitution syntax is `[[Name]]`.
- Direct postfix blocks on substitutions are illegal, but that should be diagnosed after lexing.
- Identifier suffix blocks begin with `@` and contain one `#id` at most plus arbitrarily many `.class` entries.
- Property suffix blocks begin with `[` and end with `]`.
- Only one identifier block and one property block are allowed per expression, but that is not a lexer decision.
- `fencetype` is valid only when attached to a parenthesized expression, but that is not a lexer decision.

## Proposed Cursor API

Use a cursor with direct current-character inspection:

```csharp
private bool IsAtEnd => _position >= _source.Length;
private char Current => _source[_position];
private char? Peek(int offset = 1);
private char Advance();
private bool Match(char expected);
private TextSpan SpanFrom(int start);
```

Important conventions:

- `Current` never advances.
- `Peek(1)` means the next character after `Current`.
- `Advance()` returns the current character and then moves forward.
- Reader methods record `start = _position` before consuming.
- Token spans are source-stable. Every token span should reference the same source identity/string or should store only `StartIndex` and `Length`.
- Token text should not be globally trimmed. If trimming is needed, do it per token kind and only when it is semantically correct.

This removes the current off-by-one trap where code looks at one character but consumes another.

## Token Kinds

Keep the existing broad token categories, but make their recognition precise:

- `Text`: `[A-Za-z0-9.]+`
- `String`: quoted text with escapes
- `ReservedName`: `;` plus identifier characters
- `Substitution`: `[[` name `]]`
- `IdentifierBlock`: `@` followed by raw identifier-block contents
- `PropertyBlock`: `[` followed by raw property-block contents and ending at `]`
- `Operator`: longest match from `OperatorRegistry`
- delimiters: `(`, `)`, `{`, `}`, `,`
- `EndOfInput`

The lexer can still return raw text for `IdentifierBlock` and `PropertyBlock`; detailed validation of `#id`, `.class`, property names, duplicate keys, and typed property values can happen later.

## Scanning Loop

The main loop should be a straightforward dispatcher:

```text
while not at end:
    if whitespace: skip
    else if current == '"': read string
    else if current == ';': read reserved name
    else if current == '[' and peek == '[': read substitution
    else if current == '[': read property block
    else if current == '@': read identifier block
    else if current is delimiter: emit delimiter
    else if current begins operator: read longest operator
    else if current is bare atom char: read text atom
    else: diagnostic for unexpected character
emit EndOfInput
```

This is intentionally not state-machine-heavy. Most tokens have obvious first characters, so the lexer can stay direct and local.

## Token Readers

### Whitespace

Skip whitespace outside strings. Do not emit whitespace tokens for now.

This makes:

```text
x+y
x + y
x   +   y
```

equivalent at the token level.

### Bare Text Atom

Read while the current character is ASCII letter, ASCII digit, or `.`.

Open question for implementation: a lone `.` technically matches `[A-Za-z0-9.]+`, but it is probably not useful as a text atom. The simplest first pass can allow it because punctuation policy already says only recognized syntax has special meaning; a stricter pass can require at least one letter/digit if desired.

### String

Read from opening `"` to closing `"`.

String token text should be the decoded string content, not including quotes. It should preserve ordinary spaces and punctuation.

Escapes to support initially:

- `\"`
- `\\`

If an unknown escape is encountered, prefer a source diagnostic over silently changing the text. More escapes can be added only when needed.

### Reserved Name

Read `;` plus reserved-name characters.

The emitted token text should include the semicolon because that is the planned convention and the current bug is that the sigil can be dropped. For example:

```text
;frac
```

emits:

```text
ReservedName(";frac")
```

The lexer should diagnose bare `;` with no following name. Whether the name is known is not a lexer decision.

### Substitution

Read `[[Name]]`.

The emitted token text can be just the substitution name, not the brackets. The lexer should validate the closing `]]` and basic name shape because the delimiters define the token boundary.

Do not decide whether `[[Name]]@#id` is legal in the lexer. It should tokenize as substitution plus identifier block, then the validator can report "substitutions cannot receive direct postfix blocks" with a better structural diagnostic.

### Identifier Block

Read from `@` until the next character that cannot belong to the identifier block.

Allowed raw characters inside the block:

- `#`
- `.`
- ASCII letters
- ASCII digits
- `_`

The lexer can emit token text without the leading `@`, for example:

```text
x@#id.class1.class2
```

emits:

```text
Text("x")
IdentifierBlock("#id.class1.class2")
```

Detailed rules belong in validation:

- at most one `#id`,
- arbitrarily many `.class` entries,
- no duplicate classes if that remains desired,
- no empty entries,
- only one identifier block per expression.

### Property Block

Read from `[` to the next `]`, except `[[` is substitution.

The emitted token text can be the raw content without brackets:

```text
x[foreground=#ff0000,fencetype=SquareBrackets]
```

emits:

```text
Text("x")
PropertyBlock("foreground=#ff0000,fencetype=SquareBrackets")
```

The lexer should only diagnose missing closing `]`. It should not parse property keys, duplicate properties, colors, enum values, or whether `fencetype` is attached to a parenthesized expression.

Nested `[` inside property blocks should probably be illegal initially. If property values later need strings or nested syntax, that should be a deliberate property-value parser feature, not a lexer accident.

### Operators

Use the operator registry for longest-match scanning.

The operator scanner should ask the registry for the longest operator that begins at the current position, not emit a single-character operator immediately. This avoids splitting `<=` into `<` and `=`.

Useful registry API:

```csharp
public static bool TryMatchLongestOperator(
    string source,
    int start,
    out string symbol);
```

The trie can still be used internally, but it should be defensive when a character is not in the operator alphabet.

### Delimiters

Emit these as single-character tokens:

- `(`
- `)`
- `{`
- `}`
- `,`

Do not reject top-level `,` in the lexer. The parser knows whether it is inside a reserved-call argument list and can produce a better diagnostic.

## What The Lexer Should Not Decide

The lexer should not decide:

- whether a reserved name exists,
- whether a reserved call has valid arity,
- whether commas are in a valid context,
- whether an identifier block is repeated on the same expression,
- whether a property block is repeated on the same expression,
- whether a substitution can receive postfix blocks,
- whether `fencetype` targets a parenthesized expression,
- whether a property key exists or has the right value type,
- whether an ID is globally unique.

Those checks require syntax structure, binding context, or global ID policy. Doing them in the lexer will either produce worse diagnostics or push grammar knowledge into the wrong layer.

## Diagnostic Strategy

Lexer diagnostics should be limited to local source facts:

- unexpected character,
- unterminated string,
- invalid escape sequence,
- unterminated substitution,
- invalid substitution name character,
- unterminated property block,
- bare `;` with no reserved name,
- invalid character immediately after `@` if no identifier block content follows.

Parser/validator/binder diagnostics should handle structural and semantic rules.

## Roadmap

### Phase 1: Stabilize Token Infrastructure

- Replace `TextSpan` usage so every span is source-stable.
- Store `LibraParseException.Diagnostic`.
- Stop trimming all token text in `LibraToken`.
- Add a small lexer test suite before replacing behavior.

### Phase 2: Replace The Cursor And Scanner

- Implement the explicit cursor API.
- Replace the state machine with the direct scanning loop.
- Implement readers for whitespace, text atom, string, reserved name, substitution, identifier block, property block, delimiter, and operator.
- Add longest-operator matching in `OperatorRegistry`.

### Phase 3: Lock Down Lexical Rules With Tests

Minimum lexer tests:

- `x`
- `xy`
- `2.5`
- `mt = 1 - t`
- `hello world` tokenizes as `hello`, `world` with whitespace discarded
- `"hello world"` preserves the space
- `"a\"b"`
- `"a;b"` preserves the semicolon
- unquoted punctuation fails unless it is recognized syntax
- bare `;` fails
- `;frac(x,y)` includes `;frac` as token text
- `a<=b` emits `<=`
- top-level `a,b` tokenizes but parser rejects later
- `[[name]]`
- `[[name]]@#id` tokenizes, then validator rejects later
- `x@#id.class1.class2`
- `x[foreground=#ff0000]`
- `x[color=red][background=blue]` tokenizes, then validator rejects later
- `(x)[fencetype=SquareBrackets]`
- `x[fencetype=SquareBrackets]` tokenizes, then validator/binder rejects later

### Phase 4: Move Structural Checks Outward

- Add validation for direct postfix blocks on substitutions.
- Add validation for at most one identifier block and one property block per expression.
- Add validation for one `#id` and many classes in one identifier block.
- Add validation or binding diagnostics for duplicate/conflicting property keys.
- Add binding diagnostics for `fencetype` on non-parenthesized targets.

### Phase 5: Connect To Binder Work

- Keep reserved-call arguments as syntax.
- Move reserved-call existence, arity, and typed argument binding into `LibraBinder` and reserved-call binders.
- Move property key/value validation into property binding definitions.
- Keep the lexer dumb and predictable as the language grows.

## Recommended Shape Of The Replacement

I would not try to patch the current state enum. The replacement should be a small hand-written scanner with explicit token readers. Libra's token language is not complicated enough to justify a subtle state machine, and the known bugs are exactly the kind produced by a scanner whose primitive operations are hard to reason about.

The main quality bar is that every reader should be locally understandable:

- inspect current,
- record start,
- consume exactly the token,
- emit token with exact text and span,
- leave `_position` at the first character after the token.

If the scanner follows that invariant, the parser and binder work become much easier to reason about.

