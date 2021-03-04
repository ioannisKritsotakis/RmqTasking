using Newtonsoft.Json;
using Receiver.Models;
using System.Threading.Channels;

namespace Receiver
{
    public interface IDistributionChannel
    {
        bool WriteTaskModelToChannel(TaskModel item);
        bool WriteHeartbeatToChannel(HeartbeatModel item);
        ChannelReader<string> Reader { get; }
    }

    public class DistributionChannel : IDistributionChannel
    {
        private readonly Channel<string> _channel;

        public bool WriteTaskModelToChannel(TaskModel item)
        {
            return _channel.Writer.TryWrite(JsonConvert.SerializeObject(item));
        }

        public bool WriteHeartbeatToChannel(HeartbeatModel item)
        {
            return _channel.Writer.TryWrite(JsonConvert.SerializeObject(item));
        }

        public ChannelReader<string> Reader => _channel.Reader;

        public DistributionChannel()
        {
            _channel = Channel.CreateUnbounded<string>();
        }

        public bool WriteToChannel(string item)
        {
            return _channel.Writer.TryWrite(item);
        }
    }
}
