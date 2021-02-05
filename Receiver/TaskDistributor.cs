using RmqTasking;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Receiver
{
    public class TaskDistributor
    {
        private Dictionary<string, TaskExecutioner> _RunningTasks = new Dictionary<string, TaskExecutioner>();
        private Channel<TaskExecutioner> _channel;
        public TaskDistributor()
        {
        }

        // "async void" must go
        // consume must run only once per execution
        public async void Distribute(TaskModel item, CancellationToken cancellationToken)
        {
            var res = _RunningTasks.TryGetValue(item.Id, out var runTask);
            if (!res)
            {
                runTask = new TaskExecutioner(item.Id);
                _RunningTasks.Add(item.Id, runTask);
            }

            runTask.Enqueue(item);
            await runTask.Consume(cancellationToken);


        }
    }
}
