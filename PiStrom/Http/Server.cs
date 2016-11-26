using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PiStrom.Http
{
    /// <summary>
    /// Represents the Server used to handle the incoming requests.
    /// </summary>
    public sealed class Server
    {
        /// <summary>
        /// <see cref="CancellationTokenSource"/> to provide a <see cref="CancellationToken"/> for stopping the listener <see cref="Task"/> and those handling the incoming connections.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The directory that the Server bases the paths on.
        /// </summary>
        private DirectoryInfo rootDirectory;

        private Dictionary<string, MusicStream> streams = new Dictionary<string, MusicStream>();

        /// <summary>
        /// <see cref="TcpListener"/> to listen for incoming requests.
        /// </summary>
        private TcpListener tcpListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> Class.
        /// </summary>
        /// <param name="allowedAdresses">The address or address types that are allowed to connect.</param>
        /// <param name="port">The port the server should listen on.</param>
        public Server(IPAddress allowedAdresses, int port, DirectoryInfo rootDirectory)
        {
            tcpListener = new TcpListener(allowedAdresses, port);
            this.rootDirectory = rootDirectory;

            Connection += server_Connection;
        }

        public void Start()
        {
            tcpListener.Start();

            cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
                {
                    while (true)
                    {
                        var client = await tcpListener.AcceptTcpClientAsync();

                        onConnection(client);
                    }
                }, cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();

                tcpListener.Stop();
            }
        }

        private void onConnection(TcpClient client)
        {
            Connection?.Invoke(client, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Handles the incoming connections.
        /// </summary>
        /// <param name="client">The socket that the connection uses.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for closing the connection.</param>
        private void server_Connection(TcpClient client, CancellationToken cancellationToken)
        {
            var reader = new StreamReader(client.GetStream());

            string request = "";
            Dictionary<string, string> headers = new Dictionary<string, string>();

            byte[] buffer = new byte[1];

            try
            {
                string received = null;

                while (received != "")
                {
                    received = reader.ReadLine();

                    if (received != null && received.Length > 2)
                    {
                        string[] splitReceived = received.Split(':');

                        if (splitReceived.Length < 2)
                        {
                            request = splitReceived[0];
                        }
                        else
                        {
                            if (!headers.ContainsKey(splitReceived[0]))
                                headers.Add(splitReceived[0], string.Join(":", splitReceived.Skip(1)).Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Request: " + request);
            Console.WriteLine("Headers:");
            foreach (var header in headers)
            {
                Console.WriteLine(header.Key + ": " + header.Value);
            }
            Console.WriteLine("End of Headers");
            Console.WriteLine();

            var sendIcyMeta = headers.ContainsKey("Icy-MetaData") && headers["Icy-MetaData"] == "1";

            string[] requestSplit = request.Split(' ');

            if (streams.ContainsKey(requestSplit[1]))
            {
                streams[requestSplit[1]].AddClient(client, sendIcyMeta);

                if (!streams[requestSplit[1]].Running)
                    Task.Run(() => (Action)streams[requestSplit[1]].Run, cancellationToken);
            }
            else
            {
                string path = Path.Combine(rootDirectory.FullName, "Streams", requestSplit[1].TrimStart('/').Replace('/', Path.DirectorySeparatorChar) + ".xml");

                if (File.Exists(path))
                {
                    MusicStream musicStream = new MusicStream(path);

                    musicStream.AddClient(client, sendIcyMeta);

                    Task.Run((Action)musicStream.Run, cancellationToken);

                    streams.Add(requestSplit[1], musicStream);
                }
                else
                {
                    client.Close();
                }
            }
        }

        public event ConnectionEventHandler Connection;

        public delegate void ConnectionEventHandler(TcpClient client, CancellationToken cancellationToken);
    }
}