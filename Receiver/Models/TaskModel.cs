using Newtonsoft.Json;

namespace Receiver.Models
{
    public class TaskModel
    {
        public TaskModel() { }

        public TaskModel(string type, int delay, int id)
        {
            Type = type;
            DelayInSeconds = delay;
            Id = id;
        }

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("delay")]
        public int DelayInSeconds;

        [JsonProperty("type")]
        public string Type;

        public bool IsEmpty()
        {
            return Type == null || DelayInSeconds == 0 || Id == 0;
        }
    }
}
