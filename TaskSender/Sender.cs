using Newtonsoft.Json;
using RabbitMQ.Client;
using Receiver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TaskSender
{
    public class Sender
    {

        public static void Start()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.ExchangeDeclare("johny", "direct", false, false);
                channel.QueueBind("hello", "johny", "");
                
                foreach (var i in Enumerable.Range(0, 2))
                {
                    foreach (var msg in Data())
                    {
                        msg.Id = (i * 10) + msg.Id;
                        var message = JsonConvert.SerializeObject(msg);
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "johny", routingKey: "", basicProperties: null, body: body);
                        Console.WriteLine(" [x] Sent {0}", message);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        private static IEnumerable<TaskModel> Data()
        {
            return new List<TaskModel>()
            {
                new TaskModel("A", 2, 1),
                new TaskModel("B", 5, 2),
                new TaskModel("C", 3, 3),
                new TaskModel("B", 5, 4),
                new TaskModel("D", 2, 5),
                new TaskModel("A", 3, 6),
                new TaskModel("C", 7, 8),
                new TaskModel("A", 8, 9),
                new TaskModel("D", 4, 10),
            };
        }
    }
}
