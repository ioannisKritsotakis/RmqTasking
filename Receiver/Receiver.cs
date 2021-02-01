using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RmqTasking
{
    public class Receiver
    {
        private readonly Channel<TaskModel> _channel;

        public Receiver()
        {
            _channel = Channel.CreateUnbounded<TaskModel>();
        }

        public void Start()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ConsumerOnReceived;
         
            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private void ConsumerOnReceived(object? sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                var obj = JsonConvert.DeserializeObject<TaskModel>(message);
                // Send to appropriate Task. Create if needed.
                _channel.Writer.TryWrite(obj);
            }
            catch (Exception)
            {
                Console.WriteLine(" [x] Received {0}", message);
            }
        }

        

    }

    class TaskModelExecutorIdA
    {
        public string Id;

        private readonly Channel<TaskModel> _channel;

        

        public async Task Consume()
        {
            // Receive the messages with id {Id} and process them
            // When finished close gracefully. CancellationTokenSource
            _channel.Reader.TryRead(out var msg);

        }
    }

}
