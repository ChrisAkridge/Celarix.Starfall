using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Syntax;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal sealed class LibraExpressionBuilder
    {
        public LibraExpression Build(ExpressionSyntax syntax, LibraBuildContext context, string? libraId = null)
        {
            if (syntax is TextSyntax text)
            {
                return new TextExpression(text.Text, context.ForegroundColor.WithOpacity(context.Opacity), context.BackgroundColor.WithOpacity(context.Opacity), libraId);
            }
            else if (syntax is StringSyntax @string)
            {
                return new TextExpression(@string.Text, context.ForegroundColor.WithOpacity(context.Opacity), context.BackgroundColor.WithOpacity(context.Opacity), libraId);
            }
            else if (syntax is PrefixSyntax prefix)
            {
                var operand = Build(prefix.Operand, context);
                return new UnaryPrefixExpression(OperatorRegistry.GetRenderedSymbol(prefix.Operator),
                    operand,
                    context.ForegroundColor.WithOpacity(context.Opacity),
                    context.BackgroundColor.WithOpacity(context.Opacity),
                    libraId);
            }
            else if (syntax is BinarySyntax binary)
            {
                var left = Build(binary.Left, context);
                var right = Build(binary.Right, context);
                return new BinaryExpression(OperatorRegistry.GetRenderedSymbol(binary.Operator),
                    left,
                    right,
                    context.ForegroundColor.WithOpacity(context.Opacity),
                    context.BackgroundColor.WithOpacity(context.Opacity),
                    libraId);
            }
            else if (syntax is ScriptSyntax script)
            {
                var @base = Build(script.Base, context);
                var superscript = script.Superscript is not null ? Build(script.Superscript, context) : null;
                var subscript = script.Subscript is not null ? Build(script.Subscript, context) : null;
                return new ScriptsExpression(@base, superscript, subscript, libraId);
            }
            else if (syntax is IdentifierSyntax identifier)
            {
                return Build(identifier.Expression, context, identifier.IdentifierBlock);
            }
            else if (syntax is PropertyBlockSyntax propertyBlock)
            {
                var newContext = ParsePropertyBlock(propertyBlock.PropertyBlock, context);
                return Build(propertyBlock.Expression, newContext, libraId);
            }
            else if (syntax is SubstitutionSyntax substitution)
            {
                if (!context.SubstitutionResolvers.TryGetValue(substitution.Name, out var resolver))
                {
                    throw new InvalidOperationException($"No substitution resolver registered for name '{substitution.Name}'");
                }
                return resolver();
            }
            else if (syntax is ReservedCallSyntax reservedCall)
            {
                var resolvedArguments = reservedCall.Arguments.Select(a => Build(a, context)).ToArray();
                if (!OperatorRegistry.TryGetKnownReservedFunction(reservedCall.Name, out var reservedFunctionInfo))
                {
                    throw new InvalidOperationException($"Unknown reserved function: '{reservedCall.Name}'");
                }

                var resolver = reservedFunctionInfo.Resolver;
                return resolver(context, libraId, resolvedArguments);
            }
            else if (syntax is ParenthesizedExpressionSyntax parenthesizedExpressionSyntax)
            {
                // Most properties apply to everything inside the expression, except for FenceType.
                // FenceType is applied only to this parenthesized expression, and then does not apply
                // to any parenthesized expressions inside of it.
                var fenceType = context.FenceType ?? FenceType.Parentheses;
                var newContext = context with { FenceType = null };
                var innerExpression = Build(parenthesizedExpressionSyntax.Expression, newContext); // don't carry the ID down
                return new FencedExpression(innerExpression, fenceType, newContext.ForegroundColor, newContext.BackgroundColor, libraId);
            }
            else if (syntax is BracedExpressionSyntax bracedExpression)
            {
                // Braced expressions don't render, they're just ways to group expressions so that they
                // can all be identified or given properties at once.
                return Build(bracedExpression.Expression, context, libraId);
            }
            else
            {
                throw new NotSupportedException($"Unsupported syntax type: {syntax.GetType().Name}");
            }
        }

        private LibraBuildContext ParsePropertyBlock(string propertyBlock, LibraBuildContext context)
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
