using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using CirclePrefect.Basiq;
using System.Net;
using System.Net.Http;

namespace CgiBin
{
    public class Stats : IDisposable
    {
        internal static string StatsApiUrl;
        internal static string DbName = "weekly_stats";
        private static StringReader reader;
        //private static StringWriter writer;
        private static DataStore db;
        private static readonly string CodeBlock = "```";
        public static long maxStats = 15;
        public static Meter meter = Meter.CEP;

        public Stats(bool writeWeeklyCep = false)
        {
            string file = GetFile();
            reader = new StringReader(file);
            db = new DataStore(DbName);
            db.Initialize();
        }
        private string GetFile()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            HttpClient http = new HttpClient();
            return http.GetStringAsync(StatsApiUrl).GetAwaiter().GetResult();
        }
        public List<Entry> GetStatsPlayer(IList<Result> stats, string username)
        {
            List<Entry> result = new List<Entry>();
            Result first = stats.FirstOrDefault(t => t.username == username);
            if (first.username != string.Empty)
            { 
                result.Add(Entry.NewEntry("", first, (Faction)first.faction, 1));
            }
            return result;
        }
        public List<Entry> GetStatsFaction(IList<Result> stats, Faction faction)
        {
            List<Entry> result = new List<Entry>();
            int num = 0;
            for (int i = 0; i < stats.Count; i++)
            {
                if (num > maxStats - 1)
                    break;
                if (stats[i].faction != (int)faction)
                    continue;
                if (stats[i].total == 0)
                    continue;
                num++;
                string index = num.ToString();
                if (index.Length == 1) index = index.Insert(0, " ");
                result.Add(Entry.NewEntry(index, stats[i], (Faction)stats[i].faction, num));
            }
            return result;
        }
        public List<Entry> GetStatsWeekly(IList<Result> stats)
        {
            List<Entry> result = new List<Entry>();
            stats = stats.OrderByDescending(t => t.weekly).ToList();
            int num = 0;
            for (int i = 0; i < stats.Count; i++)
            {
                if (num > maxStats - 1)
                    break;
                if (stats[i].weekly == 0)
                    continue;
                num++;
                string index = num.ToString();
                if (index.Length == 1) index = index.Insert(0, " ");
                result.Add(Entry.NewEntry(index, stats[i], (Faction)stats[i].faction, num));
            }
            return result;
        }
        public List<Entry> GetStatsTotal(IList<Result> stats)
        {
            List<Entry> result = new List<Entry>();
            for (int i = 0; i < stats.Count; i++)
            {
                if (i > maxStats) break;
                string index = i.ToString();
                if (index.Length == 1) index = index.Insert(0, " ");
                result.Add(Entry.NewEntry(index, stats[i], (Faction)stats[i].faction, i));
            }
            return result;
        }


