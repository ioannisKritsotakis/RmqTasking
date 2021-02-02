using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Receiver;

namespace RmqTasking
{
    public class Receiver
    {
        private readonly CancellationTokenSource _tokenSource;
        public Receiver()
        {
            _tokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += ConsoleOnCancelKeyPress();

        }

        private ConsoleCancelEventHandler ConsoleOnCancelKeyPress()
        {
            return delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                _tokenSource.Cancel();
            };
        }

        public void Start()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += consumerOnReceived();

            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
            _tokenSource.Cancel();
        }

        private EventHandler<BasicDeliverEventArgs> consumerOnReceived()
        {
            return (model, ea) =>
            {
                var task = new TaskDistributor();
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    var obj = JsonConvert.DeserializeObject<TaskModel>(message);
                    // Send to appropriate Task. Create if needed.
                    task.Distribute(obj, _tokenSource.Token);
                }
                catch (Exception)
                {
                    Console.WriteLine(" [x] Received {0}", message);
                }
            };
        }
    }
}
