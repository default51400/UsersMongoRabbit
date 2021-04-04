using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
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
        private IConfiguration configuration;

        public Sender(IConfiguration configuration)
        {
            this.configuration = configuration;
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

            return respQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }

        private void SetConfigurations(ConnectionFactory connectionFactory)
        {
            connectionFactory.HostName = configuration.GetSection("RabbitMqConnection:HostName").Value;
            connectionFactory.Port = Int32.TryParse(configuration.GetSection("RabbitMqConnection:Port").Value,
                out int port) ? port : 5672;
            connectionFactory.UserName = configuration.GetSection("RabbitMqConnection:Username")?.Value;
            connectionFactory.Password = configuration.GetSection("RabbitMqConnection:Password")?.Value;
        }
    }
}
