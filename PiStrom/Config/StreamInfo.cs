using System;
using System.Collections.Generic;
using System.IO;
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
        /// The Interval (in bytes) in which meta information about the stream is sent. Also the size of the byte buffer.
        /// </summary>
        public int MetaInt { get; set; }

        /// <summary>
        /// Gets or sets the Music that is played on the stream.
        /// </summary>
        public MusicInfo Music { get; set; }

        /// <summary>
        /// Represents the music that will be played on the stream, divided into timespans.
        /// </summary>
        [XmlRoot("Music")]
        public class MusicInfo
        {
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
                        return TimeSpan.MinutesToString(FromMinutes);
                    }
                    set
                    {
                        FromMinutes = TimeSpan.StringToMinutes(value);
                    }
                }

                /// <summary>
                /// Gets or sets the time at which this <see cref="TimeSpan"/> starts. Minutes from 00:00.
                /// </summary>
                [XmlIgnore()]
                public uint FromMinutes { get; set; }

                /// <summary>
                /// Gets or sets the time at which this <see cref="TimeSpan"/> ends. Format: hh:mm, from 00:00 to 23:59
                /// </summary>
                [XmlAttribute("Till")]
                public string Till
                {
                    get
                    {
                        return TimeSpan.MinutesToString(TillMinutes);
                    }
                    set
                    {
                        TillMinutes = TimeSpan.StringToMinutes(value);
                    }
                }

                /// <summary>
                /// Gets or sets the time at which this <see cref="TimeSpan"/> ends. Minutes from 00:00.
                /// </summary>
                [XmlIgnore()]
                public uint TillMinutes { get; set; }

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
                /// Converts a time in the format hh:mm, from 00:00 to 23:59 into the minutes from 00:00.
                /// </summary>
                /// <param name="strTime">The time as string.</param>
                /// <returns>The minutes from 00:00.</returns>
                public static uint StringToMinutes(string strTime)
                {
                    if (!Regex.IsMatch(strTime, "(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]")) throw new FormatException(strTime + " doesn't match format hh:mm, from 00:00 to 23:50!");

                    string[] splitTime = strTime.Split(':');
                    return uint.Parse(splitTime[0]) * 60 + uint.Parse(splitTime[1]);
                }

                /// <summary>
                /// Converts the time in minutes from 00:00 into the format hh:mm, from 0:00 to 23:59.
                /// </summary>
                /// <param name="time">The time from 00:00.</param>
                /// <returns>The time as a string.</returns>
                public static string MinutesToString(uint time)
                {
                    if (time > 1439) throw new ArgumentOutOfRangeException("Time must be between 0 (00:00) and 1439 (23:59).");

                    uint minutes = time % 60;
                    uint hours = (time - minutes) / 60;
                    return (hours < 10 ? "0" : "") + hours + ":" + (minutes < 10 ? "0" : "") + minutes;
                }
            }

            /// <summary>
            /// Gets the paths to all the files that can be played at the given time.
            /// </summary>
            /// <param name="time">The time in minutes from 00:00.</param>
            /// <returns>The paths to all the files for that time.</returns>
            public List<string> GetFilesForTime(uint time)
            {
                List<string> files = new List<string>();

                foreach (TimeSpan timeSpan in TimeSpans)
                {
                    if (timeSpan.FromMinutes <= time && time <= timeSpan.TillMinutes)
                    {
                        files.AddRange(timeSpan.Files);

                        foreach (string folder in timeSpan.Folders)
                        {
                            files.AddRange(Directory.GetFiles(folder, "*.mp3", SearchOption.AllDirectories));
                        }
                    }
                }

                return files;
            }
        }
    }
}