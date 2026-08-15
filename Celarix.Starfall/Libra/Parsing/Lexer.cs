using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal sealed class Lexer
    {
        private readonly string _source;
        private readonly List<LibraToken> _tokens = new();
        private readonly StringBuilder _tokenBuilder = new();
        private int _position;

        public Lexer(string source)
        {
            _source = source;
        }

        // New Cursor API
        private bool IsAtEnd => _position >= _source.Length;
        private char Current => _source[_position];
        private char? Peek(int offset = 1) => _position + offset < _source.Length ? _source[_position + offset] : null;
        private char Advance()
        {
            var current = Current;
            _position += 1;
            return current;
        }
        private bool Match(char expected)
        {
            if (IsAtEnd) return false;
            if (Current != expected) return false;
            _position += 1;
            return true;
        }
        private TextSpan SpanFrom(int start) => new TextSpan(_source, start, _source.Length - start);
        private TextSpan SpanFrom(int start, int length) => new TextSpan(_source, start, length);

        private LibraToken ScanToken()
        {
            if (IsAtEnd)
            {
                return new LibraToken(TokenKind.EndOfInput, "", SpanFrom(_position));
            }

            while (char.IsWhiteSpace(Current))
            {
                Advance();
            }

            if (Current == '"')
            {
                return ScanString();
            }
            else if (Current == ';')
            {
                return ScanReservedName();
            }
            else if (Current == '[')
            {
                if (Peek() == '[')
                {
                    return ScanSubstitution();
                }
                return ScanPropertyBlock();
            }
            else if (Current == '@')
            {
                return ScanIdentifierBlock();
            }
            else if (Current is '(' or '{' or ')' or '}' or ',')
            {
                return ScanSingleCharToken();
            }
            else if (OperatorRegistry.IsOperatorStart(Current.ToString()))
            {
                return ScanOperator();
            }
            else if (IsBareAtomChar(Current))
            {
                return ScanText();
            }
            else
            {
                throw new LibraParseException(new($"Unexpected character '{Current}'", SpanFrom(_position)));
            }
        }

        private LibraToken ScanString()
        {
            // Consume the first quote
            _tokenBuilder.Append(Advance());

            while (true)
            {
                if (IsAtEnd)
                {
                    throw new LibraParseException(new("Unterminated string literal", SpanFrom(_position - _tokenBuilder.Length, _tokenBuilder.Length)));
                }
                else if (Current == '\\')
                {
                    if (Peek() == '"' || Peek() == '\\')
                    {
                        _tokenBuilder.Append(Advance()); // Append the backslash
                        _tokenBuilder.Append(Advance()); // Append the escaped character
                    }
                    else
                    {
                        throw new LibraParseException(new($"Invalid escape sequence '\\{Peek()}'", SpanFrom(_position, 2)));
                    }
                }
                else if (Current == '"')
                {
                    _tokenBuilder.Append(Advance());
                    break;
                }
                else
                {
                    _tokenBuilder.Append(Advance());
                }
            }

            var tokenText = _tokenBuilder.ToString();
            _tokenBuilder.Clear();

            if (!tokenText.EndsWith("\""))
            {
                throw new LibraParseException(new("Unterminated string literal", SpanFrom(_position - tokenText.Length, tokenText.Length)));
            }

            return new LibraToken(TokenKind.String, tokenText, SpanFrom(_position - tokenText.Length, tokenText.Length));
        }

        private LibraToken ScanReservedName()
        {
            var start = _position;
            _tokenBuilder.Append(Advance()); // Consume the ';'
            while (!IsAtEnd && (char.IsLetterOrDigit(Current) || Current == '_'))
            {
                _tokenBuilder.Append(Advance());
            }
            var tokenText = _tokenBuilder.ToString();
            _tokenBuilder.Clear();

            if (tokenText == ";")
            {
                throw new LibraParseException(new("Reserved name cannot be empty", SpanFrom(start, 1)));
            }

            return new LibraToken(TokenKind.ReservedName, tokenText, SpanFrom(start, tokenText.Length));
        }

        private LibraToken ScanSubstitution()
        {
            var start = _position;
            // Consume the two opening brackets
            _tokenBuilder.Append(Advance());
            _tokenBuilder.Append(Advance());
            while (!IsAtEnd)
            {
                if (Current == ']' && Peek() == ']')
                {
                    _tokenBuilder.Append(Advance()); // Append the first closing bracket
                    _tokenBuilder.Append(Advance()); // Append the second closing bracket
                    break;
                }
                else if (char.IsLetterOrDigit(Current) || Current == '_')
                {
                    _tokenBuilder.Append(Advance());
                }
                else
                {
                    throw new LibraParseException(new($"Invalid character '{Current}' in substitution block", SpanFrom(_position)));
                }
            }
            var tokenText = _tokenBuilder.ToString();
            _tokenBuilder.Clear();

            if (!tokenText.EndsWith("]]"))
            {
                throw new LibraParseException(new("Unterminated substitution block", SpanFrom(start, tokenText.Length)));
            }

            return new LibraToken(TokenKind.Substitution, tokenText, SpanFrom(start, tokenText.Length));
        }

        private LibraToken ScanPropertyBlock()
        {
            var start = _position;
            // Consume the opening bracket
            _tokenBuilder.Append(Advance());

            while (!IsAtEnd)
            {
                if (char.IsLetterOrDigit(Current)
                    || Current == '_'
                    || Current == '='
                    || Current == '.'
                    || Current == ',')
                {
                    _tokenBuilder.Append(Advance());
                }
                else if (Current == ']')
                {
                    _tokenBuilder.Append(Advance());
                    break;
                }
                else
                {
                    throw new LibraParseException(new($"Invalid character '{Current}' in property block", SpanFrom(_position)));
                }
            }
            var tokenText = _tokenBuilder.ToString();
            _tokenBuilder.Clear();

            if (!tokenText.EndsWith("]"))
            {
                throw new LibraParseException(new("Unterminated property block", SpanFrom(start, tokenText.Length)));
            }

            return new LibraToken(TokenKind.PropertyBlock, tokenText, SpanFrom(start, tokenText.Length));
        }

        private LibraToken ScanIdentifierBlock()
        {
            var start = _position;
            // Consume the '@'
            _tokenBuilder.Append(Advance());

            while (!IsAtEnd)
            {
                if (char.IsLetterOrDigit(Current) || Current == '_' || Current == '.' || Current == '#')
                {
                    _tokenBuilder.Append(Advance());
                }
                else
                {
                    break;
                }
            }
            var tokenText = _tokenBuilder.ToString();
            _tokenBuilder.Clear();
            return new LibraToken(TokenKind.IdentifierBlock, tokenText, SpanFrom(start, tokenText.Length));
        }

        private LibraToken ScanSingleCharToken()
        {
            var start = _position;
            var c = Advance();
            var kind = c switch
            {
                '(' => TokenKind.OpenParen,
                '{' => TokenKind.OpenBrace,
                ')' => TokenKind.CloseParen,
                '}' => TokenKind.CloseBrace,
                ',' => TokenKind.Comma,
                _ => throw new InvalidOperationException($"Unexpected character '{c}'")
            };
            return new LibraToken(kind, c.ToString(), SpanFrom(start, 1));
        }

        private LibraToken ScanOperator()
        {
            var start = _position;

            if (OperatorRegistry.TryMatchLongestOperator(_source, start, out var symbol))
            {
                _position += symbol.Length;
                return new LibraToken(TokenKind.Operator, symbol, SpanFrom(start, symbol.Length));

            }
            else
            {
                throw new LibraParseException(new($"Unknown operator starting with '{Current}'", SpanFrom(start)));
            }
        }

        private LibraToken ScanText()
        {
            var start = _position;
            while (!IsAtEnd && IsBareAtomChar(Current))
            {
                _tokenBuilder.Append(Advance());
            }
            var tokenText = _tokenBuilder.ToString();
            _tokenBuilder.Clear();
            return new LibraToken(TokenKind.Text, tokenText, SpanFrom(start, tokenText.Length));
        }

        private static bool IsBareAtomChar(char c)
        {
            // Matches the regex [A-Za-z0-9.]
            return char.IsLetterOrDigit(c) || c == '.';
        }

        public IReadOnlyList<LibraToken> Parse()
        {
            while (true)
            {
                var token = ScanToken();
                _tokens.Add(token);
                if (token.Kind == TokenKind.EndOfInput)
                {
                    break;
                }
            }

            return _tokens;
        }
    }
}