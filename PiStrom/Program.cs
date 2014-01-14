using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PiStrom
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            HttpServer httpServer = new HttpServer(System.Net.IPAddress.Any, 1337, rootDirectory);
            httpServer.Start();

            Console.ReadLine();
            httpServer.Stop();
            Console.WriteLine("Server stopped.");
            Console.ReadLine();
        }
    }
}