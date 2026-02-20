namespace EmailSender.Messaging.Consumer;

using EmailSender.Factory;


public sealed class RabbitMqConsumer : BackgroundService
{
    
     private readonly RabbitMqFactory _rabbitMqFactory;

    public RabbitMqConsumer(RabbitMqFactory rabbitMqFactory)
    {
        _rabbitMqFactory = rabbitMqFactory;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {  
        string queue = "email-sender";
        var _connection = await _rabbitMqFactory.GetConnectionAsync(cancellationToken: stoppingToken);
        var _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken); 


            _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 5,
                global: false,
                cancellationToken: stoppingToken);

        return;
    }
}