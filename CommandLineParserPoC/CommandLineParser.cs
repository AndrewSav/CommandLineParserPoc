using System;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Collections.Generic;
using System.Linq;

namespace CommandLineParserPoC
{
    public class CommandLineParser
    {
        // This classifies each args[x] to either be a binary switch (requiring no value)
        // or be a switch that requires a single value that comes as the next args[x+1]
        // or a switch that can take multiple values args[x+1] ... args[x+n]
        // otherwise it's not a switch but a value belonging to a prior switch
        private enum ArgumnetType
        {
            BinarySwitch,
            ValueSwitch, //Int, String, Quouted List
            ListSwitch,
            Value
        }

        // This class describes a single argument from the passed on command line argument array string args[]
        private class Argument
        {
            // May have multiple binary switches
            public IEnumerable<SwitchDescription> BinarySwitches { get; set; }
            // But no more than one value switch
            public SwitchDescription ValueSwitch { get; set; }
            // Calculates what ArgumentType this Argument is of
            public ArgumnetType ArgumnetType
            {
                get
                {
                    // If we have neither binary nor value switches in the args[x] then it's not a switch but a value
                    if (BinarySwitches == null && ValueSwitch == null)
                    {
                        return ArgumnetType.Value;
                    }

                    // If we have a value switch, then the token type is determined by the type of the switch (see below)
                    if (ValueSwitch != null)
                    {
                        return MapSwitchTypeToTokenType(ValueSwitch);
                    }

                    // This is a binary switch then, so it takes no value
                    return ArgumnetType.BinarySwitch;
                }
            }
            private static ArgumnetType MapSwitchTypeToTokenType(SwitchDescription sd)
            {
                return new Dictionary<SwitchType, ArgumnetType>
                {
                    { SwitchType.Binary, ArgumnetType.BinarySwitch},
                    { SwitchType.Value, ArgumnetType.ValueSwitch},
                    { SwitchType.List, ArgumnetType.ListSwitch},
                }[sd.Type];
            }
        }

        private readonly TextParser<Argument> _withoutValue; // matches purely binary switches
        private readonly TextParser<Argument> _withValue; // matches switches that include a value switch

        // This is to bootstrap the "Or" aggregation for span parsers for all given long switches
        // E.g "--all-that", "--bugger-all", etc. I'd rather not have that and get Superpower to provide
        // a build in method instead. 
        // E.g public static TextParser<TextSpan> EqualToAny(string[] text)
        private readonly TextParser<TextSpan> _nullParser = input => Result.Empty<TextSpan>(TextSpan.None);

