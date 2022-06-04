using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommandLineParserPoC.Tests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestBinaryShort()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary1 = true
                }
            };

            CommandLineParser.Parse(new[] { "-a" }, sd);

            Assert.AreEqual(o.Binary1, true);
        }

        [TestMethod]
        public void TestBinaryLong()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary1 = true
                }
            };

            CommandLineParser.Parse(new[] { "--all-that" }, sd);

            Assert.AreEqual(o.Binary1, true);
        }

        [TestMethod]
        public void TestMultipleBinariesSeparately()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary1 = true
                },
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = "bugger-all",
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                }
            };

            CommandLineParser.Parse(new[] { "--all-that", "-b" }, sd);

            Assert.AreEqual(o.Binary1, true);
            Assert.AreEqual(o.Binary2, true);
        }

        [TestMethod]
        public void TestMultipleBinariesTogether()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary1 = true
                },
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary3 = true
                },
                new SwitchDescription
                {
                    ShortName = 'd',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary4 = true
                },
            };

            CommandLineParser.Parse(new[] { "--all-that", "-bcd" }, sd);

            Assert.AreEqual(o.Binary1, true);
            Assert.AreEqual(o.Binary2, true);
            Assert.AreEqual(o.Binary3, true);
            Assert.AreEqual(o.Binary4, true);
        }

        [TestMethod]
        public void TestString()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a => o.String1 = a
                }
            };

            CommandLineParser.Parse(new[] { "-a", "Hallo" }, sd);

            Assert.AreEqual(o.String1, "Hallo");
        }
        [TestMethod]
        public void TestIntBad()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a =>
                    {
                        int v;
                        if (!int.TryParse(a, out v))
                        {
                            throw new ArgumentOutOfRangeException($"You tried to pass value {a} for integer option --all-that. It does not appear to be that integer");
                        }
                        o.Int1 = v;
                    }
                }
            };


            Assert.ThrowsException<ArgumentOutOfRangeException>(() => CommandLineParser.Parse(new[] { "-a", "77n" }, sd));
        }
        [TestMethod]
        public void TestIntGood()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a =>
                    {
                        int v;
                        if (!int.TryParse(a, out v))
                        {
                            throw new ArgumentOutOfRangeException($"You tried to pass value {a} for integer option --all-that. It does not appear to be that integer");
                        }
                        o.Int1 = v;
                    }
                }
            };


            CommandLineParser.Parse(new[] { "-a", "77" }, sd);
            Assert.AreEqual(o.Int1, 77);
        }
        [TestMethod]
        public void TestList()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.List,
                    SetValue = a => o.List1 = a.Split(' ').Select(x => x.Trim()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray()
                },
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = "bugger-all",
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = "capla",
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a => o.String1 = a
                }

            };


            CommandLineParser.Parse(new[] { "--capla", "zeta", "-a", "77", "123", "hello", "-b" }, sd);
            Assert.AreEqual(o.List1[0], "77");
            Assert.AreEqual(o.List1[1], "123");
            Assert.AreEqual(o.List1[2], "hello");
            Assert.AreEqual(o.List1.Length, 3);
            Assert.AreEqual(o.Binary2, true);
            Assert.AreEqual(o.String1, "zeta");
        }

        [TestMethod]
        public void TestQuotedList()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a => o.List1 = a.Split(' ').Select(x => x.Trim()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray()
                },
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = "bugger-all",
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                }
            };


            CommandLineParser.Parse(new[] { "-a", "-a -b 1234 -c hello", "-b" }, sd);
            Assert.AreEqual(o.List1[0], "-a");
            Assert.AreEqual(o.List1[1], "-b");
            Assert.AreEqual(o.List1[2], "1234");
            Assert.AreEqual(o.List1[3], "-c");
            Assert.AreEqual(o.List1[4], "hello");
            Assert.AreEqual(o.List1.Length, 5);
            Assert.AreEqual(o.Binary2, true);
        }

        [TestMethod]
        public void TestBadSequence()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'a',
                    LongName = "all-that",
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a => { o.String1 = a; }
                }
            };

            Assert.ThrowsException<CommandLineParserException>(() => CommandLineParser.Parse(new[] { "-a", "Hello", "World" }, sd));

        }

        [TestMethod]
        public void TestBinariesAndIntInFront()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary3 = true
                },
                new SwitchDescription
                {
                    ShortName = 'd',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary4 = true
                },
                new SwitchDescription
                {
                    ShortName = 'e',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a =>
                    {
                        int v;
                        if (!int.TryParse(a, out v))
                        {
                            throw new ArgumentOutOfRangeException($"You tried to pass value {a} for integer option --all-that. It does not appear to be that integer");
                        }
                        o.Int1 = v;
                    }
                },
            };

            CommandLineParser.Parse(new[] { "-ebcd", "77" }, sd);

            Assert.AreEqual(o.Binary2, true);
            Assert.AreEqual(o.Binary3, true);
            Assert.AreEqual(o.Binary4, true);
            Assert.AreEqual(o.Int1, 77);
        }
        [TestMethod]
        public void TestBinariesAndIntInMiddle()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary3 = true
                },
                new SwitchDescription
                {
                    ShortName = 'd',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary4 = true
                },
                new SwitchDescription
                {
                    ShortName = 'e',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a =>
                    {
                        int v;
                        if (!int.TryParse(a, out v))
                        {
                            throw new ArgumentOutOfRangeException($"You tried to pass value {a} for integer option --all-that. It does not appear to be that integer");
                        }
                        o.Int1 = v;
                    }
                },
            };

            CommandLineParser.Parse(new[] { "-bced", "77" }, sd);

            Assert.AreEqual(o.Binary2, true);
            Assert.AreEqual(o.Binary3, true);
            Assert.AreEqual(o.Binary4, true);
            Assert.AreEqual(o.Int1, 77);
        }
        [TestMethod]
        public void TestBinariesAndIntAtEnd()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary3 = true
                },
                new SwitchDescription
                {
                    ShortName = 'd',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary4 = true
                },
                new SwitchDescription
                {
                    ShortName = 'e',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Value,
                    SetValue = a =>
                    {
                        int v;
                        if (!int.TryParse(a, out v))
                        {
                            throw new ArgumentOutOfRangeException($"You tried to pass value {a} for integer option --all-that. It does not appear to be that integer");
                        }
                        o.Int1 = v;
                    }
                },
            };

            CommandLineParser.Parse(new[] { "-bcde", "77" }, sd);

            Assert.AreEqual(o.Binary2, true);
            Assert.AreEqual(o.Binary3, true);
            Assert.AreEqual(o.Binary4, true);
            Assert.AreEqual(o.Int1, 77);
        }

        [TestMethod]
        public void TestBinariesAndListInFront()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary3 = true
                },
                new SwitchDescription
                {
                    ShortName = 'd',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary4 = true
                },
                new SwitchDescription
                {
                    ShortName = 'e',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.List,
                    SetValue = a => o.List1 = a.Split(' ').Select(x => x.Trim()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray()
                },
            };

            CommandLineParser.Parse(new[] { "-ebcd", "77" }, sd);

            Assert.AreEqual(o.Binary2, true);
            Assert.AreEqual(o.Binary3, true);
            Assert.AreEqual(o.Binary4, true);
            Assert.AreEqual(o.List1[0], "77");
            Assert.AreEqual(o.List1.Length, 1);
        }
        [TestMethod]
        public void TestBinariesAndListInMiddle()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary3 = true
                },
                new SwitchDescription
                {
                    ShortName = 'd',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary4 = true
                },
                new SwitchDescription
                {
                    ShortName = 'e',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.List,
                    SetValue = a => o.List1 = a.Split(' ').Select(x => x.Trim()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray()
                },
            };

            CommandLineParser.Parse(new[] { "-becd", "77" }, sd);

            Assert.AreEqual(o.Binary2, true);
            Assert.AreEqual(o.Binary3, true);
            Assert.AreEqual(o.Binary4, true);
            Assert.AreEqual(o.List1[0], "77");
            Assert.AreEqual(o.List1.Length, 1);
        }
        [TestMethod]
        public void TestBinariesAndListAtEnd()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary3 = true
                },
                new SwitchDescription
                {
                    ShortName = 'd',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary4 = true
                },
                new SwitchDescription
                {
                    ShortName = 'e',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.List,
                    SetValue = a => o.List1 = a.Split(' ').Select(x => x.Trim()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray()
                },
            };

            CommandLineParser.Parse(new[] { "-bcde", "77" }, sd);

            Assert.AreEqual(o.Binary2, true);
            Assert.AreEqual(o.Binary3, true);
            Assert.AreEqual(o.Binary4, true);
            Assert.AreEqual(o.List1[0], "77");
            Assert.AreEqual(o.List1.Length, 1);
        }
        [TestMethod]
        public void TestMoreThanOneNonBinary()
        {
            Options o = new Options();

            SwitchDescription[] sd = {
                new SwitchDescription
                {
                    ShortName = 'b',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary2 = true
                },
                new SwitchDescription
                {
                    ShortName = 'c',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary3 = true
                },
                new SwitchDescription
                {
                    ShortName = 'd',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.Binary,
                    SetValue = a => o.Binary4 = true
                },
                new SwitchDescription
                {
                    ShortName = 'e',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.List,
                    SetValue = a => o.List1 = a.Split(' ').Select(x => x.Trim()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray()
                },
                new SwitchDescription
                {
                    ShortName = 'f',
                    LongName = null,
                    Description = "Desc 1",
                    Type = SwitchType.List,
                    SetValue = a => o.List2 = a.Split(' ').Select(x => x.Trim()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray()
                },
            };

            Assert.ThrowsException<CommandLineParserException>(() => CommandLineParser.Parse(new[] { "-bfcde", "77" }, sd));
        }
        //[TestMethod]
        //public void TestErrorMessageWhenValueIsUnexpected()
        //{
        //    Options o = new Options();

        //    SwitchDescription[] sd = {
        //        new SwitchDescription
        //        {
        //            ShortName = 'a',
        //            LongName = "all-that",
        //            Description = "Desc 1",
        //            Type = SwitchType.Binary,
        //            SetValue = a => o.Binary1 = true
        //        }
        //    };

        //    CommandLineParser.Parse(new[] { "not_a_switch" }, sd);

        //    Assert.AreEqual(o.Binary1, true);

        //}
    }
}
