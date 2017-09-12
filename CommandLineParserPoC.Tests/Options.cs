using System;

namespace CommandLineParserPoC.Tests
{
    // Possible options to which command line switches get parsed to.
    public class Options
    {
        public bool Binary1 { get; set; }
        public bool Binary2 { get; set; }
        public bool Binary3 { get; set; }
        public bool Binary4 { get; set; }
        public String String1 { get; set; }
        public String String2 { get; set; }
        public String String3 { get; set; }
        public String String4 { get; set; }
        public int Int1 { get; set; }
        public int Int2 { get; set; }
        public int Int3 { get; set; }
        public int Int4 { get; set; }
        public String[] List1 { get; set; }
        public String[] List2 { get; set; }
        public String[] List3 { get; set; }
        public String[] List4 { get; set; }
    }
}
