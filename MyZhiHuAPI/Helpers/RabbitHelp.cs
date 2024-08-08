using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyZhiHuAPI.Helpers;

public class RabbitMqHelper(IConfiguration config)
{
    private readonly ConnectionFactory _connectionFactory = new()
    {
        HostName = config["RabbitMq:Host"]!,
        Port = Convert.ToInt32(config["RabbitMq:Port"]),
        UserName = config["RabbitMq:Username"]!,
        Password = config["RabbitMq:Password"]!,
        VirtualHost = config["RabbitMq:VirtualHost"]!
    };

    private IConnection Connection => _connectionFactory.CreateConnection();

    private IModel Channel => Connection.CreateModel();

    public void Publish<T>(T message) where T : class
    {
        var channelName = typeof(T).Name;
        Channel.ExchangeDeclare(exchange: channelName, type: "fanout", durable: false, autoDelete: false, null);

        var msgContent = JsonConvert.SerializeObject(message);
        var msgBytes = Encoding.UTF8.GetBytes(msgContent);
        Channel.BasicPublish(exchange: channelName, routingKey: string.Empty, body: msgBytes, mandatory: false, basicProperties: null);
    }

    public void Publish(string message, string channelName)
    {
        Channel.ExchangeDeclare(exchange: channelName, type: "fanout", durable: false, autoDelete: false, null);
        var msgBytes = Encoding.UTF8.GetBytes(message);
        Channel.BasicPublish(exchange: channelName, routingKey: string.Empty, body: msgBytes, mandatory: false, basicProperties: null);
    }

    public Task<T> PublishAsync<T>(T message) where T : class
    {
        var channelName = typeof(T).Name;
        Channel.ExchangeDeclare(exchange: channelName, type: "fanout", durable: false, autoDelete: false, null);

        var msgContent = JsonConvert.SerializeObject(message);
        var msgBytes = Encoding.UTF8.GetBytes(msgContent);
        Channel.BasicPublish(exchange: channelName, routingKey: string.Empty, body: msgBytes, mandatory: false, basicProperties: null);
        return Task.FromResult(message);
    }

    public void Subscribe(string channelName, Action<string> callback)
    {
        Channel.ExchangeDeclare(exchange: channelName, type: "fanout");
        var queueName = channelName + "_" + Guid.NewGuid().ToString().Replace("-", "");
        Channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        Channel.QueueBind(queue: queueName, exchange: channelName, routingKey: string.Empty);
        Channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        var consumer = new EventingBasicConsumer(Channel);
        consumer.Received += (_, args) => {
            var msg = Encoding.UTF8.GetString(args.Body.ToArray());
            callback(msg);
            Channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: true);
        };
        Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }
}