        public string ListStatsFaction(IList<Result> stats, Faction faction)
        {
            string result = CodeBlock + $"All-time {faction} stats:\n\n";

            var count = stats.Count(t => t.total > 0 && t.faction == (int)faction);
            int num = 0;
            for (int i = 0; i < stats.Count; i++)
            {
                if (num > maxStats - 1)
                    break;
                if (stats[i].faction != (int)faction)
                    continue;
                if (stats[i].total == 0)
                    continue;
                num++;
                string index = num.ToString();
                if (index.Length == 1) index = index.Insert(0, " ");
                string text = $"{index}. {stats[i].username}";
                while (text.Length < 30)
                {
                    text += " ";
                }
                result += text + stats[i].total + "\n";
            }
            return result + CodeBlock;
        }
        public string ListStatsWeekly(IList<Result> stats)
        {
            stats = stats.OrderByDescending(t => t.weekly).ToList();
            string result = CodeBlock + "Weekly stats:\n\n";

            var count = stats.Count(t => t.weekly > 0);
            int num = 0;
            for (int i = 0; i < stats.Count; i++)
            {
                if (num > maxStats - 1) 
                    break;
                if (stats[i].weekly == 0) 
                    continue;
                num++;
                string index = num.ToString();
                if (index.Length == 1) index = index.Insert(0, " ");
                string text = $"{index}. {(Faction)stats[i].faction} {stats[i].username}";
                while (text.Length < 30)
                {
                    text += " ";
                }
                result += text + stats[i].weekly + "\n";
            }
            return result + CodeBlock;
        }
        public string ListStatsTotal(IList<Result> stats)
        {
            string result = CodeBlock + "Total stats:\n\n";

            var count = stats.Count(t => t.total > 0);
            for (int i = 0; i < stats.Count; i++)
            {
                if (i > maxStats) break;
                string index = (i + 1).ToString(); 
                if (index.Length == 1) index = index.Insert(0, " ");
                string text = $"{index}. {(Faction)stats[i].faction} {stats[i].username}";
                while (text.Length < 30)
                {
                    text += " ";
                }
                result += text + stats[i].total + "\n";
            }
            return result + CodeBlock;
        }
        internal IList<Result> GetStats(bool writeWeeklyCep = false, Meter meter = Meter.CEP)
        {
            var _data = GetData();
            IList<Result> result = new List<Result>();
            foreach (var _char in _data)
            {
                int id = _char.id;
                string name = _char.name;
                int cep = _char.cep;
                int bep = _char.bep;
                int faction = _char.faction;
                string header = name + "_" + id;
                if (!db.BlockExists(header))
                {
                    db.NewBlock(new string[] { "id", "name", "faction_id", "bep", "cep", "weekly_cep", "weekly_bep" }, new string[] 
                    {  
                        id.ToString(),
                        name,
                        faction.ToString(),
                        bep.ToString(),
                        cep.ToString(),
                        "0",
                        "0"
                    }, header);
                }
                else
                {
                    Block data = db.GetBlock(header);
                    int.TryParse(data.GetValue("cep"), out int previous);
                    int weekly = (cep - previous) / 100;
                    int total = cep / 100;
                    if (writeWeeklyCep)
                    {
                        data.WriteValue("weekly_cep", weekly);
                    }
                    int.TryParse(data.GetValue("bep"), out previous);
                    int weekly2 = (bep - previous) / 1000;
                    int total2 = bep / 1000;
                    if (writeWeeklyCep)
                    {
                        data.WriteValue("weekly_bep", weekly2);
                        data.WriteValue("bep", bep);
                        data.WriteValue("cep", cep);
                    }
                    switch (meter)
                    {
                        case Meter.CEP:
                            result.Add(new Result(name, weekly, faction, total));
                            break;
                        case Meter.BEP:
                            result.Add(new Result(name, weekly2, faction, total2));
                            break;
                    }
                }
            }
            db.WriteToFile();
            return result;
        }

        protected static JsonTextReader GetReader()
        {
            return new JsonTextReader(reader);
        }
        internal IList<Character> GetData()
        {
            var json = Newtonsoft.Json.JsonSerializer.CreateDefault();
            var array = json.Deserialize(GetReader()) as JObject;
            return array.First.First.ToObject<IList<Character>>();
        }
        public void Dispose()
        {
            reader?.Dispose();
        }
    }
    public class Character
    {
        [JsonProperty("id")] public int id;
        [JsonProperty("name")] public string name;
        [JsonProperty("faction_id")] public int faction;
        [JsonProperty("bep")] public int bep;
        [JsonProperty("cep")] public int cep;
        public override string ToString()
        {
            return $"{{\"id\":{id},\"name\":\"{name}\",\"faction_id\":{faction},\"bep\":\"{bep}\",\"cep\":\"{cep}\"}}";
        }
    }
    public struct Result
    {
        public Result(string username, int weekly, int faction, int total)
        {
            this.username = username;
            this.weekly = weekly;
            this.faction = faction;
            this.total = total;
        }
        public string username;
        public int weekly;
        public int faction;
        public int total;
    }
    public struct Entry
    {
        private Entry(string content, Result data, Faction faction, int index)
        {
            this.index = index;
            this.faction = faction;
            this.data = data;
            this.content = content;
        }
        public Faction faction;
        public Result data;
        public string content;
        public int index;
        public static Entry NewEntry(string content, Result data, Faction faction, int index)
        {
            return new Entry(content, data, faction, index);
        }
    }
    public enum Faction : int
    {
        TR = 0,
        NC = 1,
        VS = 2
    }
    public enum StatType
    {
        Weekly,
        Monthly,
        Total
    }
    public enum Meter
    {
        CEP,
        BEP
    }
}
