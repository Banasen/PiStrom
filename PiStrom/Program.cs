using Newtonsoft.Json;
using PiStrom.Config;
using PiStrom.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace PiStrom
{
    internal class Program
    {
        public static PiStromConfig Config;

        private static readonly JsonSerializer serializer = new JsonSerializer();

        private static void Main(string[] args)
        {
            Config = serializer.Deserialize<PiStromConfig>(new JsonTextReader(new StreamReader("PiStrom.json")));

            if (Config.DefaultMusic.GetFilesForFileType("").Any(file => File.Exists(file)))
                throw new Exception("No default music exists.");

            DirectoryInfo rootDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            Server httpServer = new Server(IPAddress.Any, (int)Config.Port, rootDirectory);
            httpServer.Start();

            Console.ReadLine();
            httpServer.Stop();
            Console.WriteLine("Server stopped.");
            Console.ReadLine();
        }
    }
}