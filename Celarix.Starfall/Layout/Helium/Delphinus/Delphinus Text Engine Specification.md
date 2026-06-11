# Delphinus
An XML-based text rendering and layout engine for Helium. It is designed to be the advanced text option compared to `TextElement` and supports a wide range of features. To use Delphinus, instantiate an `XmlTextElement` and set its `Xml` property to a string containing the XML markup that defines the text content and its formatting.

## Features

- Support for a wide range of text formatting options, including font size, color, weight, and more.
- Supports basic mathematical expressions, such as fractions, square roots, and superscripts/subscripts. Mathematical elements can be nested within each other to create more complex expressions.
- Elements can be given IDs and class names that can be selected via `SelectionQuery` as with any other renderable, allowing specific text spans or elements to be targeted in transitions or animation.

## Notes

Delphinus does not implement the CSS box model. There is one kind of block element, the `<p>` paragraph element. Block elements can be nested in each other in the markup but will be separated into adjacent blocks when rendering. No element has margin, border, or padding.

For example, the below:

```html
<p>
	This is some text
	<p>
		This is some more text
	</p>
</p>
```

can be specified but will be rendered as:

```
<p>This is some text</p>
<p>This is some more text</p>
```

## XML Structure

### Text Elements

The `<xml>` tags are optional and will be included automatically if you don't specify them. A valid Delphinus XML document is made of the following tags:
- `<p>`: A block element representing a paragraph. Supports inline text spans and inline image elements.
- `<span>`: An inline element representing a span of text with specific formatting. Can be nested within other inline elements.
- `<br>`: A line break element that provides separation between block elements. It is a self-closing tag and can be written as `<br />` or simply `<br>`.
- `<hr>`: A horizontal rule element that provides separation between block elements and draws a line. It is a self-closing tag and can be written as `<hr />` or simply `<hr>`.
- `<img>`: An inline element representing an image. It is recommended to use images that aren't much taller than the line height to avoid layout issues.
- `<mathblock>`: This is shorthand for `<p halign="center" valign="center">`. It is a block element that centers its content both horizontally and vertically. It is intended to be used for mathematical expressions, but can be used for any content.

### Mathematical Elements
Delphinus also supports a number of tags that can be used to build common mathematical expression. It is not nearly as powerful as LaTeX, but can be used to create simple expressions.

- `<sup>`: An inline element representing superscript text. It is typically used for exponents or footnotes.
- `<sub>`: An inline element representing subscript text. It is typically used for chemical formulas or mathematical indices.
- `<frac>`: An inline element representing a fraction. It contains two child elements, `<num>` and `<den>`, which represent the numerator and denominator of the fraction, respectively.
- `<root>`: An inline element representing a root. It contains two child elements, `<index>` and `<radicand>`, which represent the index and radicand of the root, respectively. If the `<index>` element is omitted, it is not drawn.

Some expression elements have different appearance when rendered. Each of these has three child elements, `<lower>`, `<upper>`, and `<expr>`. When rendered inline, the lower and upper elements are rendered as subscript and superscript, respectively. When rendered in the root of a block element, the lower and upper elements are rendered below and above the summation symbol, respectively. The symbol used for the element is larger in a mathblock as well.

- `<sum>`: Represents a summation expression.
- `<prod>`: Represents a product expression.
- `<int>`: Represents an integral expression.
- `<lim>`: Represents a limit expression.

## Attributes

Delphinus does not support CSS or cascading style mechanisms. Instead, all formatting is done via attributes on the individual elements. The following attributes are supported on all elements:

- `id`: A unique identifier for the element. This can be used to target the element in transitions or animations.
- `class`: A space-separated list of class names for the element. This can be used to target the element in transitions or animations.
- `fontfamily`: The font family to use for the text. If not found, the default font family, currently Arial, will be used.
- `fontsize`: The font size to use for the text. This can be specified in points (e.g. `12`) or as a percentage of the font size of the outer element (e.g., `150%`), depending on whether a percent sign is present. If zero or negative, a default size of 100% will be used instead. If specified on the root element, it is relative to a default font size of 12 points.
- `fontstyle`: The font style to use for the text. TODO: provide reasonable options
- `color`: The color to use for the text, specified in one of the following formats:
	- Hexadecimal RGB (e.g. `#ff0000` for red)
	- Hexadecimal RGBA (e.g. `#ff000080` for semi-transparent red)
	- Hexadecimal shorthand RGB (e.g. `#f00` for red)
	- Hexadecimal shorthand RGBA (e.g. `#f008` for semi-transparent red)
	- Color name (e.g. `red`, `blue`, `green`, etc.)
