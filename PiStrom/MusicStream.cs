using PiStrom.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PiStrom
{
    public class MusicStream
    {
        public StreamInfo StreamInfo { get; set; }

        private Dictionary<Socket, bool> clients;

        private FileStream fileStream;

        private byte[] fileBuffer;

        private byte[] metaBuffer;

        private Random random = new Random();

        public MusicStream(string configPath)
        {
            XmlReader reader = XmlReader.Create(configPath);
            XmlSchema schema = new XmlSchema();
            schema.SourceUri = @"Config\StreamInfo.xsd";
            reader.Settings.Schemas.Add(schema);
            XmlSerializer serializer = new XmlSerializer(typeof(StreamInfo));
            StreamInfo = (StreamInfo)serializer.Deserialize(reader);

            clients = new Dictionary<Socket, bool>();

            fileBuffer = new byte[StreamInfo.MetaInt];
        }

        public void AddClient(Socket client, bool metaInfo)
        {
            string responseHeader = "HTTP/1.1 200 OK\r\nContent-Type: audio/mpeg\r\nServer: PiStrøm\r\nCache-Control: no-cache\r\nPragma: no-cache\r\nConnection: close\r\n" + (metaInfo ? "icy-metaint:" + StreamInfo.MetaInt + "\r\nicy-name:" + StreamInfo.Name + "\r\nicy-genre:" + StreamInfo.Genre + "\r\n" : "") + "\r\n"; //icy-url:http://localhost:1337\r\n
            client.Send(Encoding.UTF8.GetBytes(responseHeader));
            clients.Add(client, metaInfo);
        }

        public void Run(CancellationToken cancellationToken)
        {
            fileStream = File.OpenRead("Empty");
            fileStream.Position = fileStream.Length;

            while (!cancellationToken.IsCancellationRequested && clients.Count > 0)
            {
                long restOfFile = fileStream.Length - fileStream.Position;

                fileStream.Read(fileBuffer, 0, StreamInfo.MetaInt);

                metaBuffer = new byte[1] { (byte)0 };

                if (restOfFile < StreamInfo.MetaInt)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                    string[] possibleFiles = StreamInfo.Music.GetFilesForTime((uint)(DateTime.Now.Hour * 60 + DateTime.Now.Minute)).ToArray();
                    int fileIndex = random.Next(0, possibleFiles.Length);
                    fileStream = File.OpenRead(possibleFiles[fileIndex]);
                    fileStream.Read(fileBuffer, (int)restOfFile, (int)(StreamInfo.MetaInt - restOfFile));

                    List<byte> metaByteBuffer = new List<byte>();

                    string meta = "StreamTitle='" + Regex.Match(possibleFiles[fileIndex], @"\\[^\\]*$").Value + "';";
                    meta = meta.PadRight(meta.Length + (16 - (meta.Length % 16)));
                    byte[] metaBytes = Encoding.UTF8.GetBytes(meta);

                    metaByteBuffer.Add((byte)(metaBytes.Length / 16));
                    metaByteBuffer.AddRange(metaBytes);
                    metaBuffer = metaByteBuffer.ToArray();
                }

                List<Socket> remove = new List<Socket>();

                Parallel.ForEach(clients, client =>
                {
                    try
                    {
                        client.Key.Send(fileBuffer);
                        if (client.Value) client.Key.Send(metaBuffer);
                    }
                    catch
                    {
                        remove.Add(client.Key);
                    }
                });

                remove.ForEach(client => clients.Remove(client));
            }
        }
    }
}