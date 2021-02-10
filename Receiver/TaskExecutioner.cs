using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Receiver
{
    public class TaskExecutioner
    {
        private readonly Channel<TaskModel> _channel;

        public string Type { get; }
        private ChannelReader<TaskModel> ModelReader { get; }

        private readonly ILogger<TaskDistributor> _logger;

        public Task _task;

        public TaskExecutioner(string type, ILogger<TaskDistributor> logger)
        {
            _channel = Channel.CreateUnbounded<TaskModel>();
            ModelReader = _channel.Reader;
            Type = type;
            _logger = logger;
        }

        public bool SendJob(TaskModel taskModel)
        {
            return _channel.Writer.TryWrite(taskModel);
        }


        public Task ExecuteTask(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(async () => await Consume(cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
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
                    _logger.LogWarning($"The operation was cancelled for Task Executioner of type {Type}");
                    await Task.CompletedTask;
                }
            }
        }

        private async Task ShowDelay(TaskModel obj, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Task {obj.Type} with id: {obj.Id:D2} - Received");
            _logger.LogInformation($"Task {obj.Type} with id: {obj.Id:D2} - Awaiting {obj.DelayInSeconds} seconds");
            await Task.Delay(TimeSpan.FromSeconds(obj.DelayInSeconds), cancellationToken);
            _logger.LogInformation($"Task {obj.Type} with id: {obj.Id:D2} - Finished awaiting {obj.DelayInSeconds} seconds");
        }
    }
}
