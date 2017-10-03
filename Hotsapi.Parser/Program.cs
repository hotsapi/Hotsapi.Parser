using Heroes.ReplayParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Hotsapi.Parser
{
    public class Program
    {
#if DEBUG
        const bool Debug = true;
#else
        const bool Debug = false;
#endif
        public static int Main(string[] args)
        {
            try {
                if (args.Length == 0) {
                    if (Debug) {
                        args = new string[] { @"V:\Development\heroprotocol\bd1449e2-4687-06f8-7761-8b2a4b3ed3df.StormReplay" };
                    } else {
                        Console.Error.WriteLine("Specify replay file to parse");
                        return 1;
                    }
                }            
                if (!File.Exists(args[0])) {
                    Console.Error.WriteLine($"File '{args[0]}' does not exist");
                    return 1;
                }
                var result = DataParser.ParseReplay(args[0], false, false);
                if (result.Item1 != DataParser.ReplayParseResult.Success || result.Item2 == null) {
                    Console.Error.WriteLine($"Error parsing replay: {result.Item1}");
                    return 1;
                }
                Console.WriteLine(ToJson(result.Item2));
                return 0;
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Error parsing replay: {ex}");
                return 1;
            }
        }

        public static string ToJson(Replay replay)
        {
            var obj = new {
                mode = replay.GameMode.ToString(),
                date = replay.Timestamp,
                length = replay.ReplayLength,
                map = replay.Map,
                version = replay.ReplayVersion,
                bans = replay.TeamHeroBans,
                players = from p in replay.Players
                          select new {
                              battletag_name = p.Name,
                              battletag_id = p.BattleTag,
                              blizz_id = p.BattleNetId,
                              hero = p.Character,
                              hero_level = p.CharacterLevel,
                              team = p.Team,
                              winner = p.IsWinner,
                              silenced = p.IsSilenced,
                              party = p.PartyValue,
                              talents = p.Talents.Select(t => t.TalentName),
                              score = p.ScoreResult
                }
            };
            return JObject.FromObject(obj).ToString(Debug ? Formatting.Indented : Formatting.None);
        }
    }
}
