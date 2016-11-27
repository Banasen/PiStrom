using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PiStrom.Config
{
    /// <summary>
    /// Represents the timespan something is valid.
    /// </summary>
    [JsonObject]
    public sealed class Time
    {
        /// <summary>
        /// Gets the time (in minutes from 00:00) at which this timespan starts (inclusive).
        /// </summary>
        [JsonProperty("from", Required = Required.Always)]
        [JsonConverter(typeof(TimeConverter))]
        public uint From { get; private set; }

        /// <summary>
        /// Gets the time (in minutes from 00:00) at which this timespan ends (inclusive).
        /// </summary>
        [JsonProperty("till", Required = Required.Always)]
        [JsonConverter(typeof(TimeConverter))]
        public uint Till { get; private set; }

        /// <summary>
        /// Checks whether the given time falls into the timespan covered.
        /// </summary>
        /// <param name="time">The time (in minutes from 00:00) to check.</param>
        /// <returns>Whether the given time falls into the timespan covered.</returns>
        public bool Covers(uint time)
        {
            return time >= From && time <= Till;
        }

        private sealed class TimeConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(uint) == objectType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader);
                switch (token.Type)
                {
                    case JTokenType.Integer:
                        return token.Value<uint>();

                    case JTokenType.String:
                        var value = token.Value<string>();
                        TimeSpan time;
                        if (TimeSpan.TryParseExact(value, "h\\:mm", CultureInfo.InvariantCulture, out time))
                            return (uint)time.TotalMinutes;
                        else
                            throw new JsonException("Time must be in HH:mm format.");

                    default:
                        throw new JsonException("Unexpected format.");
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var time = TimeSpan.FromMinutes((int)value);
                writer.WriteValue(time.ToString("hh\\:mm"));
            }
        }
    }
}