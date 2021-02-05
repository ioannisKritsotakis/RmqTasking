using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using RmqTasking;

namespace Receiver
{
    public class TaskExecutioner
    {
        private Channel<TaskModel> _channel;

        public string Id { get; }
        private ChannelReader<TaskModel> ModelReader { get; }

        public TaskExecutioner(string id)
        {
            _channel = Channel.CreateUnbounded<TaskModel>();
            ModelReader = _channel.Reader;
            Id = id;
        }

        public bool Enqueue(TaskModel taskModel)
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

        private static async Task ShowDelay(TaskModel obj, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received Task {obj.Id} with execId: {obj.ExecutionId}");
            Console.WriteLine($"Awaiting {obj.DelayInSeconds} seconds for Task {obj.Id}");
            await Task.Delay(TimeSpan.FromSeconds(obj.DelayInSeconds), cancellationToken);
            Console.WriteLine($"Finished awaiting {obj.DelayInSeconds} seconds for Task {obj.Id} with execId: {obj.ExecutionId}");
        }
    }
}
