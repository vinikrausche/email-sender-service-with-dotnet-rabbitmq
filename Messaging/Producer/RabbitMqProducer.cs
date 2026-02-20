using EmailSender.Factory;
using RabbitMQ.Client;
using System.Text;

namespace EmailSender.Messaging.Producer;

public sealed class RabbitMqProducer
{
    private readonly RabbitMqFactory _rabbitMqFactory;

    public RabbitMqProducer(RabbitMqFactory rabbitMqFactory)
    {
        _rabbitMqFactory = rabbitMqFactory;
    }

    public async Task PublishAsync(string queue, string message, CancellationToken cancellationToken = default)
    {
        var connection = await _rabbitMqFactory.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queue,
            body: body,
            cancellationToken: cancellationToken);
    }
}
