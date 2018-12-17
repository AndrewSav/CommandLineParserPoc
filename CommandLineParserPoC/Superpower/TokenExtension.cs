using System;
using Superpower;
using Superpower.Model;

namespace CommandLineParserPoC.Superpower
{
    internal class TokenExtension
    {
        public static TokenListParser<TKind, Token<TKind>> Matching<TKind>(Func<TKind, bool> predicate, string name)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (name == null) throw new ArgumentNullException(nameof(name));

            return Matching(predicate, new[] { name });
        }

        private static TokenListParser<TKind, Token<TKind>> Matching<TKind>(Func<TKind, bool> predicate, string[] expectations)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (expectations == null) throw new ArgumentNullException(nameof(expectations));

            return input =>
            {
                var next = input.ConsumeToken();
                if (!next.HasValue || !predicate(next.Value.Kind))
                    return TokenListParserResult.Empty<TKind, Token<TKind>>(input , expectations);

                return next;
            };
        }
    }
}
