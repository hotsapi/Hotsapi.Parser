using Heroes.ReplayParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

[assembly: InternalsVisibleTo("Hotsapi.Parser.Test")]

namespace Hotsapi.Parser
{
    public static class Program
    {
#if DEBUG
        const bool Debug = true;
#else
        const bool Debug = false;
#endif

        private const string PathBase = "/tmp";
        public static int Main(string[] args)
        {
            try {
                if (args.Length == 0) {
                    Listen();
                    return 0;
                }            
                if (!File.Exists(args[0])) {
                    Console.Error.WriteLine($"File '{args[0]}' does not exist");
                    return 1;
                }

                var result = ParseReplay(args[0]);
                if (result == null) {
                    return 1;
                }
                Console.WriteLine(result);
                return 0;
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Error parsing replay: {ex}");
                return 1;
            }
        }

        internal static string ParseReplay(string fileName)
        {
            var (replayParseResult, replay) = DataParser.ParseReplay(fileName, false, false, skipUnitParsing: true, skipMouseMoveEvents: true);
            if (replayParseResult != DataParser.ReplayParseResult.Success || replay == null) {
                Console.Error.WriteLine($"Error parsing replay: {replayParseResult}");
                return null;
            }
            return ToJson(replay);
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
                    Winner = i == replay.GetWinnerTeam(),
                    FirstPick = i == replay.GetFirstPickTeam(),
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
        
        public static int GetFirstPickTeam(this Replay replay)
        {
            return replay.DraftOrder.Count < 10 ? -1 : replay.Players.First().Team;
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

        public static void Listen()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/");
            listener.Start();
            Console.WriteLine("Listening...");
            while (true) {
                try {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(o => {
                        try {
                            var request = context.Request;
                            var response = context.Response;

                            // todo: more strictly validate request filename 
                            if (request.HttpMethod != "GET" || !Regex.Match(request.RawUrl, @"/[\w]+").Success) {
                                Console.WriteLine("Got invalid request");
                                response.StatusCode = 401;
                                response.OutputStream.Close();
                                return;
                            }

                            var filePath = Path.Combine(PathBase, request.RawUrl.Remove(0, 1));
                            var result = ParseReplay(filePath);
                            if (result == null) {
                                response.StatusCode = 503;
                                response.OutputStream.Close();
                                return;
                            }
                            
                            var buffer = System.Text.Encoding.UTF8.GetBytes(result);
                            response.ContentLength64 = buffer.Length;
                            response.Headers.Add("Content-Type", "application/json");
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                            response.OutputStream.Close();
                        }
                        catch (Exception e) {
                            Console.WriteLine(e.ToString());
                        }
                    });
                } 
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
