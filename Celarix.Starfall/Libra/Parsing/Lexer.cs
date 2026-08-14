using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal sealed class Lexer
    {
        private enum State
        {
            Start,
            String,
            Reserved,
            Substitution,
            PropertyBlock,
            Operator,
            Text,
            IdentifierBlock,
            End
        }

        private readonly string _source;
        private readonly List<LibraToken> _tokens = new();
        private readonly StringBuilder _tokenBuilder = new();
        private int _position;
        private char? _last;
        private State _state = State.Start;

        public Lexer(string source)
        {
            _source = source;
        }

        public IReadOnlyList<LibraToken> Parse()
        {
            while (_state != State.End)
            {
                switch (_state)
                {
                    case State.Start: Start(); break;
                    case State.String: String(); break;
                    case State.Reserved: Reserved(); break;
                    case State.Substitution: Substitution(); break;
                    case State.PropertyBlock: PropertyBlock(); break;
                    case State.Operator: Operator(); break;
                    case State.Text: Text(); break;
                    case State.IdentifierBlock: IdentifierBlock(); break;
                }
            }

            _tokens.Add(new(TokenKind.EndOfInput, "", new(_source, _position, 0)));
            return _tokens;
        }

        private char? PeekNext()
        {
            if (_position + 1 < _source.Length)
            {
                return _source[_position + 1];
            }
            return null;
        }

        private char? PeekAhead(int offset)
        {
            if (_position + offset < _source.Length)
            {
                return _source[_position + offset];
            }
            return null;
        }

        private char? MoveNext()
        {
            if (_position < _source.Length)
            {
                _last = _source[_position];
                _position += 1;
                return _last;
            }
            return null;
        }

        private char? MoveNextAndAppend()
        {
            var next = MoveNext();
            if (next != null)
            {
                _tokenBuilder.Append(next);
            }
            return next;
        }

        private void AddTokenIfNotEmpty(TokenKind kind)
        {
            if (_tokenBuilder.Length > 0)
            {
                var tokenText = _tokenBuilder.ToString();
                var tokenSpan = new TextSpan(tokenText, _position - tokenText.Length, tokenText.Length);
                var token = new LibraToken(kind, tokenText, tokenSpan);
                _tokens.Add(token);
                _tokenBuilder.Clear();
            }
        }

        private void ConsumeUntilDelimiter(TokenKind tokenKind)
        {
            while (true)
            {
                var next = MoveNext();
                if (next == null)
                {
                    AddTokenIfNotEmpty(tokenKind);
                    _state = State.End;
                    return;
                }

                if (TryHandleDelimiter(next.Value, tokenKind))
                {
                    return;
                }

                _tokenBuilder.Append(next.Value);
            }
        }

        private bool TryHandleDelimiter(char c, TokenKind currentTokenKind)
        {
            switch (c)
            {
                case '"':
                    AddTokenIfNotEmpty(currentTokenKind);
                    MoveNextAndAppend();
                    _state = State.String;
                    return true;
                case ';':
                    AddTokenIfNotEmpty(currentTokenKind);
                    MoveNextAndAppend();
                    _state = State.Reserved;
                    return true;
                case '@':
                    AddTokenIfNotEmpty(currentTokenKind);
                    throw CreateException("Cannot start an identifier block in the middle of a token stream.");
                case '[':
                    var nextPeek = PeekNext();
                    if (nextPeek == '[')
                    {
                        AddTokenIfNotEmpty(currentTokenKind);
                        MoveNextAndAppend();
                        _state = State.Substitution;
                        return true;
                    }
                    else
                    {
                        AddTokenIfNotEmpty(currentTokenKind);
                        throw CreateException("Cannot start a property block in the middle of a token stream.");
                    }
                case '(':
                case '{':
                case ')':
                case '}':
                case ',':
                    AddTokenIfNotEmpty(currentTokenKind);
                    _tokens.Add(new(c == '(' ? TokenKind.OpenParen :
                                    c == '{' ? TokenKind.OpenBrace :
                                    c == ')' ? TokenKind.CloseParen :
                                    c == '}' ? TokenKind.CloseBrace :
                                    c == ',' ? TokenKind.Comma :
                                    throw new InvalidOperationException($"Unexpected character '{c}'"),
                                    c.ToString(),
                                    new TextSpan(c.ToString(), _position - 1, 1)));
                    _state = State.Text;
                    return true;
                default:
                    if (OperatorRegistry.IsOperatorStart(c.ToString()))
                    {
                        AddTokenIfNotEmpty(currentTokenKind);
                        _tokens.Add(new(TokenKind.Operator, c.ToString(), new TextSpan(c.ToString(), _position - 1, 1)));
                        _state = State.Text;
                        return true;
                    }
                    break;
            }
            return false;
        }

        private void AddSingleCharToken(TokenKind tokenKind, char c)
        {
            _tokens.Add(new(tokenKind, c.ToString(), new TextSpan(c.ToString(), _position - 1, 1)));
        }

        private LibraParseException CreateException(string message, int length = 1)
        {
            var tokenText = _tokenBuilder.ToString();
            var tokenSpan = new TextSpan(tokenText, _position - tokenText.Length, tokenText.Length);
            return new LibraParseException(new(message, tokenSpan));
        }

        private void Start()
        {
            var peek = PeekNext();
            if (peek == null)
            {
                _state = State.End;
                return;
            }

            var peekValue = peek.Value;
            if (peekValue == '"')
            {
                MoveNextAndAppend();
                _state = State.String;
                return;
            }
            else if (peekValue == ';')
            {
                MoveNextAndAppend();
                _state = State.Reserved;
            }
            else if (peekValue == '@')
            {
                throw CreateException("Cannot start an identifier block at the beginning of a token stream.");
            }
            else if (peekValue == '[')
            {
                var nextPeek = PeekAhead(2);
                if (nextPeek == '[')
                {
                    MoveNextAndAppend();
                    _state = State.Substitution;
                }
                else
                {
                    throw CreateException("Cannot start a property block at the beginning of a token stream.");
                }
            }
            else if (peekValue == '(')
            {
                MoveNext();
                AddSingleCharToken(TokenKind.OpenParen, '(');
                _state = State.Text;
            }
            else if (peekValue == '{')
            {
                MoveNext();
                AddSingleCharToken(TokenKind.OpenBrace, '{');
                _state = State.Text;
            }
            else if (peekValue is ')' or '}')
            {
                throw CreateException($"Cannot start a token stream with '{peekValue}'.");
            }
            else if (OperatorRegistry.IsOperatorStart(peekValue.ToString()))
            {
                MoveNextAndAppend();
                _state = State.Operator;
            }
            else
            {
                MoveNextAndAppend();
                _state = State.Text;
            }
        }

        private void String()
        {
            while (true)
            {
                var next = MoveNext() ?? throw CreateException("Unterminated string literal.", _tokenBuilder.Length);

                if (next == '\\' && PeekNext() is '"' or '\\')
                {
                    _tokenBuilder.Append(next);
                    _tokenBuilder.Append(MoveNext()!.Value); // Consume escaped char
                }
                else if (next == '"')
                {
                    AddTokenIfNotEmpty(TokenKind.String);
                    _state = State.Text;
                    return;
                }
                else
                {
                    _tokenBuilder.Append(next);
                }
            }
        }

        private void Reserved()
        {
            // Takes up [A-Za-z0-9_] characters until a non-matching character is found.
            while (true)
            {
                var next = MoveNext();
                if (next == null)
                {
                    AddTokenIfNotEmpty(TokenKind.ReservedName);
                    _state = State.End;
                    return;
                }
                else if (char.IsLetterOrDigit(next.Value) || next.Value == '_')
                {
                    _tokenBuilder.Append(next.Value);
                }
                else
                {
                    AddTokenIfNotEmpty(TokenKind.ReservedName);
                    _state = State.Text;
                    return;
                }
            }
        }

        private void Substitution()
        {
            // Takes up [A-Za-z0-9_] characters until a "]]" is found, which ends the substitution block.
            while (true)
            {
                var next = MoveNext();
                if (next == null)
                {
                    throw CreateException("Unterminated substitution block.", _tokenBuilder.Length);
                }
                else if (char.IsLetterOrDigit(next.Value) || next.Value == '_')
                {
                    _tokenBuilder.Append(next.Value);
                }
                else if (next.Value == ']' && PeekNext() == ']')
                {
                    MoveNextAndAppend(); // Append the second ']'
                    AddTokenIfNotEmpty(TokenKind.Substitution);
                    _state = State.Text;
                    return;
                }
                else
                {
                    throw CreateException($"Invalid character '{next}' in substitution block.", 1);
                }
            }
        }

        private void PropertyBlock()
        {
            // Takes up any characters until a "]" is found, which ends the property block.
            while (true)
            {
                var next = MoveNext();
                if (next == null)
                {
                    throw CreateException("Unterminated property block.", _tokenBuilder.Length);
                }
                else if (next.Value == ']')
                {
                    AddTokenIfNotEmpty(TokenKind.PropertyBlock);
                    _state = State.Text;
                    return;
                }
                else
                {
                    _tokenBuilder.Append(next.Value);
                }
            }
        }

        private void Operator()
        {
            // Takes up any characters that are part of an operator until a non-operator character is found.
            while (true)
            {
                var next = MoveNext();
                if (next == null)
                {
                    AddTokenIfNotEmpty(TokenKind.Operator);
                    _state = State.End;
                    return;
                }
                else if (OperatorRegistry.IsOperatorStart(_tokenBuilder.ToString() + next.Value))
                {
                    _tokenBuilder.Append(next.Value);
                }
                else
                {
                    AddTokenIfNotEmpty(TokenKind.Operator);
                    _state = State.Text;
                    return;
                }
            }
        }

        private void Text() => ConsumeUntilDelimiter(TokenKind.Text);
        private void IdentifierBlock() => ConsumeUntilDelimiter(TokenKind.IdentifierBlock);
    }
}