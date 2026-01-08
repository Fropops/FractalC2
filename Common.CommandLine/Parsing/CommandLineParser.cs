using System;
using System.Collections.Generic;
using System.Text;
using SharpCommandLine.Parsing;

namespace Common.CommandLine.Parsing
{
    public class CommandLineParser
    {
        public ParsedCommand Parse(string input)
        {
            var tokens = Tokenize(input);
            return ParseTokens(tokens);
        }

        public List<CommandLineToken> Tokenize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<CommandLineToken>();

            var tokens = new List<CommandLineToken>();
            var currentToken = new StringBuilder();
            int tokenStart = -1;
            bool inDoubleQuotes = false;
            bool inSingleQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (tokenStart == -1 && !char.IsWhiteSpace(c))
                {
                    tokenStart = i;
                }

                if (c == '"' && !inSingleQuotes)
                {
                    inDoubleQuotes = !inDoubleQuotes;
                    continue; // Skip the quote character itself but keep it as part of token logic? 
                    // Actually, if we skip it, the Length won't match the original string substring.
                    // But here we want logical values.
                    // For Remainder, we rely on StartIndex/EndIndex from original string.
                    // However, we need accurate StartIndex for the token.
                }
                else if (c == '\'' && !inDoubleQuotes)
                {
                    inSingleQuotes = !inSingleQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    if (inDoubleQuotes || inSingleQuotes)
                    {
                        currentToken.Append(c);
                    }
                    else if (currentToken.Length > 0)
                    {
                        // End of token
                        // Calculate length based on current position and start
                        // Note: i is the whitespace index.
                        int length = i - tokenStart;
                        tokens.Add(new CommandLineToken(currentToken.ToString(), tokenStart, length));
                        currentToken.Clear();
                        tokenStart = -1;
                    }
                }
                else
                {
                    currentToken.Append(c);
                }
            }

            if (currentToken.Length > 0)
            {
                int length = input.Length - tokenStart;
                tokens.Add(new CommandLineToken(currentToken.ToString(), tokenStart, length));
            }

            return tokens;
        }

        private ParsedCommand ParseTokens(List<CommandLineToken> tokens)
        {
            if (tokens.Count == 0)
                return null;

            var result = new ParsedCommand
            {
                Name = tokens[0].Value
            };
            result.Tokens.AddRange(tokens);

            for (int i = 1; i < tokens.Count; i++)
            {
                var token = tokens[i];

                if (token.Value.StartsWith("-"))
                {
                    string optionName = token.Value.TrimStart('-');
                    string optionValue = "true"; // Default for flags

                    // Check if next token is a value (not an option)
                    if (i + 1 < tokens.Count && !tokens[i + 1].Value.StartsWith("-"))
                    {
                        optionValue = tokens[i + 1].Value;
                        i++; // Consume value token
                    }

                    result.Options[optionName] = optionValue;
                }
                else
                {
                    result.Arguments.Add(token.Value);
                }
            }

            return result;
        }
    }
}
