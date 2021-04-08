using System;
using System.Collections.Concurrent;
using System.Text;
using BLL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BLL.Infrastructure.RabbitMq
{
    public class Sender
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue;
        private readonly IBasicProperties props;
        private readonly IOptions<RabbitMqConnection> rabbitMqConnection;

        public Sender(IOptions<RabbitMqConnection> rabbitMqConnection)
        {
            this.rabbitMqConnection = rabbitMqConnection;
            var factory = new ConnectionFactory();
            SetConfigurations(factory);

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
            respQueue = new BlockingCollection<string>();

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                    respQueue.TryAdd(Encoding.UTF8.GetString(ea.Body.Span));
            };
        }

        public string Call(string message)
        {
            channel.BasicPublish(
                exchange: "",
                routingKey: "UsersQ",
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(message));

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return "Ok";//respQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }

        private void SetConfigurations(ConnectionFactory connectionFactory)
        {
            connectionFactory.HostName = rabbitMqConnection.Value.HostName;
            connectionFactory.Port = rabbitMqConnection.Value.Port;
            connectionFactory.UserName = rabbitMqConnection.Value.Username;
            connectionFactory.Password = rabbitMqConnection.Value.Password;
        }
    }
}
