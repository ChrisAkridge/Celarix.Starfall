# Starfall Implementation To-Do

This list tracks implementation status across the AI Notes design threads.

## Done

- [x] MeasurementService text measurement cache from #05.
- [x] AnimationContextRegistry from #07.
- [x] AtriaSlide and AtriaElement animation contexts routed through the registry from #02/#07.
- [x] Existing slide and element AnimationContext call sites migrated to the registry-owned model.
- [x] AnimateBasic and AtriaElement property animations moved onto AnimationContext.
- [x] Central registry ticking so all element-owned animation contexts advance every frame.
- [x] AnimationSlot first pass from #04.
- [x] ForceFinish() first pass for FixedDurationAnimation and ContinuingAnimation.
- [x] FloatingPointWindowElement migrated to slots for scroll, window movement, arrow movement, arrow opacity, and negative flag opacity.

## Current Priority

- [ ] Layers, transforms, and render context as the next larger phase.
  - [x] Define STransform2D or equivalent friendly transform helper.
  - [x] Decide the render-target push/pop API for transform and clipping scopes.
  - [x] Add layer model, likely LayeredAtriaSlide<TLayer>.
  - [x] Let layers pan/zoom independently.
  - [ ] Pass viewport/render context information down to AtriaElement.Render.
  - [ ] Add geometry helpers needed for transformed viewport visibility.
- [ ] Findings from the Libra Pratt Parser review
  - [x] Parser behavior tests and diagnostics.
    - [x] Add parser tests for script grouping: `x^y_z`, `x_y^z`, `x^y^z`, and `x_y_z`.
    - [x] Add parser tests that parenthesized and braced syntax spans include both opening and closing delimiters.
    - [x] Add parser tests that commas inside reserved calls remain valid argument separators.
    - [x] Add parser tests that top-level, parenthesized, and braced commas outside reserved calls produce the comma-specific diagnostic.
    - [x] Fix comma-specific diagnostics outside reserved calls; `Expect(TokenKind.CloseParen)` / `Expect(TokenKind.CloseBrace)` / top-level EOF checks should special-case actual comma tokens.
    - [x] Remove unused `ReservedFunctionWhenNoneRule.BindingPower` unless a concrete use appears.
  - [x] Reserved function binding pass based on `Libra Reserved Call Binding Notes.md`.
    - [x] Introduce a `LibraBinder` layer between `ExpressionSyntax` and `LibraExpression`.
    - [x] Keep parser responsibility limited to syntax shape; do not enforce reserved-call existence, arity, or typed arguments in the parser.
    - [x] Add `IReservedCallBinder` or equivalent function-specific binder abstraction.
    - [x] Add a reserved-call registry mapping semicolon reserved names to binders.
    - [x] Move `;frac` construction into a `FractionBinder` that validates arity and binds both arguments as expressions.
    - [x] Add `;catEm(gapInEm, expression1, expression2, ...)` binder with conservative numeric binding for the gap and expression binding for the remaining args.
    - [x] Add binder helper methods only as needed, starting with `BindExpression`, `BindNumber`, and possibly `BindString` / `BindInteger`.
    - [x] Ensure binder diagnostics carry source spans and do not require extracting typed values back out of built `LibraExpression` objects.
  - [x] Property block binding pass.
    - [x] Define valid property keys in code rather than scattering string switches through the builder.
    - [x] Give each property definition an accepted value type and parsing/validation routine.
    - [x] Support property values needed by current properties, especially HTML-style colors without # characters such as `ff0000` and enum values such as `FenceType`.
    - [x] Produce source-span-aware diagnostics for unknown properties, malformed values, and unsupported value types.
    - [x] Keep property validation/binding aligned with the future binder model so the parser only recognizes property-block syntax.
  - [ ] Libra postfix syntax validation pass.
    - [ ] Reject direct identifier/property postfix blocks on substitutions, such as `[[Substitution]]@#id` or `[[Substitution]][color=red]`.
    - [ ] Allow at most one identifier block and one property block per expression.
    - [ ] Require identifier blocks to contain at most one `#id` and arbitrarily many classes.
    - [ ] Require repeated properties to be expressed inside one property block and diagnose duplicate/conflicting keys through the property binder.
    - [x] Diagnose `fencetype` when it is attached to anything other than a parenthesized expression.
    - [x] Decide and test whether non-rendering braces may forward `fencetype` to an enclosed parenthesized expression.
  - [x] Libra bare atom lexer rules.
    - [x] Restrict bare text atoms to `[A-Za-z0-9.]+`.
    - [x] Treat whitespace outside strings as syntactic separation and discard it.
    - [x] Require literal semicolons and other punctuation to appear in quoted strings unless they are recognized Libra syntax.
    - [x] Add lexer tests for decimal atoms such as `2.5`, whitespace-normalized expressions, quoted punctuation, and unquoted punctuation diagnostics.
    - [x] Remove old unused lexer state-machine members and methods after confirming no code path still uses them.
  - [ ] Global Atria/Libra ID system pass.
    - [ ] Review how `LibraId` relates to `AtriaId` and the shared `Identification` parsing/matching rules.
    - [ ] Decide where ID uniqueness should be enforced: per Libra expression tree, per Atria slide/layer, globally registered, or some combination.
    - [ ] Investigate whether IDs/classes should be registered for query, replacement, animation, renderable lookup, or diagnostics.
    - [ ] Centralize ID parsing/validation behavior so Libra identifier blocks and Atria IDs cannot drift.
    - [ ] Add tests around duplicate IDs, class matching, ID matching, and cross-system selector behavior.

## Backburner

- [ ] SteppedAtriaSlide / choreography helper from #03.
- [ ] TextBlockFactory and related constructor cleanup from #02.
- [ ] FrameScheduler from #08.

## Later Cleanup

- [ ] Remove now-redundant manual Animations.Update(...) calls from slides/elements.
- [ ] Split continuing animations into finite-until-stable vs. infinite/ambient animation concepts.
- [ ] Add ScheduleWhenAnimationsStabilize or equivalent registry/context API.
- [ ] Add tests around AnimationContextRegistry, AnimationSlot replacement policy, and ForceFinish behavior.
