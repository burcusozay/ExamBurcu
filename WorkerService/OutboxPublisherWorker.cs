using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net.Http.Json;
using VaccineApp.OutboxPublisher.Options;
using WorkerService;

// Bu servis veritabanýnda 5 kez baþarýsýz kayýt olmasý halinde api url e bilgilendirme gönderir. Notification iþlemi yapar
public class OutboxPublisherWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IDistributedCache _distributedCache; 
    private readonly ServiceAccountOptions _serviceAccount;

    // YAPILAN DEÐÝÞÝKLÝK: Constructor temizlendi.
    // Artýk sadece gerçekten kullanýlan servisler enjekte ediliyor.
    public OutboxPublisherWorker(
        IHttpClientFactory httpClientFactory,
        IConnectionMultiplexer redis,
        ILogger<OutboxPublisherWorker> logger,
        IDistributedCache distributedCache,
        IOptions<ServiceAccountOptions> serviceAccount)
    {
        _httpClientFactory = httpClientFactory;
        _redis = redis;
        _logger = logger;
        _distributedCache = distributedCache;
        _serviceAccount = serviceAccount.Value; 
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var locker = new RedisDistributedSynchronizationProvider(_redis.GetDatabase());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Her döngüde güncel ve geçerli bir token al
                //var token = await GetBearerTokenAsync(stoppingToken);
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var messages = await client.GetFromJsonAsync<List<HsysNotificationPayload>>($"{_serviceAccount.ApiCallEndpoint}/UnprocessedList", cancellationToken: stoppingToken);

                if (messages == null || !messages.Any())
                {
                    await Task.Delay(2000, stoppingToken);
                    continue;
                }

                foreach (var msg in messages)
                {
                    var lockKey = $"outbox-msg-lock:{msg.VaccineApplicationId}";
                    await using (var handle = await locker.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), stoppingToken))
                    {
                        if (handle == null) continue;

                        try
                        {
                            _logger.LogInformation($"Mesaj {msg.VaccineApplicationId} iþleniyor.");
                            var markResponse = await client.PostAsync($"{_serviceAccount.ApiCallEndpoint}/MarkProcessed/{msg.VaccineApplicationId}", null, stoppingToken);
                            markResponse.EnsureSuccessStatusCode();
                            _logger.LogInformation($"Mesaj {msg.VaccineApplicationId} baþarýyla iþlendi.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Outbox mesajý iþlenirken hata: {msg.VaccineApplicationId}");
                            
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError(httpEx, "Yetkilendirme hatasý (401 Unauthorized) alýndý. Token geçersiz veya süresi dolmuþ olabilir. Cache temizleniyor...");
                // Token geçersizse cache'i temizle ki bir sonraki denemede yeni token alýnsýn.
                await _distributedCache.RemoveAsync("outbox_bearer_token", stoppingToken);
                await Task.Delay(1000, stoppingToken); // Kýsa bir süre bekle
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "modülde genel bir hata oluþtu.");
                
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
     
}
