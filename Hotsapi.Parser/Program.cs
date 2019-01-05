using Heroes.ReplayParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Hotsapi.Parser
{
    public static class Program
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
                    Console.Error.WriteLine("Specify replay file to parse");
                    return 1;
                }            
                if (!File.Exists(args[0])) {
                    Console.Error.WriteLine($"File '{args[0]}' does not exist");
                    return 1;
                }
                var result = DataParser.ParseReplay(args[0], false, false, skipUnitParsing: true, skipMouseMoveEvents: true);
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

        private static string ToJson(Replay replay)
        {
            var obj = new {
                mode = replay.GameMode.ToString(),
                date = replay.Timestamp,
                length = replay.ReplayLength,
                map = replay.Map,
                version = replay.ReplayVersion,
                bans = replay.TeamHeroBans,
                teams = replay.TeamPeriodicXPBreakdown.Select(x => x.Last()).Select((xp, i) => new {
                    winner = i == replay.GetWinnerTeam(),
                    xp.TeamLevel,
                    xp.CreepXP,
                    xp.HeroXP,
                    xp.MinionXP,
                    xp.StructureXP,
                    xp.TrickleXP,
                    xp.TotalXP
                }),
                players = replay.GetPlayerInPickOrder().Select(p => new {
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
                })
            };
            return JObject.FromObject(obj).ToString(Debug ? Formatting.Indented : Formatting.None);
        }

        public static int GetWinnerTeam(this Replay replay)
        {
            return replay.Players.First(p => p.IsWinner).Team;
        }

        public static Player[] GetPlayerInPickOrder(this Replay replay)
        {
            return replay.DraftOrder.Count < 10 ? 
                replay.Players : 
                replay.DraftOrder
                    .Where(x => x.PickType == DraftPickType.Picked)
                    .Select(x => replay.Players[x.SelectedPlayerSlotId])
                    .ToArray();   
        }
    }
}
