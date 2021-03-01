using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Receiver.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Receiver
{
    public interface ITaskDistributor
    {
    }

    public class TaskDistributor : BackgroundService, ITaskDistributor
    {
        private readonly Dictionary<string, TaskExecutioner> _runningTasks = new Dictionary<string, TaskExecutioner>();
        private readonly ILogger<TaskDistributor> _logger;

        private readonly IDistributionChannel _distributionChannel;
        public TaskDistributor(ILogger<TaskDistributor> logger, IDistributionChannel distributionChannel)
        {
            _logger = logger;
            _distributionChannel = distributionChannel;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Started TaskDistributor");
            while (!_distributionChannel.Reader.Completion.IsCompleted && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var msg = await _distributionChannel.Reader.ReadAsync(cancellationToken);
                    var item = JsonConvert.DeserializeObject(msg) as JObject;
                    var model = item?.ToObject<TaskModel>();
                    var heartbeat = item?.ToObject<HeartbeatModel>();

                    if (model != null && !model.IsEmpty())
                    {
                        await ProcessTaskModel(cancellationToken, model);
                    }
                    else if (heartbeat != null && !heartbeat.IsEmpty())
                    {
                       await ProcessHeartbeat(cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Received unknown item");
                    }

                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Operation was cancelled");
                }
                catch (AggregateException ae)
                {
                    foreach (var ex in ae.InnerExceptions)
                    {
                        _logger.LogError(ex, "The task threw and exception");
                    }
                }

            }
        }

        private async Task ProcessTaskModel(CancellationToken cancellationToken, TaskModel taskModel)
        {
            var res = _runningTasks.TryGetValue(taskModel.Type, out var runTask);
            if (!res)
            {
                _logger.LogDebug($"Creating new TaskExecutioner for type {taskModel.Type}");
                runTask = new TaskExecutioner(taskModel.Type, _logger);
                _runningTasks.Add(taskModel.Type, runTask);
                await runTask.FireUpTask(cancellationToken);
            }

            if (runTask.IsProcessDown)
            {
                _logger.LogError($"{runTask.Type} has faulted");
            }

            runTask.SendNewJob(taskModel);
        }

        private async Task ProcessHeartbeat(CancellationToken cancellationToken)
        {
            foreach (var (_, runTask) in _runningTasks)
            {
                if (!runTask.IsProcessDown) continue;
                _logger.LogWarning($"Fixing Type {runTask.Type} TaskExecutioner");
                await runTask.FireUpTask(cancellationToken);
            }
        }
    }
}
