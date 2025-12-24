using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Push.Messaging.Consumer;
using Push.Messaging.Infrastructure.Options;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqOptions>(
            context.Configuration.GetSection("RabbitMq"));

        services.Configure<FirebaseOptions>(
            context.Configuration.GetSection("Firebase"));

        services.AddSingleton<IConnectionFactory>(_ =>
        {
            var options = context.Configuration
                .GetSection("RabbitMq")
                .Get<RabbitMqOptions>();

            return new ConnectionFactory
            {
                HostName = options!.Host,
            };
        });

        services.AddHttpClient<FirebasePushHandler>();

        services.AddHostedService<RabbitMqListener>();
    })
    .Build();

await host.RunAsync();