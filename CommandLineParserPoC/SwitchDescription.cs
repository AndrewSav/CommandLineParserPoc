using System;

namespace CommandLineParserPoC
{
    public class SwitchDescription
    {
        // E.g 'a' for "-a"
        public char? ShortName { get; set; }
        // E.g "all-that" for "--all-that"
        public string LongName { get; set; }
        public SwitchType Type { get; set; }
        // Not used in the PoC in a longer run command line Help can be generated from this
        public string Description { get; set; }
        // When the command line is parsed and the value of this switch is determined
        // this is what the command line parser calls to set the now known value of this switch
        public Action<string> SetValue { get; set; }
    }
}