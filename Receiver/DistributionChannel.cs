using RmqTasking;
using System.Threading.Channels;

namespace Receiver
{
    public interface IDistributionChannel
    {
        bool WriteToChannel(TaskModel item);
        ChannelReader<TaskModel> Reader { get; }
    }

    public class DistributionChannel : IDistributionChannel
    {
        private readonly Channel<TaskModel> _channel;

        public ChannelReader<TaskModel> Reader => _channel.Reader;

        public DistributionChannel()
        {
            _channel = Channel.CreateUnbounded<TaskModel>();
        }

        public bool WriteToChannel(TaskModel item)
        {
            return _channel.Writer.TryWrite(item);
        }
    }
}
