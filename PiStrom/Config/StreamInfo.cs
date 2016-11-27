using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PiStrom.Config
{
    /// <summary>
    /// Represents an available radio stream.
    /// </summary>
    [JsonObject]
    public class StreamInfo
    {
        /// <summary>
        /// Gets the Genre of the stream. Used in the http response header as icy-genre:Genre.
        /// </summary>
        [JsonProperty("genre")]
        public string Genre { get; private set; }

        /// <summary>
        /// Gets the interval (in bytes) in which meta information about the stream is sent. Also the size of the byte buffer.
        /// </summary>
        [JsonProperty("metaInterval")]
        public int MetaInterval { get; private set; }

        /// <summary>
        /// Gets the Music that is played on the stream.
        /// </summary>
        [JsonProperty("music")]
        public MusicInfo Music { get; private set; }

        /// <summary>
        /// Gets the Name of the stream. Used in the http response header as icy-name:Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the maximum rate at which data is sent. In bytes per second.
        /// </summary>
        [JsonProperty("targetByteRate")]
        public int TargetByteRate { get; private set; }

        /// <summary>
        /// Represents the music that will be played on the stream.
        /// </summary>
        [JsonObject]
        public sealed class MusicInfo
        {
            /// <summary>
            /// Get the file type (mp3, ogg, etc.) that the stream will use.
            /// </summary>
            [JsonProperty("fileType")]
            public string FileType { get; private set; }

            /// <summary>
            /// Gets the music that will be played on the stream.
            /// </summary>
            [JsonProperty("sources")]
            public ReadOnlyCollection<MusicSource> Sources { get; set; }

            /// <summary>
            /// Gets the paths to all the files that can be played at the given time.
            /// </summary>
            /// <param name="time">The time in minutes from 00:00.</param>
            /// <returns>The paths to all the files for the time.</returns>
            public IEnumerable<string> GetFilesForTime(uint time)
            {
                foreach (var source in Sources)
                {
                    if (source.Time.Covers(time))
                    {
                        foreach (var file in source.GetFilesForFileType(FileType))
                            yield return file;
                    }
                }
            }
        }
    }
}