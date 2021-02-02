using System;
using System.Threading.Tasks;

namespace RmqTasking
{
    class Program
    {
        static void Main(string[] args)
        {
            var rec = new Receiver();
            rec.Start();
        }
    }
}
