using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiStrom
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HttpServer httpServer = new HttpServer(System.Net.IPAddress.Any, 1337);
            httpServer.Start();

            Console.ReadLine();
            httpServer.Stop();
            Console.WriteLine("Server stopped.");
            Console.ReadLine();
        }
    }
}