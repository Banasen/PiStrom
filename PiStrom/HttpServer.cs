using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PiStrom
{
    /// <summary>
    /// Represents the Server used to handle the incoming requests.
    /// </summary>
    public sealed class HttpServer
    {
        /// <summary>
        /// <see cref="TcpListener"/> to listen for incoming requests.
        /// </summary>
        private TcpListener tcpListener;

        /// <summary>
        /// The directory that the Server bases the paths on.
        /// </summary>
        private DirectoryInfo rootDirectory;

        private Dictionary<string, MusicStream> streams = new Dictionary<string, MusicStream>();

        /// <summary>
        /// <see cref="CancellationTokenSource"/> to provide a <see cref="CancellationToken"/> for stopping the listener <see cref="Task"/> and those handling the incoming connections.
        /// </summary>
        private CancellationTokenSource cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> Class.
        /// </summary>
        /// <param name="allowedAdresses">The address or address types that are allowed to connect.</param>
        /// <param name="port">The port the server should listen on.</param>
        public HttpServer(IPAddress allowedAdresses, int port, DirectoryInfo rootDirectory)
        {
            tcpListener = new TcpListener(allowedAdresses, (int)port);
            this.rootDirectory = rootDirectory;

            Connection += OnConnection;
        }

        /// <summary>
        /// Handles the incoming connections.
        /// </summary>
        /// <param name="socket">The socket that the connection uses.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for closing the connection.</param>
        private void OnConnection(Socket socket, CancellationToken cancellationToken)
        {
            string request = "";
            Dictionary<string, string> headers = new Dictionary<string, string>();

            byte[] buffer = new byte[1];

            try
            {
                string received = "";

                while (received != "\r\n")
                {
                    socket.Receive(buffer);

                    received += Encoding.UTF8.GetString(buffer);

                    if (received.EndsWith("\r\n") && received.Length > 2)
                    {
                        received = received.TrimEnd('\r', '\n');
                        string[] splitReceived = received.Split(':');

                        if (splitReceived.Length < 2)
                        {
                            request = splitReceived[0];
                        }
                        else
                        {
                            headers.Add(splitReceived[0], string.Join(":", splitReceived.Skip(1)).Trim());
                        }

                        received = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Request: " + request);
            Console.WriteLine("Headers:");
            foreach (KeyValuePair<string, string> header in headers)
            {
                Console.WriteLine(header.Key + " -> " + header.Value);
            }
            Console.WriteLine("End of Headers");

            bool sendIcyMeta = false;
            //Icy-MetaData: 1
            if (headers.ContainsKey("Icy-MetaData"))
            {
                sendIcyMeta = headers["Icy-MetaData"] == "1";
            }

            string[] requestSplit = request.Split(' ');

            if (streams.ContainsKey(requestSplit[1]))
            {
                streams[requestSplit[1]].AddClient(socket, sendIcyMeta);

                if (!streams[requestSplit[1]].Running)
                    Task.Factory.StartNew(() => streams[requestSplit[1]].Run(cancellationToken));
            }
            else
            {
                string path = rootDirectory.FullName + Path.DirectorySeparatorChar + "Streams" + Path.DirectorySeparatorChar + requestSplit[1].TrimStart('/').Replace('/', Path.DirectorySeparatorChar) + ".xml";

                if (File.Exists(path))
                {
                    MusicStream musicStream = new MusicStream(path);

                    musicStream.AddClient(socket, sendIcyMeta);

                    Task.Factory.StartNew(() => musicStream.Run(cancellationToken));

                    streams.Add(requestSplit[1], musicStream);
                }
            }

            //socket.Send(Encoding.UTF8.GetBytes(responseHeader));

            //string[] files = new string[] { @"C:\Users\Banane\Music\Binärpilot\Nordland\10 - Nordland.mp3", @"C:\Users\Banane\Music\Binärpilot\Nordland\01 - aXXo.mp3" };
            //string[] names = new string[] { "Nordland", "aXXo" };

            //buffer = new byte[32768];

            //try
            //{
            //    for (int i = 0; i < 2; i++)
            //    {
            //        FileStream fileStream = File.OpenRead(files[i]);

            //        while (fileStream.Position < fileStream.Length)
            //        {
            //            fileStream.Read(buffer, 0, buffer.Length);
            //            socket.Send(buffer);

            //            if (sendIcyMeta)
            //            {
            //                List<byte> metaByteBuffer = new List<byte>();

            //                string meta = "StreamTitle='" + names[i] + "';";
            //                meta = meta.PadRight(meta.Length + (16 - (meta.Length % 16)));
            //                byte[] metaBytes = Encoding.UTF8.GetBytes(meta);

            //                metaByteBuffer.Add((byte)(metaBytes.Length / 16));
            //                metaByteBuffer.AddRange(metaBytes);

            //                socket.Send(metaByteBuffer.ToArray());
            //            }
            //        }
            //    }
            //}
            //catch
            //{
            //    Console.WriteLine("Connection dropped.");
            //}
        }

        public void Start()
        {
            tcpListener.Start();

            cancellationToken = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            Socket socket = tcpListener.AcceptSocket();

                            if (Connection != null)
                                Connection.BeginInvoke(socket, cancellationToken.Token, (ar) =>
                                    {
                                        try
                                        {
                                            ((ConnectionEventHandler)((AsyncResult)ar).AsyncDelegate).EndInvoke(ar);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                        }
                                    },
                               null);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("TcpListener Stopped.");
                    }
                });
        }

        public void Stop()
        {
            if (cancellationToken != null)
            {
                cancellationToken.Cancel();

                tcpListener.Stop();
            }
        }

        public delegate void ConnectionEventHandler(Socket socket, CancellationToken cancellationToken);

        public event ConnectionEventHandler Connection;
    }
}