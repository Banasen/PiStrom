using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PiStrom.Config
{
    /// <summary>
    /// Represents a collection of possible music.
    /// </summary>
    [JsonObject]
    public sealed class MusicSource
    {
        /// <summary>
        /// Gets the files that can be played in the stream. Doesn't include the files included in the <see cref="Folders"/>.
        /// </summary>
        [JsonProperty("files", DefaultValueHandling = DefaultValueHandling.Populate)]
        public ReadOnlyCollection<string> Files { get; private set; }

        /// <summary>
        /// Gets the folders from which music files for the stream can be sourced.
        /// </summary>
        [JsonProperty("folders", DefaultValueHandling = DefaultValueHandling.Populate)]
        public ReadOnlyCollection<string> Folders { get; private set; }

        /// <summary>
        /// Gets the time in which this music is available to be played.
        /// </summary>
        [JsonProperty("time", Required = Required.DisallowNull)]
        public Time Time { get; private set; }

        /// <summary>
        /// Gets the paths to all the files that are included in this music source.
        /// </summary>
        /// <param name="fileType">The filetype of the music files.</param>
        /// <returns>The paths to all the files for that time.</returns>
        public IEnumerable<string> GetFilesForFileType(string fileType)
        {
            if (Files != null)
                foreach (var file in Files.Where(file => file.EndsWith(fileType, StringComparison.InvariantCultureIgnoreCase)))
                    yield return file;

            if (Folders != null)
                foreach (string folder in Folders)
                    foreach (var file in Directory.GetFiles(folder, "*." + fileType, SearchOption.AllDirectories))
                        yield return file;
        }
    }
}