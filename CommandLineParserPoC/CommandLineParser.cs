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
        private enum ArgumentType
        {
            BinarySwitch,
            ValueSwitch,
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
            public ArgumentType ArgumentType
            {
                get
                {
                    // If we have neither binary nor value switches in the args[x] then it's not a switch but a value
                    if (BinarySwitches == null && ValueSwitch == null)
                    {
                        return ArgumentType.Value;
                    }

                    // If we have a value switch, then the token type is determined by the type of the switch (see below)
                    if (ValueSwitch != null)
                    {
                        return MapSwitchTypeToTokenType(ValueSwitch);
                    }

                    // This is a binary switch then, so it takes no value
                    return ArgumentType.BinarySwitch;
                }
            }

            private static ArgumentType MapSwitchTypeToTokenType(SwitchDescription sd)
            {
                return new Dictionary<SwitchType, ArgumentType>
                {
                    {SwitchType.Binary, ArgumentType.BinarySwitch},
                    {SwitchType.Value, ArgumentType.ValueSwitch},
                    {SwitchType.List, ArgumentType.ListSwitch},
                }[sd.Type];
            }
        }

        private readonly TextParser<Argument> _tokenizer;
        private readonly SwitchDescription[] _switches;

        TextParser<SwitchDescription> GetShortSwitchParser(SwitchDescription[] switches, bool isBinary)
        {
            // Get all the switches short names
            char[] names = switches.Where(x => x.Type == SwitchType.Binary == isBinary && x.ShortName != null).Select(x => x.ShortName.Value).ToArray();
            // Construct a parser that parses such a name and returns the corresponding SwitchDescription
            return Character.In(names).Select(x => switches.First(s => s.ShortName == x));
        }

        TextParser<SwitchDescription> GetLongSwitchParser(SwitchDescription[] switches, bool isBinary)
        {
            // Get all the switches long names
            IEnumerable<string> names = switches.Where(x => x.Type == SwitchType.Binary == isBinary && x.LongName != null).Select(x => x.LongName);
            // Construct a parser that parses such a name and returns the corresponding SwitchDescription
            return CombinatorsExtention.OneOf(true, names.Select(Span.EqualTo).ToArray()).Select(x => switches.First(s => s.LongName == x.ToStringValue()));
        }
        
        private CommandLineParser(SwitchDescription[] switches)
        {
            _switches = switches;
            _tokenizer = BuildTokenizer();
        }

        private TextParser<Argument> BuildTokenizer()
        {
            // Parses a single args[x] and returns SwitchDescription for it if matches. First let's deal with short names: "-abcd"
            // There can be many of those in a single swich, any number of binary ones and zero or one value one.
            TextParser<SwitchDescription> binarySwitchShort = GetShortSwitchParser(_switches, isBinary: true);
            TextParser<SwitchDescription> valueSwitchShort = GetShortSwitchParser(_switches, isBinary: false);

            // Zero or one value switch is allowed
            TextParser<Argument> intermediate =
                from first in binarySwitchShort.Many()
                from second in valueSwitchShort.OptionalOrDefault()
                from third in binarySwitchShort.Many()
                select new Argument {BinarySwitches = first.Concat(third), ValueSwitch = second};

            // Adding switch prefix
            TextParser<Argument> withValueShort = Character.In('/', '-').Then(x => intermediate);
            TextParser<Argument> withoutValueShort = Character.In('/', '-')
                .Then(x => binarySwitchShort.Many().Select(u => new Argument {BinarySwitches = u}));

            // Now let's deal with the long names "--all-that" for long ones obvious only a single switch can be present in a single args[x]
            TextParser<SwitchDescription> binarySwitchLong = GetLongSwitchParser(_switches, isBinary: true);
            TextParser<SwitchDescription> valueSwitchLong = GetLongSwitchParser(_switches, isBinary: false);

            // Adding switch prefix and converting from SwitchDescription to Argument
            // Later tokenizer needs Argument to determine Argument Type
            TextParser<Argument> withValueLong = Span.EqualTo("--").Then(x => valueSwitchLong)
                .Select(u => new Argument {ValueSwitch = u});
            TextParser<Argument> withoutValueLong = Span.EqualTo("--").Then(x => binarySwitchLong)
                .Select(u => new Argument {BinarySwitches = new[] {u}});

            // Finally combine long and short parsers.
            TextParser<Argument> withoutValue = withoutValueLong.Try().Or(withoutValueShort).AtEnd();
            TextParser<Argument> withValue = withValueLong.Try().Or(withValueShort).AtEnd();
            // Three possible options: argument that does not require a value, argument that requires one ore more values, or a value
            // If we did not match an argument whatever is left must be value
            TextParser<Argument> any = Character.AnyChar.Many().Value(new Argument()).AtEnd();
            // Try our three options
            return withoutValue.Try().Or(withValue).Try().Or(any);
        }

        // Okay, now all ground work is done, let convert all our args to the Token List
        private TokenList<Argument> Tokenize(string[] args)
        {
            try
            {
                // Run the tokenizer now. The output of the tokenizer for each args[x] is and Argument instance which we use to construct the token
                return new TokenList<Argument>(args.Select(a => new Token<Argument>(_tokenizer.Parse(a), new TextSpan(a))).ToArray());
            }
            catch (ParseException ex)
            {
                throw new CommandLineParserException(ex.Message, ex);
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

        private void Parse(TokenList<Argument> tokens)
        {
            // Three possible options: argument that does not require a value, argument that requires a single value or argument requiring a list of values
            // Note that SetSwitchValue is called to call the parsed value setter given to us from the outside world
            TokenListParser<Argument, Unit> binary =
                from sw in TokenExtension.Matching<Argument>(x => x.ArgumentType == ArgumentType.BinarySwitch, "binary switch")
                select SetSwitchValue(sw.Kind, null);

            TokenListParser<Argument, Unit> single =
                from sw in TokenExtension.Matching<Argument>(x => x.ArgumentType == ArgumentType.ValueSwitch, "value switch")
                from val in TokenExtension.Matching<Argument>(x => x.ArgumentType == ArgumentType.Value, "value")
                select SetSwitchValue(sw.Kind, val.ToStringValue());

            TokenListParser<Argument, Unit> list =
                from sw in TokenExtension.Matching<Argument>(x => x.ArgumentType == ArgumentType.ListSwitch, "list switch")
                from val in TokenExtension.Matching<Argument>(x => x.ArgumentType == ArgumentType.Value, "value").AtLeastOnce()
                select SetSwitchValue(sw.Kind, string.Join(" ", val.Select(v => v.ToStringValue())));

            // Let's try all the three one after another
            TokenListParser<Argument, Unit[]> parser = binary.Try().Or(single).Try().Or(list).Many().AtEnd();

            try
            {
                // Now we run our constructed parser
                parser.Parse(tokens);
            }
            catch (ParseException ex)
            {
                throw new CommandLineParserException(ex.Message, ex);
            }
        }

        private void Parse(string[] args)
        {
            Parse(Tokenize(args));
        }

        public static void Parse(string[] args, SwitchDescription[] switches)
        {
            (new CommandLineParser(switches)).Parse(args);
        }
    }
}
