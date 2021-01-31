using Newtonsoft.Json;
using RabbitMQ.Client;
using RmqTasking;
using System;
using System.Collections.Generic;
using System.Text;

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

                foreach (var msg in Data())
                {
                    var message = JsonConvert.SerializeObject(msg);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "johny", routingKey: "", basicProperties: null, body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }

            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static List<TaskModel> Data()
        {
            return new List<TaskModel>()
            {
                new TaskModel("A", 5),
                new TaskModel("B", 15),
                new TaskModel("C", 10),
            };
        }
    }
}
