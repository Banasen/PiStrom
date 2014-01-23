using PiStrom.Config;
using PiStrom.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PiStrom
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlReader reader = XmlReader.Create("Config" + Path.DirectorySeparatorChar + "PiStrom.xml");
            XmlSchema schema = new XmlSchema();
            schema.SourceUri = "Config" + Path.DirectorySeparatorChar + "PiStrom.xsd";
            reader.Settings.Schemas.Add(schema);
            XmlSerializer serializer = new XmlSerializer(typeof(PiStromConfig));
            Config = (PiStromConfig)serializer.Deserialize(reader);

            if (Config.DefaultMusic.Files.Count < 1 && Config.DefaultMusic.Folders.Count < 1) throw new Exception("No default music provided.");

            DirectoryInfo rootDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            Server httpServer = new Server(IPAddress.Any, (int)Config.Port, rootDirectory);
            httpServer.Start();

            Console.ReadLine();
            httpServer.Stop();
            Console.WriteLine("Server stopped.");
            Console.ReadLine();
        }

        public static PiStromConfig Config;
    }
}