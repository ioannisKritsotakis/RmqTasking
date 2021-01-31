using Newtonsoft.Json;

namespace RmqTasking
{
    public class TaskModel
    {
        public TaskModel() { }

        public TaskModel(string id, int delay)
        {
            Id = id;
            DelayInSeconds = delay;
        }

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("delay")]
        public int DelayInSeconds;
    }
}
