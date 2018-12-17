namespace CommandLineParserPoC
{
    // These are the different switch types specified in the requirements
    public enum SwitchType
    {
        Binary, // switch with no value
        Value, // switch with the next argument of a single integer value
        List // switch with the next arguments of one or more string values
    }
}
