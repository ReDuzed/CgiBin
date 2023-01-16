using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CgiBin
{
    public class NetHandler
    {
        private static TcpListener tcp;
        public NetHandler() { }
        private NetHandler(int port)
        {
            tcp = new TcpListener(IPAddress.Any, port);
            tcp.Start(10);
Console.WriteLine($"TCP listening on {port}");
        }
        internal Task Begin(int port)
        {
            var timer = new System.Timers.Timer(TimeSpan.FromMinutes(3).TotalMilliseconds);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += (sender, e) => 
            { 
                try
                { 
                    foreach (string _file in Directory.GetFiles(Raster.Path()))
                    {
                        if (!_file.Contains("texture.png"))
                        { 
                            File.Delete(_file);
Console.WriteLine($"    Deleted: {_file}");
                        }
                    }
                }
                catch (Exception c)
                {
                    Console.WriteLine(c.Message);
                }
            };
            timer.Start();
Console.WriteLine("Stats image file purging every 3 minutes has started");
            return Task.Factory.StartNew(_begin, port);
        }
        private async void _begin(object state)
        {
            int _port = (int)state;
            new NetHandler(_port);
            Info info = new Info() { port = _port };
Console.WriteLine($"Info object initialized");
            NetworkStream ns = null;
            while (true)
            { 
                try
                { 
Console.WriteLine($"Waiting for client connection {tcp.LocalEndpoint}");
                    info.client = tcp.AcceptTcpClient();
                    byte[] buffer = new byte[256];
                    using (ns = new NetworkStream(info.client.Client))
                    {
                        int num = await ns.ReadAsync(buffer, 0, buffer.Length);
Console.WriteLine($"{num} bytes read from {info.client.Client}");
                    }
                    if (Array.TrueForAll(buffer, t => t == default(byte)))
                    {
Console.WriteLine($"{info.client.Client} read nothing therefore continuing");
                        info.client.Dispose(); 
                        continue;
                    }
                    info.args = Encoding.ASCII
                        .GetString(buffer)
                        .Replace("\0", "")
                        .Split(' ');
                    info.file = InputResult(info.args);
Console.WriteLine("Argument inputs:");
Array.ForEach(info.args.TakeWhile(t => t.Length > 1).ToArray(), t => Console.WriteLine("    " + t));
Console.WriteLine($"{info} received and file output to local directory");

                    continue;
Console.WriteLine($"Starting to send file over network {info.client.Client.RemoteEndPoint}");
                    await StartSend(info);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        private Task WriteFile(Info info)
        {
            return Task.Factory.StartNew(OutputFile, info);
        }
        private void OutputFile(object state)
        {
            Info info = (Info)state;
        }
        private Task StartSend(Info info)
        {
            return Task.Factory.StartNew(SendHandler, info);
        }
        private async void SendHandler(object state)
        {
            Info info = (Info)state;
            NetworkStream net = null;
            int tries = 0;
            try
            {
                net = new NetworkStream(info.client.Client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            SEND:
            try
            {
                byte[] buffer = new byte[4096];
                using (FileStream fs = new FileStream(info.file, FileMode.Open))
                { 
                    int read = await fs.ReadAsync(buffer, 0, buffer.Length);
                    await net.WriteAsync(buffer, 0, read);
                }
            }
            catch
            {
                if (tries++ < 5)
                    goto SEND;
                else return;
            }
        }

        private string InputResult(string[] args)
        {
            string rand = string.Empty;
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i - 1])
                {
                    case "--maxstats":
                        long.TryParse(args[i], out Stats.maxStats);
                        break;
                    case "--rand":
                        rand = args[i];
                        break;
                }
            }
            string file = string.Empty;
            StatType type = StatType.Monthly;
            Stats stats = new Stats();
            var list = stats.GetStats(false);
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i - 1])
                {
                    case "--weekly":
                        type = StatType.Weekly;
                        break;
                    case "--player":
                        file = Raster.RasterizeToFile(600, stats.GetStatsPlayer(list, args[i]), $"{_header(type)} character stats", type, rand);
                        stats.Dispose();
                        break;
                }
            }
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--week":
                        file = Raster.RasterizeToFile(600, stats.GetStatsWeekly(list), "Weekly stats", StatType.Weekly, rand);
                        stats.Dispose();
                        break;
                    case "--total":
                        file = Raster.RasterizeToFile(600, stats.GetStatsTotal(list), "All-time stats", StatType.Monthly, rand);
                        stats.Dispose();
                        break;
                    case "--tr":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.TR), $"{_header(type)}  TR stats", type, rand);
                        stats.Dispose();
                        break;
                    case "--nc":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.NC), $"{_header(type)}  NC stats", type, rand);
                        stats.Dispose();
                        break;
                    case "--vs":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.VS), $"{_header(type)}  VS stats", type, rand);
                        stats.Dispose();
                        break;
                    case "--bgflag":
                        Raster.renderBackground = true;
                        break;
                }
            }
            return file;
        }
        private string _header(StatType type)
        {
            return type == StatType.Monthly ? "All-time" : "Weekly";
        }

        internal class Info
        {
            internal TcpClient client;
            public string[] args;
            public string file;
            public int port;
            public override string ToString()
            {
                return $"File: {file}, Port: {port}, Length: {args.Length}";
            }
        }
    }
}
