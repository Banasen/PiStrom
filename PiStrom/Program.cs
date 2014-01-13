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
            Console.WriteLine("Waiting for Connections");
            FileServer fS = new FileServer();
            fS.Start();
            Console.ReadLine();
        }
    }
}