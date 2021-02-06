using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Receiver
{
    public interface ITaskDistributor
    {
    }

    public class TaskDistributor : IHostedService, ITaskDistributor
    {
        private readonly Dictionary<string, TaskExecutioner> _runningTasks = new Dictionary<string, TaskExecutioner>();
        private readonly ILogger<TaskDistributor> _logger;

        private readonly IDistributionChannel _distributionChannel;

        public TaskDistributor(ILogger<TaskDistributor> logger, IDistributionChannel distributionChannel)
        {
            _logger = logger;
            _distributionChannel = distributionChannel;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started TaskDistributor");
            while (!_distributionChannel.Reader.Completion.IsCompleted || !cancellationToken.IsCancellationRequested)
            {
                var item = await _distributionChannel.Reader.ReadAsync(cancellationToken);
                _logger.LogInformation($"Received {item.Id}");
                var res = _runningTasks.TryGetValue(item.Id, out var runTask);
                if (!res)
                {
                    _logger.LogInformation("Creating new TaskExecutioner");
                    runTask = new TaskExecutioner(item.Id, _logger);
                    _runningTasks.Add(item.Id, runTask);
                    await runTask.Consume(cancellationToken);
                }

                runTask.SendJob(item);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
