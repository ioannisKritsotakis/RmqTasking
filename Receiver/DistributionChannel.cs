using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RmqTasking;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Receiver
{
    public interface IDistributionChannel
    {
        bool WriteToChannel(TaskModel item);
        ChannelReader<TaskModel> Reader { get; }
    }

    public class DistributionChannel : IHostedService, IDistributionChannel
    {
        private readonly Channel<TaskModel> _channel;

        public ChannelReader<TaskModel> Reader => _channel.Reader;

        private ILogger<DistributionChannel> _Logger;

        public DistributionChannel(ILogger<DistributionChannel> logger)
        {
            _Logger = logger;
            _channel = Channel.CreateUnbounded<TaskModel>();
        }

        public bool WriteToChannel(TaskModel item)
        {
            return _channel.Writer.TryWrite(item);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _Logger.LogInformation("Starting DistributionChannel");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _Logger.LogInformation("Starting DistributionChannel");
            return Task.CompletedTask;

        }
    }
}
