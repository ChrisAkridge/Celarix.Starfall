using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Syntax;
using Celarix.Starfall.Rendering.Models;
using System.Globalization;

namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal sealed class LibraBinder
    {
        public LibraBuildContext Context { get; }
        public string? LibraId { get; }

        public LibraBinder(LibraBuildContext context)
            : this(context, null)
        {
        }

        internal LibraBinder(LibraBuildContext context, string? libraId)
        {
            Context = context;
            LibraId = libraId;
        }

        public LibraExpression BindExpression(ExpressionSyntax syntax)
        {
            if (syntax is TextSyntax text)
            {
                return new TextExpression(text.Text,
                    Context.ForegroundColor.WithOpacity(Context.Opacity),
                    Context.BackgroundColor.WithOpacity(Context.Opacity),
                    LibraId);
            }
            else if (syntax is StringSyntax @string)
            {
                return new TextExpression(@string.Text,
                    Context.ForegroundColor.WithOpacity(Context.Opacity),
                    Context.BackgroundColor.WithOpacity(Context.Opacity),
                    LibraId);
            }
            else if (syntax is PrefixSyntax prefix)
            {
                var operand = BindExpression(prefix.Operand);
                return new UnaryPrefixExpression(OperatorRegistry.GetRenderedSymbol(prefix.Operator, OperatorKind.Prefix),
                    operand,
                    Context.ForegroundColor.WithOpacity(Context.Opacity),
                    Context.BackgroundColor.WithOpacity(Context.Opacity),
                    LibraId);
            }
            else if (syntax is BinarySyntax binary)
            {
                var left = BindExpression(binary.Left);
                var right = BindExpression(binary.Right);
                return new BinaryExpression(OperatorRegistry.GetRenderedSymbol(binary.Operator, OperatorKind.Infix),
                    left,
                    right,
                    Context.ForegroundColor.WithOpacity(Context.Opacity),
                    Context.BackgroundColor.WithOpacity(Context.Opacity),
                    LibraId);
            }
            else if (syntax is ScriptSyntax script)
            {
                var @base = BindExpression(script.Base);
                var superscript = script.Superscript is not null ? BindExpression(script.Superscript) : null;
                var subscript = script.Subscript is not null ? BindExpression(script.Subscript) : null;
                return new ScriptsExpression(@base, superscript, subscript, LibraId);
            }
            else if (syntax is IdentifierSyntax identifier)
            {
                return WithId(identifier.IdentifierBlock).BindExpression(identifier.Expression);
            }
            else if (syntax is PropertyBlockSyntax propertyBlock)
            {
                var newContext = ParsePropertyBlock(propertyBlock.PropertyBlock, Context);
                return WithContext(newContext).BindExpression(propertyBlock.Expression);
            }
            else if (syntax is SubstitutionSyntax substitution)
            {
                if (!Context.SubstitutionResolvers.TryGetValue(substitution.Name, out var resolver))
                {
                    throw CreateValidationException(substitution,
                        $"No substitution resolver registered for name '{substitution.Name}'.");
                }
                return resolver();
            }
            else if (syntax is ReservedCallSyntax reservedCall)
            {
                if (!ReservedCallRegistry.TryGetBinder(reservedCall.Name, out var reservedCallBinder))
                {
                    throw CreateValidationException(reservedCall,
                        $"Unknown reserved function '{reservedCall.Name}'.");
                }

                return reservedCallBinder.Bind(reservedCall, this);
            }
            else if (syntax is ParenthesizedExpressionSyntax parenthesized)
            {
                var fenceType = Context.FenceType ?? FenceType.Parentheses;
                var newContext = Context with { FenceType = null };
                var innerExpression = WithContext(newContext).WithId(null).BindExpression(parenthesized.Expression);
                return new FencedExpression(innerExpression,
                    fenceType,
                    newContext.ForegroundColor,
                    newContext.BackgroundColor,
                    LibraId);
            }
            else if (syntax is BracedExpressionSyntax braced)
            {
                return BindExpression(braced.Expression);
            }

            throw new NotSupportedException($"Unsupported syntax type: {syntax.GetType().Name}");
        }

        public double BindNumber(ExpressionSyntax syntax)
        {
            if (syntax is TextSyntax text
                && double.TryParse(text.Text,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var value))
            {
                return value;
            }

            if (syntax is PrefixSyntax { Operator: "+", Operand: TextSyntax positiveText }
                && double.TryParse(positiveText.Text,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var positiveValue))
            {
                return positiveValue;
            }

            if (syntax is PrefixSyntax { Operator: "-", Operand: TextSyntax negativeText }
                && double.TryParse(negativeText.Text,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var negativeValue))
            {
                return -negativeValue;
            }

            throw CreateValidationException(syntax, "Expected a numeric literal.");
        }

        public LibraParseException CreateValidationException(ExpressionSyntax syntax,
            string message)
        {
            return new LibraParseException(new(message, syntax.Span));
        }

        private LibraBinder WithContext(LibraBuildContext context) => new(context, LibraId);

        private LibraBinder WithId(string? libraId) => new(Context, libraId);

        private static LibraBuildContext ParsePropertyBlock(string propertyBlock,
            LibraBuildContext context)
        {
            var newContext = context;
            var properties = propertyBlock.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var property in properties)
            {
                var parts = property.Split('=', StringSplitOptions.TrimEntries);
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                switch (key.ToLowerInvariant())
                {
                    case "foreground":
                        newContext = newContext with { ForegroundColor = SColor.FromHtmlAttribute(value, SColor.White) };
                        break;
                    case "background":
                        newContext = newContext with { BackgroundColor = SColor.FromHtmlAttribute(value, SColor.Black) };
                        break;
                    case "fencetype":
                        if (!Enum.TryParse<FenceType>(value, true, out var fenceType))
                        {
                            throw new InvalidOperationException($"Invalid fence type: '{value}'");
                        }
                        newContext = newContext with { FenceType = fenceType };
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown property key: '{key}'");
                }
            }
            return newContext;
        }
    }
}
