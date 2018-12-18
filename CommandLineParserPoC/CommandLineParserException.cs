using System;
// ReSharper disable UnusedMember.Global

namespace CommandLineParserPoC
{
    public class CommandLineParserException : Exception
    {
        public CommandLineParserException()
        {
        }

        public CommandLineParserException(string message) : base(message)
        {
        }

        public CommandLineParserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
