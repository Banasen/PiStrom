using PiStrom.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PiStrom
{
    public class MusicStream
    {
        private Dictionary<TcpClient, bool> clients;
        private int delay;
        private byte[] fileBuffer;
        private FileStream fileStream;
        private byte[] metaBuffer;
        private Random random = new Random();
        public bool Running { get; private set; }
        public StreamInfo StreamInfo { get; set; }

        public MusicStream(string configPath)
        {
            XmlReader reader = XmlReader.Create(configPath);
            XmlSchema schema = new XmlSchema();
            schema.SourceUri = @"Config" + Path.DirectorySeparatorChar + "StreamInfo.xsd";
            reader.Settings.Schemas.Add(schema);
            XmlSerializer serializer = new XmlSerializer(typeof(StreamInfo));
            StreamInfo = (StreamInfo)serializer.Deserialize(reader);

            clients = new Dictionary<TcpClient, bool>();

            fileBuffer = new byte[StreamInfo.MetaInt];

            delay = (int)(((double)StreamInfo.MetaInt / (double)StreamInfo.TargetByteRate) * 1000d);

            Running = false;
        }

        public void AddClient(TcpClient client, bool metaInfo)
        {
            string responseHeader = "HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nServer: PiStrøm\r\nCache-Control: no-cache\r\nPragma: no-cache\r\nConnection: close\r\n" + (metaInfo ? "icy-metaint:" + StreamInfo.MetaInt + "\r\nicy-name:" + StreamInfo.Name + "\r\nicy-genre:" + StreamInfo.Genre + "\r\n" : "") + "\r\n"; //icy-url:http://localhost:1337\r\n type: audio/mpeg

            var headerBytes = Encoding.UTF8.GetBytes(responseHeader);
            client.GetStream().WriteAsync(headerBytes, 0, headerBytes.Length);

            lock (clients)
                clients.Add(client, metaInfo);
        }

        public void Run()
        {
            Running = true;

            DateTime lastSend = DateTime.Now.AddDays(-1);

            if (fileStream == null)
                selectNextSong();

            while (clients.Count > 0)
            {
                int read = fileStream.Read(fileBuffer, 0, StreamInfo.MetaInt);

                if (read < StreamInfo.MetaInt)
                {
                    selectNextSong();

                    fileStream.Read(fileBuffer, read, StreamInfo.MetaInt - read);
                }

                var remove = new List<TcpClient>();

                int sinceLastSend = (int)(DateTime.Now - lastSend).TotalMilliseconds;
                if (sinceLastSend < delay)
                {
                    Thread.Sleep(delay - sinceLastSend);
                }

                lastSend = DateTime.Now;

                lock (clients)
                {
                    foreach (var client in clients)
                    {
                        try
                        {
                            client.Key.GetStream().WriteAsync(fileBuffer, 0, fileBuffer.Length);
                            if (client.Value) client.Key.GetStream().WriteAsync(metaBuffer, 0, metaBuffer.Length);
                        }
                        catch
                        {
                            remove.Add(client.Key);
                        }
                    }
                }

                remove.ForEach(client => clients.Remove(client));
            }

            Running = false;
        }

        private void selectNextSong()
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream.Dispose();
            }

            string[] possibleFiles = StreamInfo.Music.GetFilesForTime((uint)(DateTime.Now.Hour * 60 + DateTime.Now.Minute)).ToArray();
            if (possibleFiles.Length < 1)
                possibleFiles = Program.Config.DefaultMusic.GetFilesForFileType(StreamInfo.Music.FileType).ToArray();

            int fileIndex = random.Next(0, possibleFiles.Length);

            fileStream = File.OpenRead(possibleFiles[fileIndex]);

            List<byte> metaByteBuffer = new List<byte>();

            var meta = "StreamTitle='" + Path.GetFileNameWithoutExtension(possibleFiles[fileIndex]);
            var metaBytes = Encoding.UTF8.GetBytes(meta);

            metaByteBuffer.Add((byte)Math.Ceiling(metaBytes.Length / 16d));
            metaByteBuffer.AddRange(metaBytes);

            while ((metaByteBuffer.Count - 1) % 16 != 0)
                metaByteBuffer.Add(0);

            metaBuffer = metaByteBuffer.ToArray();
        }
    }
}