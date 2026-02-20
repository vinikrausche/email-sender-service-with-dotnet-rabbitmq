using System.Reflection;
using EmailSender.Factory;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace EmailSender.Tests.Factory;

public class RabbitMqFactoryConfigurationTests
{
    [Fact]
    public void Should_ReadRabbitMqSettings_FromConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["RABBITMQ_HOST"] = "rabbitmq.local",
            ["RABBITMQ_PORT"] = "5679",
            ["RABBITMQ_USER"] = "app-user",
            ["RABBITMQ_PASS"] = "app-pass"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var factory = new RabbitMqFactory(configuration);
        var connectionFactory = GetConnectionFactory(factory);

        Assert.Equal("rabbitmq.local", connectionFactory.HostName);
        Assert.Equal(5679, connectionFactory.Port);
        Assert.Equal("app-user", connectionFactory.UserName);
        Assert.Equal("app-pass", connectionFactory.Password);
    }

    [Fact]
    public void Should_UseDefaults_WhenConfigurationIsMissingOrInvalid()
    {
        var settings = new Dictionary<string, string?>
        {
            ["RABBITMQ_PORT"] = "invalid"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var factory = new RabbitMqFactory(configuration);
        var connectionFactory = GetConnectionFactory(factory);

        Assert.Equal("localhost", connectionFactory.HostName);
        Assert.Equal(5672, connectionFactory.Port);
        Assert.Equal("guest", connectionFactory.UserName);
        Assert.Equal("guest", connectionFactory.Password);
    }

    private static ConnectionFactory GetConnectionFactory(RabbitMqFactory factory)
    {
        var field = typeof(RabbitMqFactory).GetField("_connectionFactory", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        var value = field.GetValue(factory);
        return Assert.IsType<ConnectionFactory>(value);
    }
}
