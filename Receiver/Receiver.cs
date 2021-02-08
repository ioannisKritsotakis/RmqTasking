using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Receiver
{
    public interface IReceiver
    {
    }

    public class Receiver : BackgroundService, IReceiver
    {
        private readonly ILogger<Receiver> _logger;
        private readonly IDistributionChannel _distributionChannel;
        public Receiver(IDistributionChannel distributionChannel, ILogger<Receiver> logger)
        {
            _distributionChannel = distributionChannel;
            _logger = logger;
        }

        public void MainBody()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            _logger.LogInformation(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ConsumerOnReceived();

            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
            Console.ReadLine();
        }

        private EventHandler<BasicDeliverEventArgs> ConsumerOnReceived()
        {
            return (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    var obj = JsonConvert.DeserializeObject<TaskModel>(message);
                    _distributionChannel.WriteToChannel(obj);
                }
                catch (Exception)
                {
                    _logger.LogError(" [x] Received {0}", message);
                }
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(MainBody, stoppingToken);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting Receiver");
            return base.StartAsync(cancellationToken);
        }
    }
}
