using System;
using System.Text;
using System.Text.Json;
using DAL.Contexts;
using DAL.Models;
using DAL.Models.Entities;
using DAL.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsoleConsumer
{
    public class Program
    {

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello I'm console!");
                ConsumData();
                Console.WriteLine("Success finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ConsoleConsumer run error");
            }
        }

        private static void ConsumData()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "(AMQP default)", type: ExchangeType.Direct);

                var queueName = "UsersQ";//channel.QueueDeclare().QueueName;//
                channel.QueueBind(queue: queueName,
                                  exchange: "(AMQP default)",
                                  routingKey: "");

                Console.WriteLine(" [*] Waiting for create user requests.");

                var consumer = new EventingBasicConsumer(channel);
                GetDataAndCreateUser(consumer);
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static void GetDataAndCreateUser(EventingBasicConsumer consumer)
        {
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] {0}", message);

                var user = JsonSerializer.Deserialize<User>(message);
                user.CreatedDate = DateTime.UtcNow.AddHours(2);

                string connectionString = "mongodb://localhost:27017";
                MongoClient client = new MongoClient(connectionString);
                IMongoDatabase database = client.GetDatabase("MongoDbTest");

                var Users = database.GetCollection<User>("UsersRabbit");
                Users.InsertOneAsync(user).Wait();
                Console.WriteLine($"User created: {user.UserId}");
            };
        }
    }
}
