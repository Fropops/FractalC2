using System;
using System.Collections.Generic;
using System.Text;

namespace Common.CommandLine.Parsing
{
    public class CommandLineParser
    {
        public ParsedCommand Parse(string input)
        {
            var tokens = Tokenize(input);
            return ParseTokens(tokens);
        }

        public string[] Tokenize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Array.Empty<string>();

            var tokens = new List<string>();
            var currentToken = new StringBuilder();
            bool inDoubleQuotes = false;
            bool inSingleQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"' && !inSingleQuotes)
                {
                    inDoubleQuotes = !inDoubleQuotes;
                    continue; // Skip the quote character itself
                }
                else if (c == '\'' && !inDoubleQuotes)
                {
                    inSingleQuotes = !inSingleQuotes;
                    continue; // Skip the quote character itself
                }

                if (char.IsWhiteSpace(c))
                {
                    if (inDoubleQuotes || inSingleQuotes)
                    {
                        currentToken.Append(c);
                    }
                    else if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                }
                else
                {
                    currentToken.Append(c);
                }
            }

            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
            }

            return tokens.ToArray();
        }

        private ParsedCommand ParseTokens(string[] tokens)
        {
            if (tokens.Length == 0)
                return null;

            var result = new ParsedCommand
            {
                Name = tokens[0]
            };

            for (int i = 1; i < tokens.Length; i++)
            {
                var token = tokens[i];

                if (token.StartsWith("-"))
                {
                    string optionName = token.TrimStart('-');
                    string optionValue = "true"; // Default for flags

                    // Check if next token is a value (not an option)
                    if (i + 1 < tokens.Length && !tokens[i + 1].StartsWith("-"))
                    {
                        optionValue = tokens[i + 1];
                        i++; // Consume value token
                    }

                    result.Options[optionName] = optionValue;
                }
                else
                {
                    result.Arguments.Add(token);
                }
            }

            return result;
        }
    }
}
