using Microsoft.Extensions.Logging;
using RmqTasking;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Receiver
{
    public class TaskExecutioner
    {
        private readonly Channel<TaskModel> _channel;

        public string Id { get; }
        private ChannelReader<TaskModel> ModelReader { get; }

        private readonly ILogger<TaskDistributor> _logger;

        public TaskExecutioner(string id, ILogger<TaskDistributor> logger)
        {
            _channel = Channel.CreateUnbounded<TaskModel>();
            ModelReader = _channel.Reader;
            Id = id;
            _logger = logger;
        }

        public bool SendJob(TaskModel taskModel)
        {
            return _channel.Writer.TryWrite(taskModel);
        }


        public async Task Consume(CancellationToken cancellationToken)
        {
            // Receive the messages with id {Id} and process them
            // When finished close gracefully. CancellationTokenSource
            while (!ModelReader.Completion.IsCompleted && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var taskModel = await ModelReader.ReadAsync(cancellationToken);
                    await ShowDelay(taskModel, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"The operation was cancelled for {Id}");
                    await Task.CompletedTask;
                }
            }

        }

        private async Task ShowDelay(TaskModel obj, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received Task {obj.Id}");
            _logger.LogInformation($"Awaiting {obj.DelayInSeconds} seconds for Task {obj.Id}");
            await Task.Delay(TimeSpan.FromSeconds(obj.DelayInSeconds), cancellationToken);
            _logger.LogInformation($"Finished awaiting {obj.DelayInSeconds} seconds for Task {obj.Id}");
        }
    }
}
