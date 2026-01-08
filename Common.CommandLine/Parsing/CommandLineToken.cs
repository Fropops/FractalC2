namespace SharpCommandLine.Parsing
{
    public class CommandLineToken
    {
        public string Value { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public int EndIndex => StartIndex + Length;
        public bool IsOption => Value.StartsWith("-");

        public CommandLineToken(string value, int startIndex, int length)
        {
            Value = value;
            StartIndex = startIndex;
            Length = length;
        }
    }
}
