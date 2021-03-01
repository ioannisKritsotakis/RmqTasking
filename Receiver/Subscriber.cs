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

        public Subscriber(ILogger<Subscriber> logger, IDistributionChannel distributionChannel)
        {
            _logger = logger;
            _distributionChannel = distributionChannel;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            _rmqChannel = channel;
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {

            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var firstIteration = true;
            _logger.LogInformation("Initiating RMQ subscription");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Establishing connection to RMQ");
                    if (firstIteration)
                    {
                        _rmqChannel.BasicQos(0, 1000, false);
                        _rmqChannel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
                        firstIteration = false;
                    }

                    using var ch = _rmqChannel;

                    await ChannelConsumeAsync(_rmqChannel, stoppingToken);

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "An unhandled exception occurred");
                    await Task.Delay(TimeSpan.FromMilliseconds(50), stoppingToken);
                }
            }
        }


        private async Task<bool> ChannelConsumeAsync(IModel channel, CancellationToken cancellationToken)
        {

            var channelTaskCompletionSource = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => channelTaskCompletionSource.TrySetResult(true));

            void OnConsumerShutDown(object? sender, ShutdownEventArgs args)
            {
                channelTaskCompletionSource.TrySetException(new Exception(args.ToString()));
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += EventConsumerOnReceived();
            consumer.Shutdown += OnConsumerShutDown;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                channel.BasicQos(0, 1000, false);

                channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);



                return await channelTaskCompletionSource.Task;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "On channel consume async");
                return false;
            }
            finally
            {
                // cancels subscription
                try
                {
                    if (channel.IsOpen)
                    {
                        lock (channel)
                        {
                            channel.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "could close model gracefully");
                }
            }
        }

        private EventHandler<BasicDeliverEventArgs> EventConsumerOnReceived()
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
                    _logger.LogError(" [x] Received {0}", message);
                }
            };
        }

    }
}

