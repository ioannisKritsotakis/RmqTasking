using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Receiver.Models;

namespace Receiver
{
    public class Subscriber : BackgroundService
    {
        private readonly ILogger<Subscriber> _logger;
        private readonly IDistributionChannel _distributionChannel;
        private IModel _rmqChannel;
        private IConnection _connection;

        public Subscriber(ILogger<Subscriber> logger, IDistributionChannel distributionChannel)
        {
            _logger = logger;
            _distributionChannel = distributionChannel;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            _connection = factory.CreateConnection();
            _rmqChannel = _connection.CreateModel();
            _rmqChannel.BasicQos(0, 1000, false);
            _rmqChannel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false,
                arguments: null);
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;  

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_rmqChannel);
            consumer.Received += ConsumerOnReceived();
            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;
            
            _rmqChannel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
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
                    _distributionChannel.WriteTaskModelToChannel(obj);
                }
                catch (Exception)
                {
                    _logger.LogError(" [x] Received {message}");
                }
            };
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
        }
    }
}