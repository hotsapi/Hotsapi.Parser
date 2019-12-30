using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.IO;

namespace Hotsapi.Parser.Test
{
    [TestClass]
    public class ProgramTest
    {
        private string fixturePath = "";

        [TestInitialize]
        public void Init()
        {
            fixturePath = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName, "Fixtures");
        }

        private void RunTest(string name)
        {
            var expected = File.ReadAllText(Path.Combine(fixturePath, $"{name}.json"), Encoding.UTF8).Trim();
            var actual = Program.ParseReplay(Path.Combine(fixturePath, $"{name}.StormReplay")).Trim();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void alterac_quickmatch_2_45_StormReplay_is_parsed_correctly()
        {
            RunTest("alterac_quickmatch_2.45");
        }

        [TestMethod]
        public void sky_storm_league_2_46_StormReplay_is_parsed_correctly()
        {
            RunTest("sky_storm_league_2.46");
        }

        [TestMethod]
        public void infernal_storm_league_2_47_StormReplay_is_parsed_correctly()
        {
            RunTest("infernal_storm_league_2.47");
        }

        [TestMethod]
        public void blackheart_unranked_2_48_StormReplay_is_parsed_correctly()
        {
            RunTest("blackheart_unranked_2.48");
        }

        [TestMethod]
        public void dragon_custom_2_48_StormReplay_is_parsed_correctly()
        {
            RunTest("dragon_custom_2.48");
        }

        [TestMethod]
        public void volskaya_unranked_2_48_StormReplay_is_parsed_correctly()
        {
            RunTest("volskaya_unranked_2.48");
        }

        [TestMethod]
        public void braxis_storm_league_2_49_StormReplay_is_parsed_correctly()
        {
            RunTest("braxis_storm_league_2.49");
        }

        [TestMethod]
        public void dragon_quickmatch_2_49_StormReplay_is_parsed_correctly()
        {
            RunTest("dragon_quickmatch_2.49");
        }

        [TestMethod]
        public void warhead_quickmatch_2_49_StormReplay_is_parsed_correctly()
        {
            RunTest("warhead_quickmatch_2.49");
        }
    }
}
