namespace EmailSender.Messaging.Consumer;

using EmailSender.Factory;
using EmailSender.Infra;
using EmailSender.Records;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


public sealed class RabbitMqConsumer : BackgroundService
{
    private readonly RabbitMqFactory _rabbitMqFactory;
    private readonly MailKitSender _mailKitSender;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(
        RabbitMqFactory rabbitMqFactory,
        MailKitSender mailKitSender,
        ILogger<RabbitMqConsumer> logger)
    {
        _rabbitMqFactory = rabbitMqFactory;
        _mailKitSender = mailKitSender;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string queue = "email-sender";
        var connection = await _rabbitMqFactory.GetConnectionAsync(cancellationToken: stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken); 

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 5,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var request = JsonConvert.DeserializeObject<SendEmailRequest>(json);

                if (request is null)
                {
                    _logger.LogWarning("Discarding message because payload is null or invalid JSON.");
                    await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    return;
                }

                await _mailKitSender.SendEmailAsync(request, stoppingToken);
                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message from queue {Queue}.", queue);
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RabbitMQ consumer is stopping.");
        }
    }
}
