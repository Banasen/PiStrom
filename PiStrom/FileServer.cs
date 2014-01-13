using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PiStrom
{
    public class FileServer
    {
        private static string hostName = "127.0.0.1";
        private static int port = 1337; //if this port is not available on your machine; pick up some other
        private IPAddress localAddr = IPAddress.Parse(hostName);

        public void Start()
        {
            TcpListener tcpListner = new TcpListener(localAddr, port);
            tcpListner.Start(); //start listening to client request
            while (true) //infinite loop
            {
                //blocks until a client request comes
                Socket socket = tcpListner.AcceptSocket();
                if (socket.Connected)
                {
                    Console.WriteLine("Connection");
                    //Delegate to the SendFileToClient method
                    SendFileToClient(socket);
                    socket.Close();
                }
            }
        }

        private void SendFileToClient(Socket socket)
        {
            List<string> headers = new List<string>();
            byte[] buffer = new byte[1];

            Console.WriteLine("Response Header:");

            try
            {
                socket.ReceiveTimeout = 100;
                string received = "";
                List<byte> line = new List<byte>();
                do
                {
                    socket.Receive(buffer);
                    line.Add(buffer[0]);

                    if (line.Count >= 2)
                    {
                        if (line[line.Count - 2] == CLRF[0] && line[line.Count - 1] == CLRF[1])
                        {
                            received = Encoding.UTF8.GetString(line.ToArray());
                            Console.Write(received);
                            headers.Add(received);

                            line.Clear();
                        }
                    }
                }
                while (true);
            }
            catch
            {
                Console.WriteLine("End of Response Header\r\n");
            }

            //Icy-MetaData: 1

            bool sendMeta = false;

            foreach (string header in headers)
            {
                if (header.ToLower().Replace(" ", "") == "icy-metadata:1\r\n")
                {
                    sendMeta = true;
                    break;
                }
            }

            socket.Send(Encoding.UTF8.GetBytes(responseHeader));

            string[] files = new string[] { @"C:\Users\Banane\Music\Binärpilot\Nordland\10 - Nordland.mp3", @"C:\Users\Banane\Music\Binärpilot\Nordland\01 - aXXo.mp3" };
            string[] names = new string[] { "Nordland", "aXXo" };

            buffer = new byte[32768];

            try
            {
                for (int i = 0; i < 2; i++)
                {
                    FileStream fileStream = File.OpenRead(files[i]);

                    while (fileStream.Position < fileStream.Length)
                    {
                        fileStream.Read(buffer, 0, buffer.Length);
                        socket.Send(buffer);

                        if (sendMeta)
                        {
                            List<byte> metaByteBuffer = new List<byte>();

                            string meta = "StreamTitle='" + names[i] + "';";
                            meta = meta.PadRight(meta.Length + (16 - (meta.Length % 16)));
                            byte[] metaBytes = Encoding.UTF8.GetBytes(meta);

                            metaByteBuffer.Add((byte)(metaBytes.Length / 16));
                            metaByteBuffer.AddRange(metaBytes);

                            socket.Send(metaByteBuffer.ToArray());
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Connection dropped.");
            }
        }

        private byte[] CLRF = Encoding.UTF8.GetBytes("\r\n");

        private string responseHeader = "HTTP/1.1 200 OK\r\nContent-Type: audio/mpeg\r\nServer: Banane9's Media Server Thing\r\nCache-Control: no-cache\r\nPragma: no-cache\r\nConnection: close\r\nicy-metaint:32768\r\nicy-name:Binärpilot Stream\r\nicy-genre:Electronic\r\nicy-url:http://localhost:65000\r\n\r\n";
    }
}