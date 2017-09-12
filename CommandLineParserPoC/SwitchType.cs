namespace CommandLineParserPoC
{
    // These are the different switch types specified in the requirements
    public enum SwitchType
    {
        Binary, // switch with no value
        Int, // switch with the next argument of a single integer value
        String, // switch with the next argument of a single string value
        QuotedList, // switch with the next argument of a single value that is a white-space separated list of opaque values
        List // switch with the next arguments of one or more string values
    }
}
