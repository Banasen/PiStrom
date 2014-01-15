using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PiStrom.Config
{
    /// <summary>
    /// Represents the Configuration of the Program.
    /// </summary>
    [XmlRoot("PiStrom")]
    public class PiStromConfig
    {
        /// <summary>
        /// Gets or sets the port that the server will serve the streams on.
        /// </summary>
        public uint Port { get; set; }

        /// <summary>
        /// Gets or sets the music that will be played if no music is available for a stream.
        /// </summary>
        public Music DefaultMusic { get; set; }

        public class Music
        {
            /// <summary>
            /// Gets or sets the <see cref="List"/> of folders from which music files for the stream can be sourced.
            /// </summary>
            [XmlElement("Folder")]
            public List<string> Folders { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="List"/> of files which can be played in the stream. Doesn't include the files from the Folders <see cref="List"/>.
            /// </summary>
            [XmlElement("File")]
            public List<string> Files { get; set; }

            /// <summary>
            /// Gets the paths to all the files that can be played at the given time.
            /// </summary>
            /// <param name="time">The time in minutes from 00:00.</param>
            /// <returns>The paths to all the files for that time.</returns>
            public List<string> GetFilesForFileType(string fileType)
            {
                List<string> files = new List<string>(Files.Where(file => file.EndsWith(fileType) && File.Exists(file)));

                foreach (string folder in Folders)
                {
                    files.AddRange(Directory.GetFiles(folder, "*." + fileType, SearchOption.AllDirectories));
                }

                return files;
            }
        }
    }
}