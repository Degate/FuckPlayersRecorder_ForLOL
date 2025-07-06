using System.Text.Json.Serialization;


namespace FuckPlayersRecorder_ForLOL.Event
{
    public class EventArgument
    {
        [JsonPropertyName("data")]
        public dynamic Data { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}
