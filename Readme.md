# Command Line parser requirements

 - Written in C# so command line arguments come in a string array.
 - All arguments are _switches_ some of those come with vaules (value switches) and some do not (binary switches)
 - Values in value switches are never optional. Values do not start with '-' or '/'
 - Every element of the input array of a conforming command line belongs to a switch (or its value). That is there are no commands, just switches.
 - An example of binary switch: `-a`; an example of value switch: `-b fidget`
 - Switches start with either `-` (but not `--`) or `/` - those are treated the same
 - Switches can have two forms representing the same switch, the short form was shown above the long form start with `--`. Example: `--thingamabob bleh`
 - Both binary and value switches can have a long or a short form
 - Switches can have either long or short form or both
 - Several short switches can be comnibed into one: `-abcd`. This combination cannot contain more than one value switch.
 - There are two types of value switches: string and list (e.g `--servers server1 server2 server3`).
 - It expected that the parser is aware of the type of every switch, so that if it's a value switch it will know to treat the subsequent input array item as the switch argument (or several argurments for list switches).
