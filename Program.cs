using System;

namespace CgiBin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i - 1])
                {
                    case "--maxstats":
                        long.TryParse(args[i], out Stats.maxStats);
                        break;
                    case "--texture":
                        Raster.texture = args[i];
                        break;
                    case "--output":
                        Raster.file = args[i];
                        break;
                    case "--apiurl":
                        Stats.StatsApiUrl = args[i];
                        break;
                    case "--dbname":
                        Stats.DbName = args[i];
                        break;
                    case "--runserver":
                        int.TryParse(args[i], out int port);
                        new NetHandler().Begin(port);
                        while (Console.ReadLine() != "exit");
                        Environment.Exit(0);
                        return;
                }
            }
            string file = string.Empty;
            Stats stats = new Stats();
            var list = stats.GetStats(false);
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i - 1])
                {
                    case "--player":
                        file = Raster.RasterizeToFile(600, stats.GetStatsPlayer(list, args[i]), "All-time character stats", StatType.Monthly);
                        stats.Dispose();
                        break;
                }
            }
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--help":
                    case "/?":
                    case "-help":
                        Console.WriteLine("" +
                            "Command                Info\n\n" +
                            "[/? | --help | -help]  Displays help text\n" +
                            "--maxstats [number]    How many stats to display (Int64)\n" +
                            "--[tr | nc | vs]       Which faction to output stats for\n" +
                            "--week                 Weekly stats\n" +
                            "--total                Total stats\n" +
                            "--character [name]     Character stats" +
                            "--output [path]        Output of temporary graphic file\n" +
                            "--texture [path]       Texture of text background for output\n" +
                            "--bgflag               Set rendering of background to true\n" +
                            "--apiurl [url]         API URL\n" +
                            "--runserver [number]   Starts the TCP server with specified port");
                        return;
                    case "--week":
                        file = Raster.RasterizeToFile(600, stats.GetStatsWeekly(list), "Weekly stats", StatType.Weekly);
                        stats.Dispose();
                        break;
                    case "--total":
                        file = Raster.RasterizeToFile(600, stats.GetStatsTotal(list), "All-time stats", StatType.Monthly);
                        stats.Dispose();
                        break;
                    case "--tr":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.TR), "All-time TR stats", StatType.Monthly);
                        stats.Dispose();
                        break;
                    case "--nc":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.NC), "All-time NC stats", StatType.Monthly);
                        stats.Dispose();
                        break;
                    case "--vs":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.VS), "All-time VS stats", StatType.Monthly);
                        stats.Dispose();
                        break;
                    case "--bgflag":
                        Raster.renderBackground = true;
                        break;
                }
            }
        }
    }
}
