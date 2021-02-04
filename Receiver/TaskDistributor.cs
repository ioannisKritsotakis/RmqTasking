using RmqTasking;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;

namespace Receiver
{
    public class TaskDistributor
    {
        private Dictionary<string, TaskExecutioner> _RunningTasks = new Dictionary<string, TaskExecutioner>();
        
        public TaskDistributor()
        {
        }

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
