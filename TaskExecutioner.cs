using System;
using System.Threading.Tasks;

namespace RmqTasking
{
    public class TaskExecutioner
    {
        public static async void Execute(TaskModel obj)
        {
            Console.WriteLine($"{obj.Id}");
            await Task.Delay(TimeSpan.FromSeconds(obj.DelayInSeconds));
            Console.WriteLine($"Finished awaiting {obj.DelayInSeconds} seconds for Task {obj.Id}");
        }
    }
}
