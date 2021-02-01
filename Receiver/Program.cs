using System.Threading.Tasks;

namespace RmqTasking
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rec = new Receiver();
            rec.Start();
        }
    }
}
