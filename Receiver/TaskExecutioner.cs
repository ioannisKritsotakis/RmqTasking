using Microsoft.Extensions.Logging;
using Receiver.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Receiver
{
    public class TaskExecutioner
    {
        private readonly Queue<(Guid, TaskModel)> _channel;
        public string Type { get; }

        private readonly ILogger<TaskDistributor> _logger;

        public Task RunningTask;

        private int fakeFaulty = 2;

        public bool IsProcessDown =>
            RunningTask != null && (RunningTask.IsFaulted && RunningTask.Status == TaskStatus.Faulted);

        public TaskExecutioner(string type, ILogger<TaskDistributor> logger)
        {
            _channel = new Queue<(Guid, TaskModel)>();
            Type = type;
            _logger = logger;
        }

        public void SendNewJob(TaskModel taskModel)
        {
            _logger.LogInformation($"Task {taskModel.Type} with id: {taskModel.Id:D2} - Received");
            WriteItem(taskModel);
        }

        public Task FireUpTask(CancellationToken cancellationToken)
        {
            var createdTask = Task.Factory.StartNew(
                async () => await Consume(cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            RunningTask = createdTask.Unwrap();
            return createdTask;
        }

        public async Task Consume(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var (guid, taskModel) = await ReadItem(cancellationToken);
                    await ShowDelay(taskModel, cancellationToken);
                    AckItem(guid);
                }
                catch (ArgumentNullException ex)
                {
                    _logger.LogError($"The operation for Task Executioner of type {Type} threw an exception");
                    fakeFaulty--;
                    await Task.FromException(ex);
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
            if (obj.Type == "D" && fakeFaulty > 0) throw new ArgumentNullException();
            _logger.LogInformation($"Task {obj.Type} with id: {obj.Id:D2} - Started processing...");
            _logger.LogInformation($"Task {obj.Type} with id: {obj.Id:D2} - Awaiting {obj.DelayInSeconds} seconds");
            await Task.Delay(TimeSpan.FromSeconds(obj.DelayInSeconds), cancellationToken);
            _logger.LogInformation(
                $"Task {obj.Type} with id: {obj.Id:D2} - Finished awaiting {obj.DelayInSeconds} seconds");
        }

        private Task<(Guid, TaskModel)> ReadItem(CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    (Guid, TaskModel) taskModel;
                    while (!_channel.TryPeek(out taskModel))
                    {
                        Task.Delay(500, cancellationToken);
                    }

                    return taskModel;
                }, cancellationToken);
        }

        private void WriteItem(TaskModel item)
        {
            _channel.Enqueue((Guid.NewGuid(), item));
        }

        private bool AckItem(Guid guid)
        {
            (Guid, TaskModel) kvpair;
            if (!_channel.TryPeek(out kvpair)) return true;
            if (kvpair.Item1 != guid)
            {
                return false;
            }
            _channel.Dequeue();
            return true;
        }
    }
}