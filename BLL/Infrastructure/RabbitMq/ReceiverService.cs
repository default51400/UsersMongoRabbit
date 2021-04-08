using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BLL.Models;
using DAL.Models.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BLL.Infrastructure.RabbitMq
{
    public class ReceiverService : BackgroundService
    {
        private readonly ILogger logger;
        private IConnection connection;
        private IModel channel;
        private IUsersRepository db;
        private ConnectionFactory factory;
        private readonly RabbitMqConnection rabbitMqConnection;

        public ReceiverService(RabbitMqConnection rabbitMqConnection, IUsersRepository usersRepository, ILoggerFactory loggerFactory)
        {
            this.rabbitMqConnection = rabbitMqConnection;
            db = usersRepository;
            logger = loggerFactory.CreateLogger(typeof(ReceiverService));
            factory = new ConnectionFactory();
            SetConfigurations(factory);
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare("UsersQ", false, false, false, null);
            channel.BasicQos(0, 1, false);
            connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (ch, ea) =>
            {
                string response = null;

                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                try
                {
                    response = await CreateResponse(ea);
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    response = e.Message + "\n" + e.StackTrace;
                }
                finally
                {
                    PublishResponse(ea, channel, response, replyProps);
                }
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            channel.BasicConsume("UsersQ", false, consumer);
            return Task.CompletedTask;
        }

        private async Task<string> CreateResponse(BasicDeliverEventArgs ea)
        {
            var takedMessage = Encoding.UTF8.GetString(ea.Body.Span);
            logger.LogInformation("takedMessage:" + takedMessage);
            var user = JsonSerializer.Deserialize<User>(takedMessage);

            await db.Create(user);

            return JsonSerializer.Serialize(user);
        }

        private static void PublishResponse(BasicDeliverEventArgs ea, IModel channel, string response, IBasicProperties replyProps)
        {
            channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo,
              basicProperties: replyProps, body: Encoding.UTF8.GetBytes(response));
            channel.BasicAck(deliveryTag: ea.DeliveryTag,
              multiple: false);
        }

        private void SetConfigurations(ConnectionFactory connectionFactory)
        {
            connectionFactory.HostName = rabbitMqConnection.HostName;
            connectionFactory.Port = rabbitMqConnection.Port;
            connectionFactory.UserName = rabbitMqConnection.Username;
            connectionFactory.Password = rabbitMqConnection.Password;
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            channel.Close();
            connection.Close();
            base.Dispose();
        }
    }
}
