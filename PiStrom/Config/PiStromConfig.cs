using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiStrom.Config
{
    /// <summary>
    /// Represents the configuration for PiStrøm.
    /// </summary>
    [JsonObject]
    public sealed class PiStromConfig
    {
        /// <summary>
        /// Get the music that will be played if no other music is specified for a stream.
        /// </summary>
        [JsonProperty("defaultMusic")]
        public MusicSource DefaultMusic { get; private set; }

        /// <summary>
        /// Gets the port that the server will serve the streams on.
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; private set; }
    }
}