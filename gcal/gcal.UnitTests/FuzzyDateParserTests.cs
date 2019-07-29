using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gcal;

namespace gcal.UnitTests
{
    [TestClass]
    public class FuzzyDateParserTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            TimeSpan Time = FuzzyDateParser.ParseTime("15 minutes");
            Assert.IsTrue(Time.Hours == 0);
            Assert.IsTrue(Time.Days == 0);
            Assert.IsTrue(Time.Minutes == 15);
        }
    }
}
