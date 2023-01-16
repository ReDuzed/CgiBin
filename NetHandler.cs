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
        private TcpListener tcp;
        public NetHandler() { }
        private NetHandler(int port)
        {
            tcp = new TcpListener(IPAddress.Any, port);
            tcp.Start(10);
        }
        internal Task Begin(int port)
        {
            return new Task(_begin, port);
        }
        private async void _begin(object state)
        {
            int _port = (int)state;
            new NetHandler(_port);
            Info info = new Info() { port = _port };
            NetworkStream ns = null;
            while (true)
            { 
                info.client = tcp.AcceptTcpClient();
                byte[] buffer = new byte[256];
                using (ns = new NetworkStream(info.client.Client))
                {
                    await ns.ReadAsync(buffer, 0, buffer.Length);
                }
                if (Array.TrueForAll(buffer, t => t == default(byte)))
                {
                    info.client.Dispose(); 
                    continue;
                }
                info.args = Encoding.ASCII
                    .GetString(buffer)
                    .Replace("\0", "")
                    .Split(' ');
                info.file = InputResult(info.args);
                await StartSend(info);
            }
        }
        private Task StartSend(Info info)
        {
            return Task.Factory.StartNew(SendHandler, info);
        }
        private async void SendHandler(object state)
        {
            Info info = (Info)state;
            NetworkStream net = new NetworkStream(info.client.Client);
            int tries = 0;
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
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i - 1])
                {
                    case "--maxstats":
                        long.TryParse(args[i], out Stats.maxStats);
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
                        file = Raster.RasterizeToFile(600, stats.GetStatsPlayer(list, args[i]), $"{_header(type)} character stats", type);
                        stats.Dispose();
                        break;
                }
            }
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--week":
                        file = Raster.RasterizeToFile(600, stats.GetStatsWeekly(list), "Weekly stats", StatType.Weekly);
                        stats.Dispose();
                        break;
                    case "--total":
                        file = Raster.RasterizeToFile(600, stats.GetStatsTotal(list), "All-time stats", StatType.Monthly);
                        stats.Dispose();
                        break;
                    case "--tr":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.TR), $"{_header(type)}  TR stats", type);
                        stats.Dispose();
                        break;
                    case "--nc":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.NC), $"{_header(type)}  NC stats", type);
                        stats.Dispose();
                        break;
                    case "--vs":
                        file = Raster.RasterizeToFile(600, stats.GetStatsFaction(list, Faction.VS), $"{_header(type)}  VS stats", type);
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
        }
    }
}
