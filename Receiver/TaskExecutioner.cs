using System;
using System.Threading.Tasks;

namespace RmqTasking
{
    public class TaskExecutioner
    {
        public static async void Execute(TaskModel obj)
        {
            await Task.Run(() => ShowDelay(obj));
        }

        private static async Task ShowDelay(TaskModel obj)
        {
            Console.WriteLine($"Received Task {obj.Id}");
            Console.WriteLine($"Awaiting {obj.DelayInSeconds} seconds for Task {obj.Id}");
            await Task.Delay(TimeSpan.FromSeconds(obj.DelayInSeconds));
            Console.WriteLine($"Finished awaiting {obj.DelayInSeconds} seconds for Task {obj.Id}");
        }
    }
}
