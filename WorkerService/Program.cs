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
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}