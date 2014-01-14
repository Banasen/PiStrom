using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PiStrom.Config
{
    /// <summary>
    /// Represents the information about a Stream.
    /// </summary>
    [XmlRoot("StreamInfo")]
    public class StreamInfo
    {
        /// <summary>
        /// Gets or sets the Name of the stream. Used in the Http response header as icy-name:Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Genre of the stream. Used in the Http response header as icy-genre:Genre.
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// Gets or sets the Music that is played on the stream.
        /// </summary>
        public Music Music { get; set; }

        /// <summary>
        /// Represents the music that will be played on the stream, divided into timespans.
        /// </summary>
        [XmlRoot("Music")]
        public class Music
        {
            /// <summary>
            /// Gets or sets the path that the folders and files will be relative to, if they aren't absolute.
            /// </summary>
            [XmlAttribute("RelativeTo")]
            public string RelativeTo { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="List"/> of <see cref="TimeSpans"/> for the stream.
            /// </summary>
            [XmlElement("TimeSpan")]
            public List<TimeSpan> TimeSpans { get; set; }

            /// <summary>
            /// Represents a timespan and what music will be played in it.
            /// </summary>
            [XmlRoot("TimeSpan")]
            public class TimeSpan
            {
                /// <summary>
                /// Gets or sets the time at which this <see cref="TimeSpan"/> starts. Format: hh:mm, from 00:00 to 23:59
                /// </summary>
                [XmlAttribute("From")]
                public string From
                {
                    get
                    {
                        int minutes = FromMinutes % 60;
                        int hours = (FromMinutes - minutes) / 60;
                        return (hours < 10 ? "0" : "") + hours + ":" + (minutes < 10 ? "0" : "") + minutes;
                    }
                    set
                    {
                        if (!Regex.IsMatch(value, "(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]")) throw new FormatException(value + " doesn't match format hh:mm, from 00:00 to 23:50!");

                        string[] splitTime = value.Split(':');
                        FromMinutes = int.Parse(splitTime[0]) * 60 + int.Parse(splitTime[1]);
                    }
                }

                /// <summary>
                /// Gets or sets the time at which this <see cref="TimeSpan"/> starts. Minutes from 00:00.
                /// </summary>
                [XmlIgnore()]
                public int FromMinutes { get; set; }

                /// <summary>
                /// Gets or sets the time at which this <see cref="TimeSpan"/> ends. Format: hh:mm, from 00:00 to 23:59
                /// </summary>
                [XmlAttribute("From")]
                public string Till
                {
                    get
                    {
                        int minutes = TillMinutes % 60;
                        int hours = (TillMinutes - minutes) / 60;
                        return (hours < 10 ? "0" : "") + hours + ":" + (minutes < 10 ? "0" : "") + minutes;
                    }
                    set
                    {
                        if (!Regex.IsMatch(value, "(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]")) throw new FormatException(value + " doesn't match format hh:mm, from 00:00 to 23:50!");

                        string[] splitTime = value.Split(':');
                        TillMinutes = int.Parse(splitTime[0]) * 60 + int.Parse(splitTime[1]);
                    }
                }

                /// <summary>
                /// Gets or sets the time at which this <see cref="TimeSpan"/> ends. Minutes from 00:00.
                /// </summary>
                [XmlIgnore()]
                public int TillMinutes { get; set; }

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
            }
        }
    }
}