- `bgcolor`: The color of the background behind the text, specified in the same formats as `color`. The background will be drawn behind only the span or block of text of the element it is defined on.
- `halign`: The horizontal alignment of the text within its container. Valid values are `left`, `center`, and `right`. The default value is `left`. This attribute is only applicable to block-level elements, and will be ignored on inline elements.
- `valign`: The vertical alignment of the text within its container. Valid values are `top`, `center`, and `bottom`. The default value is `top`. This attribute is only applicable to block-level elements, and will be ignored on inline elements.
- `linemargin`: The amount of vertical space to add between lines of text in a block element, specified as a multiple of the height of a line of text at the font size of the element that contains nothing that extends the line's height. For example, if the line contains only text at 12 points and the `linemargin` is set to `0.5`, then there will be an additional 6 points of space between that line and the next. This applies only to block-level elements and will be ignored on inline elements. The default value is `0`, meaning that there is no extra space added.

## Mathematical Layout Engine Notes

### Layout Model

Delphinus mathematical rendering is based on a recursive box layout model. Each mathematical element is measured independently and produces a layout box that reports metrics used by its parent element during layout.

Mathematical layout boxes are not defined solely by width and height. In addition to size, boxes also report alignment metrics that allow neighboring expressions to align consistently.

A mathematical layout box conceptually contains:

* Width
* Height above baseline
* Depth below baseline
* Baseline position
* Math axis position

The baseline represents the line ordinary text sits on. The math axis represents the visual balancing line used by mathematical operators and structures such as fractions, sums, and delimiters.

For example, a fraction does not vertically center its contents within its total bounds. Instead, the vinculum is positioned near the math axis, with the numerator above and denominator below.

Mathematical elements are laid out recursively. Child elements are measured first, then parent elements determine placement offsets and resulting metrics.

### Rendering Strategy

Delphinus mathematical rendering is intended to prioritize stable animated layout and semantic structure over exact TeX compatibility.

Mathematical elements are rendered procedurally rather than relying entirely on font-provided glyph composition. This allows individual components of expressions to maintain stable identity during transitions and animations.

For example, a fraction may internally consist of:

* Numerator box
* Vinculum box
* Denominator box

A root expression may internally consist of:

* Radical symbol
* Vinculum
* Index
* Radicand

These components may be individually targeted during animation.

### Mathematical Spacing

Mathematical spacing is determined semantically rather than purely by glyph advance widths.

Expressions are categorized into broad atom types that determine spacing behavior between adjacent elements. These categories are intentionally simpler than TeX's full spacing system but are designed to produce visually readable mathematical layout.

Possible atom categories include:

* Ordinary
* Operator
* Binary operator
* Relation
* Opening delimiter
* Closing delimiter
* Punctuation
* Large operator
* Structural expression

Examples:

* Multiplication binds visually tighter than addition.
* Equality operators receive larger surrounding spacing than binary operators.
* Parentheses and delimiters bind tightly to enclosed expressions.

Spacing behavior may evolve independently of font metrics in order to improve readability and animation stability.

### Stretchy Delimiters

Large delimiters such as parentheses, brackets, braces, and radical symbols are procedurally constructed when expressions exceed the size practical for ordinary glyph rendering.

Procedural delimiters are intended to preserve:

* Stable stroke thickness
* Consistent curvature
* Arbitrary height support
* Animation friendliness

Stretchy delimiters are not required to exactly match TeX or font-provided delimiter construction behavior.

Delimiter construction may use Bézier curves, repeated extender segments, or procedurally generated geometry.

### Radicals

Root symbols are rendered procedurally rather than by vertically scaling ordinary glyphs.

A root expression consists conceptually of:

* Radical hook
* Vinculum
* Optional index
* Radicand

The radical symbol stretches dynamically to accommodate the size of the radicand.

Nested radicals are expected to trigger recursive resizing of parent radicals and surrounding layout.

### Animation Identity

Mathematical elements preserve semantic identity through rendering and layout changes.

Elements with IDs or class names may retain continuity across transitions even when their rendered geometry changes substantially.

For example:

```xml
<span id="a">2<sup>7</sup></span>
```

may later become:

```xml
<span id="a">128</span>
```

During animation, Delphinus may treat the expression as the same semantic object whose rendered contents changed rather than as unrelated geometry.

This allows transitions to animate mathematical meaning rather than solely morphing vector paths.

### Scope

Delphinus mathematical layout is intended to support visually expressive animated mathematical explanations rather than complete TeX compatibility.

The initial implementation is expected to support:

* Superscripts and subscripts
* Fractions
* Roots
* Large operators
* Limits
* Semantic spacing
* Stretchy delimiters
* Recursive mathematical layout

Additional mathematical constructs may be added incrementally over time.
