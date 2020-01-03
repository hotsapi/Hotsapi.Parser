using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Hotsapi.Parser.Test
{
    [TestClass]
    public class ReplayParseTest
    {
        [DataTestMethod]
        [DynamicData(nameof(GetFileNames), DynamicDataSourceType.Method)]
        public void TestReplayParsing(string name)
        {
            var filePath = Path.Combine(GetFixturePath(), name);
            var expected = File.ReadAllText($"{filePath}.json", Encoding.UTF8).Trim();
            var actual = Program.ParseReplay($"{filePath}.StormReplay").Trim();

            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<object[]> GetFileNames()
        {
            var fixturePath = GetFixturePath();
            foreach (var file in Directory.EnumerateFiles(fixturePath)) {
                if (file.EndsWith(".StormReplay")) {
                    yield return new object[] { Path.GetFileNameWithoutExtension(file) };
                }
            }
        }

        private static string GetFixturePath()
        {
            return Path.Combine(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName, "Fixtures");
        }
    }
}
