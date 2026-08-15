using Celarix.Starfall.Libra.Parsing.Rules;
using Celarix.Starfall.Libra.Parsing.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Celarix.Starfall.Libra.Parsing
{
    internal sealed class LibraParser
    {
        private readonly IReadOnlyList<LibraToken> _tokens;
        private readonly IdentifierRule _identifierRule = new();
        private readonly PropertyBlockRule _propertyRule = new();
        private readonly ReservedFunctionWhenNoneRule _reservedFunctionRule = new();
        private int _position;

        public LibraParser(IEnumerable<LibraToken> tokens)
        {
            _tokens = [.. tokens];

            if (_tokens.Count == 0
                || _tokens[^1].Kind != TokenKind.EndOfInput)
            {
                throw new ArgumentException("Tokens must end with an EndOfInput token.", nameof(tokens));
            }
        }

        private LibraToken Read()
        {
            if (_position >= _tokens.Count)
            {
                return _tokens[_tokens.Count - 1];
            }
            
            var result = _tokens[_position];
            _position += 1;
            return result;
        }

        internal LibraToken Peek()
        {
            if (_position >= _tokens.Count)
            {
                return _tokens.Last();
            }
            return _tokens[_position];
        }

        internal LibraToken Expect(TokenKind kind)
        {
            var token = Read();
            if (token.Kind != kind)
            {
                throw CreateUnexpectedTokenException(token, $"token of kind {kind}");
            }
            return token;
        }

        internal ExpressionSyntax Parse()
        {
            var expression = ParseExpression();
            var endOfInputToken = Read();
            if (endOfInputToken.Kind != TokenKind.EndOfInput)
            {
                throw CreateUnexpectedTokenException(endOfInputToken, "end of input");
            }
            return expression;
        }

        internal ExpressionSyntax ParseExpression(int minimumBindingPower = 0)
        {
            var token = Read();
            var left = ParseWhenNone(token);

            while (TryGetWhenSome(Peek(), out var rule)
                && rule.LeftBindingPower >= minimumBindingPower)
            {
                var nextToken = Read();
                left = rule.Parse(this, left, nextToken);
            }
            return left;
        }

        private ExpressionSyntax ParseWhenNone(LibraToken token) => token.Kind switch
        {
            TokenKind.Text => new TextSyntax(token.Text, token.Span),
            TokenKind.String => new StringSyntax(token.Text, token.Span),
            TokenKind.ReservedName => Peek().Kind == TokenKind.OpenParen
                ? _reservedFunctionRule.Parse(this, token)
                : new ReservedNameSyntax(token.Text, token.Span),
            TokenKind.Substitution => new SubstitutionSyntax(token.Text, token.Span),
            TokenKind.Operator => OperatorRegistry.TryGetWhenNone(token.Text, OperatorKind.Prefix, out var operatorRule)
                ? operatorRule.Parse(this, token)
                : throw new LibraParseException(new($"Unexpected operator '{token.Text}' of kind {token.Kind} at {token.Span}.", token.Span)),
            TokenKind.OpenParen => new ParenthesizedExpressionSyntax(ParseExpression(), TextSpan.FromBounds(token.Span, Expect(TokenKind.CloseParen).Span)),
            TokenKind.OpenBrace => new BracedExpressionSyntax(ParseExpression(), TextSpan.FromBounds(token.Span, Expect(TokenKind.CloseBrace).Span)),
            _ => throw new LibraParseException(new($"Unexpected token '{token.Text}' of kind {token.Kind} at {token.Span}.", token.Span))
        };

        private bool TryGetWhenSome(LibraToken token,
            [NotNullWhen(true)] out IWhenSomeRule? rule)
        {
            switch (token.Kind)
            {
                case TokenKind.Operator or TokenKind.ReservedName:
                    return OperatorRegistry.TryGetWhenSome(token.Text, OperatorKind.Infix, out rule);
                case TokenKind.IdentifierBlock:
                    rule = _identifierRule;
                    return true;
                case TokenKind.PropertyBlock:
                    rule = _propertyRule;
                    return true;
                default:
                    rule = null;
                    return false;
            }
        }

        private static LibraParseException CreateUnexpectedTokenException(LibraToken token,
            string expected)
        {
            return token.Kind == TokenKind.Comma
                ? new LibraParseException(new("Unexpected comma. Commas are only valid in strings and reserved function calls.", token.Span))
                : new LibraParseException(new($"Unexpected token '{token.Text}' of kind {token.Kind} at {token.Span}. Expected {expected}.", token.Span));
        }
    }
}
