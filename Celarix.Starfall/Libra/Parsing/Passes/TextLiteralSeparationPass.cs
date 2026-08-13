using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Passes
{
    internal sealed class TextLiteralSeparationPass : IParsePass
    {
        internal enum State
        {
            Start,
            Token,
            TextLiteral
        }

        private State _state = State.Start;
        private readonly List<LibraToken> _parsedTokens = new();
        private readonly StringBuilder _tokenBuilder = new();

        public bool TryPass(IReadOnlyList<LibraToken> tokens,
            [NotNullWhen(true)] out IReadOnlyList<LibraToken>? parsedTokens,
            [NotNullWhen(false)] out LibraDiagnostic? diagnostic)
        {
            var inputValid = InputValid(tokens);
            if (inputValid != null)
            {
                parsedTokens = null;
                diagnostic = inputValid;
                return false;
            }

            var flatToken = (FlatToken)tokens[0];
            var text = flatToken.Text;
            char? last = null;

            foreach (char c in text)
            {
                switch (_state)
                {
                    case State.Start:
                    case State.Token:
                        if (c == '"')
                        {
                            _state = State.TextLiteral;
                            AddTokenIfNotEmpty();
                            _tokenBuilder.Append(c);
                        }
                        else
                        {
                            _tokenBuilder.Append(c);
                        }
                        break;
                    case State.TextLiteral:
                        _tokenBuilder.Append(c);
                        if (c == '"')
                        {
                            if (last != '\\')
                            {
                                _tokenBuilder.Append(c);
                                _parsedTokens.Add(new FlatToken(TokenKind.TextLiteral, ReplaceEscapeSequences(_tokenBuilder.ToString())));
                                _tokenBuilder.Clear();
                                _state = State.Token;
                            }
                        }
                        break;
                }
                last = c;
            }

            // Verify end state.
            if (_state == State.Start)
            {
                parsedTokens = null;
                diagnostic = new LibraDiagnostic("Internal parser error: ended in the Start state, expected tokens.", null);
                return false;
            }
            else if (_state == State.TextLiteral)
            {
                parsedTokens = null;
                diagnostic = new LibraDiagnostic("Unterminated text literal.", new(flatToken.Text, 0, flatToken.Text.Length));
                return false;
            }
            else
            {
                AddTokenIfNotEmpty();
                parsedTokens = _parsedTokens;
                diagnostic = null;
                return true;
            }
        }

        private LibraDiagnostic? InputValid(IReadOnlyList<LibraToken> tokens)
        {
            if (tokens.Count != 1)
            {
                return new LibraDiagnostic("Invalid empty input.", null);
            }
            else if (tokens[0] is not FlatToken flatToken)
            {
                return new LibraDiagnostic($"Internal parser error: received an unexpected token type {tokens[0].GetType().Name}.", null);
            }
            else if (string.IsNullOrWhiteSpace(flatToken.Text))
            {
                return new LibraDiagnostic("Invalid empty input.", new(flatToken.Text, 0, flatToken.Text.Length));
            }
            else if (flatToken.Kind != TokenKind.Unresolved)
            {
                return new LibraDiagnostic($"Internal parser error: expected an unresolved token, but received a {flatToken.Kind}.",
                    new(flatToken.Text, 0, flatToken.Text.Length));
            }

            return null;
        }

        private void AddTokenIfNotEmpty()
        {
            if (_tokenBuilder.Length > 0)
            {
                _parsedTokens.Add(new FlatToken(TokenKind.Unresolved, _tokenBuilder.ToString()));
                _tokenBuilder.Clear();
            }
        }

        private string ReplaceEscapeSequences(string text)
        {
            return text.Replace("\\\"", "\"")
                       .Replace("\\\\", "\\")
                       .Replace("\\n", "\n")
                       .Replace("\\r", "\r")
                       .Replace("\\t", "\t");
        }
    }
}
