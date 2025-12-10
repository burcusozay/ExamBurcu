using StackExchange.Redis;
using VaccineApp.OutboxPublisher.Options;
using WorkerService;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHttpClient();
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            // HostApplicationBuilder yapýsý Configuration'a doðrudan builder.Configuration üzerinden eriþim saðlar
            options.Configuration = builder.Configuration["Redis:ConnectionString"];
            options.InstanceName = builder.Configuration["Redis:InstanceName"];
        });

        builder.Services.Configure<ServiceAccountOptions>(builder.Configuration.GetSection("ServiceAccount"));

        builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer => ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));


        builder.Services.AddHostedService<Worker>();
        builder.Services.AddHostedService<OutboxPublisherWorker>();

        var host = builder.Build();
        host.Run();
    }
}