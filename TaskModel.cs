using Newtonsoft.Json;

namespace RmqTasking
{
    public class TaskModel
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("delay")]
        public int DelayInSeconds;
    }
}
