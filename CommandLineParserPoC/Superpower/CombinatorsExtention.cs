using System;
using Superpower;
using Superpower.Model;

namespace CommandLineParserPoC.Superpower
{
    internal static class CombinatorsExtention
    {
        public static TextParser<T> OneOf<T>(bool backtrack = false, params TextParser<T>[] parsers)
        {
            if (parsers == null) throw new ArgumentNullException(nameof(parsers));

            if (parsers.Length == 0)
            {
                return i => Result.Empty<T>(TextSpan.None);
            }

            TextParser<T> c = parsers[0];
            for (int i = 1; i < parsers.Length; i++)
            {
                if (backtrack)
                {
                    c = c.Try();
                }
                c = c.Or(parsers[i]);
            }

            return c;
        }
    }
}