        // This not a very nice method, my apologies. Please let me know if you have a good idea how to improve it.
        // Partially it came to because of the problem mentioned just above, Superpower not providing an easy way to match from a set of strings.
        // For each given "switches" parameter, it can return 4 different paresers: for short and long names (e.g. "-a" or "--all-that")
        // for binary and value options each. Which of the four is returned depends on "isBinary" and "isShort" parameters
        private TextParser<SwitchDescription> GetSwitchParser(SwitchDescription[] switches, bool isBinary, bool isShort)
        {
            return isShort ? GetSwitchParserInner(s => s.ShortName) : GetSwitchParserInner(s => s.LongName);
            TextParser<SwitchDescription> GetSwitchParserInner<T>(Func<SwitchDescription, T> nameSelector)
            {
                // This gives us the list of short or long names
                var u = switches.Where(s => s.Type == SwitchType.Binary == isBinary && nameSelector(s) != null).Select(nameSelector);
                switch (u)
                {
                    case IEnumerable<char?> z:
                        // ReSharper disable once PossibleInvalidOperationException
                        // for short names
                        return Character.In(z.Select(x => x.Value).ToArray()).Select(x => switches.First(s => nameSelector(s) as char? == x));
                    case IEnumerable<string> z:
                        // for long names
                        return z.Select(Span.EqualTo).Aggregate(_nullParser, (a, b) => a.Try().Or(b)).Select(x => switches.First(s => nameSelector(s) as string == x.ToStringValue()));
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public CommandLineParser(SwitchDescription[] switches)
        {
            // Parses a single args[x] and returns SwitchDescription for it if matches. First let's deal with short names: "-abcd"
            // There can be many of those in a single swich, any number of binary ones and zero or one value one.
            TextParser<SwitchDescription> binarySwitchShort = GetSwitchParser(switches, isBinary: true, isShort: true);
            TextParser<SwitchDescription> valueSwitchShort = GetSwitchParser(switches, isBinary: false, isShort: true);

            // Zero or one value switch is allowed
            TextParser<Argument> intermediate =
                from first in binarySwitchShort.Many()
                from second in valueSwitchShort.OptionalOrDefault()
                from third in binarySwitchShort.Many()
                select new Argument { BinarySwitches = first.Concat(third), ValueSwitch = second };

            // Adding switch prefix
            TextParser<Argument> withValueShort = Character.In('/', '-').Then(x => intermediate);
            TextParser<Argument> withoutValueShort = Character.In('/', '-').Then(x => binarySwitchShort.Many().Select(u => new Argument { BinarySwitches = u }));

            // Now let's deal with the long names "--all-that" for long ones obvious only a single switch can be present in a single args[x]
            TextParser<SwitchDescription> binarySwitchLong = GetSwitchParser(switches, isBinary: true, isShort: false);
            TextParser<SwitchDescription> valueSwitchLong = GetSwitchParser(switches, isBinary: false, isShort: false);

            // Adding switch prefix and converting from SwitchDescription to Argument
            // Later tokenizer needs Argument to determine Argument Type
            TextParser<Argument> withValueLong = Span.EqualTo("--").Then(x => valueSwitchLong).Select(u => new Argument { ValueSwitch = u });
            TextParser<Argument> withoutValueLong = Span.EqualTo("--").Then(x => binarySwitchLong).Select(u => new Argument { BinarySwitches = new [] {u} });

            // Finally combine long and short parsers. Note that the resulting two parsers will be used in two places
            // in tokenzier - so that we get Argument Types and then in parser so we actualy parse these
            // It feels inefficient, I'd rather pass already parsed args[x]'s to parser from tokenizer
            // But unfortunately each Token is always TextSpan based. I wonder if Superpower can change to make Token generic
            // so that it could use any type, not just TextSpan.
            _withoutValue = withoutValueLong.Try().Or(withoutValueShort).AtEnd();
            _withValue = withValueLong.Try().Or(withValueShort).AtEnd();

        }

        // Okay, now all ground work is done, let convert all our args to the Token List
        private TokenList<ArgumnetType> Tokenize(string[] args)
        {
            try
            {
                // Three possible options: argument that does not require a value, argument that requires one ore more values, or a value
                // If we did not match an argument whatever is left must be value
                TextParser<Argument> any = Character.AnyChar.Many().Value(new Argument()).AtEnd();
                // Try our three options
                TextParser<Argument> tokenizer = _withoutValue.Try().Or(_withValue).Try().Or(any);
                // Run the tokenizer now. The output of the tokenizer for each args[x] is and Argument instance which we use to construct the token
                // Pitty we have to use TextSpan and will have to re-parse that again in the parser
                return new TokenList<ArgumnetType>(args
                    .Select(a => new Token<ArgumnetType>(tokenizer.Parse(a).ArgumnetType, new TextSpan(a))).ToArray());
            } catch (ParseException ex)
            {
                throw new CommandLineParserException(ex.Message,ex);
            }
        }

        // This is called by the parser when its know an argument and all its values
        private Unit SetSwitchValue(Argument sw, string val)
        {
            sw.ValueSwitch?.SetValue(val);
            if (sw.BinarySwitches != null)
            {
                foreach (var bin in sw.BinarySwitches)
                {
                    bin.SetValue(null);
                }
            }
            return Unit.Value;
        }
        private void Parse(TokenList<ArgumnetType> tokens)
        {
            // Three possible options: argument that does not require a value, argument that requires a single value or argument requiring a list of values
            // Note how .Apply re-parser the argument that we already parsed in the tokenized
            // Also note that SetSwitchValue is called to call the parsed value setter given to us from the outside world
            TokenListParser<ArgumnetType, Unit> binary =
                from sw in Token.EqualTo(ArgumnetType.BinarySwitch).Apply(x => _withoutValue)
                select SetSwitchValue(sw, null);

            TokenListParser<ArgumnetType, Unit> single =
                from sw in Token.EqualTo(ArgumnetType.ValueSwitch).Apply(x => _withValue)
                from val in Token.EqualTo(ArgumnetType.Value)
                select SetSwitchValue(sw, val.ToStringValue());

            TokenListParser<ArgumnetType, Unit> list =
                from sw in Token.EqualTo(ArgumnetType.ListSwitch).Apply(x => _withValue)
                from val in Token.EqualTo(ArgumnetType.Value).AtLeastOnce()
                select SetSwitchValue(sw, string.Join(" ", val.Select(v => v.ToStringValue())));

            // Let's try all the three one after another
            TokenListParser<ArgumnetType, Unit[]> parser = binary.Try().Or(single).Try().Or(list).Many().AtEnd();

            try
            {
                // Now we run our constructed parser
                parser.Parse(tokens);
            } catch (ParseException ex)
            {
                throw new CommandLineParserException(ex.Message,ex);
            }
        }

        public void Parse(string[] args)
        {
            Parse(Tokenize(args));
        }

        public static void Parse(string[] args, SwitchDescription[] switches)
        {
            (new CommandLineParser(switches)).Parse(args);
        }
    }
}
