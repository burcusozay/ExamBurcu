using Microsoft.Extensions.Caching.Distributed;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        public static class QueueConstants
        {
            public const string HsysTaskPrefix = "HSYS_NOTIFY_TASK:";
            public const string HsysTaskSetKey = "HSYS_PENDING_KEYS"; // Tüm bekleyen görev anahtarlarýný tutan Set
        }

        private const int MaxRetries = 5;
        private readonly ILogger<Worker> _logger;
        private readonly IDistributedCache _cache; // Redis'e eriþim
        private readonly IHttpClientFactory _httpClientFactory; // HTTP istekleri için
        public Worker(ILogger<Worker> logger, IDistributedCache cache, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HSYS Bildirim Worker'ý Baþladý.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Worker her 30 saniyede bir kuyruðu kontrol edecek.
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                _logger.LogInformation("HSYS Worker: Kuyruk kontrol ediliyor. Zaman: {time}", DateTimeOffset.Now);

                // **UYARI:** IDistributedCache (StackExchange.Redis) ile tüm anahtarlarý listelemek mümkün deðildir.
                // Bu kodun çalýþmasý için, tüm görev anahtarlarýný tutan ayrý bir Redis listesi (SET veya LIST)
                // kullanýldýðýný veya bu anahtarlarýn bir þekilde elde edildiðini varsayýyoruz.

                // Eðer Redis'i gerçek bir LIST olarak kullandýysanýz (LPUSH/RPOP), bu kýsým deðiþir.
                // Bu örnekte, þu anki Redis'e atýlmýþ GÖREV ID'lerini içeren bir SET olduðunu varsayalým:

                // --- GÖREV ÇEKME SÝSTEMÝ (Simülasyon) ---

                // 1. Ýþlenecek Anahtarlar Listesini Al (Bu kýsým Redis'in kullaným þekline göre deðiþir)
                var taskKeys = await GetPendingTaskKeysFromRedisAsync(); // Bu, Redis'ten bekleyen tüm anahtarlarý çekmeli

                foreach (var key in taskKeys)
                {
                    var message = await _cache.GetStringAsync(key);

                    if (string.IsNullOrEmpty(message))
                    {
                        // Mesaj yoksa (baþka bir worker almýþ veya süresi dolmuþsa) devam et
                        continue;
                    }

                    HsysNotificationPayload payload;
                    try
                    {
                        payload = JsonSerializer.Deserialize<HsysNotificationPayload>(message);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Payload JSON format hatasý: {key}", key);
                        // Hatalý payload'ý sil ve devam et
                        await _cache.RemoveAsync(key);
                        continue;
                    }

                    // 2. Görevi Ýþle
                    var success = await NotifyHsysAsync(payload, key);

                    if (success)
                    {
                        // 3. Baþarýlý ise Kuyruktan Çýkar
                        await _cache.RemoveAsync(key);
                        _logger.LogInformation($"HSYS Bildirimi Baþarýlý ve silindi: App ID {payload.VaccineApplicationId}");
                    }
                    // Baþarýsýz ise (success == false), deneme sayacý artýrýlmýþ payload zaten 
                    // kuyruða geri yazýldýðý için burada ek bir iþlem yapmaya gerek yoktur.
                }
            }
        }

        // Not: Gerçekte tüm anahtarlarý almak zor olduðu için, bu bir yer tutucudur.
        private Task<List<string>> GetPendingTaskKeysFromRedisAsync()
        {
            // Gerçek bir Redis yapýsýnda (SET, sorted set veya stream kullanýlýyorsa) bu fonksiyon
            // o yapýdan bekleyen görevlerin anahtarlarýný çekmelidir.
            // IDistributedCache (StackExchange.Redis) ile tüm anahtarlarý listelemek mümkün deðildir.
            // Bu yüzden, sadece deneme amaçlý sabit bir key listesi döndürüyoruz.
            return Task.FromResult(new List<string>());
        }

        // 2. HSYS Servisini Çaðýran Metot
        private async Task<bool> NotifyHsysAsync(HsysNotificationPayload payload, string key)
        {
            // 1. HttpClient Factory ile Client Oluþturma
            var client = _httpClientFactory.CreateClient();

            // 2. HSYS Payload'unu Hazýrlama
            var hsysData = new
            {
                AppId = payload.VaccineApplicationId,
                Tckn = payload.ChildId,
                VaccineCode = payload.VaccineId,
                // ...
            };

            // 3. JSON Serileþtirme
            var jsonContent = JsonSerializer.Serialize(hsysData);

            // 4. StringContent Oluþturma (content deðiþkeni burada tanýmlanýyor)
            // Eðer Encoding.UTF8 kullanýlýyorsa System.Text using'i gereklidir.
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://hsys.saglik.gov.tr/NotifyVaccine", content);

                if (response.IsSuccessStatusCode)
                {
                    return true; // Baþarýlý
                }
                else
                {
                    // HTTP Hata Kodu (4xx, 5xx) alýnýrsa
                    return await HandleRetry(payload, key, $"HTTP Hata Kodu: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Að veya baðlantý hatasý oluþursa
                return await HandleRetry(payload, key, $"Að/Baðlantý Hatasý: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Genel istisna
                return await HandleRetry(payload, key, $"Genel Ýstisna: {ex.Message}");
            }
        }

        private async Task<bool> HandleRetry(HsysNotificationPayload payload, string key, string errorMessage)
        {
            payload.RetryCount++;
            _logger.LogWarning($"HSYS Bildirimi Baþarýsýz. App ID {payload.VaccineApplicationId}. Deneme Sayýsý: {payload.RetryCount}. Hata: {errorMessage}");

            if (payload.RetryCount < MaxRetries)
            {
                // Tekrar deneme hakký varsa, mesajý güncelleyip kuyruða geri gönder
                var updatedMessage = JsonSerializer.Serialize(payload);
                await _cache.SetStringAsync(key, updatedMessage, new DistributedCacheEntryOptions
                {
                    // Baþarýsýz görevlerin biraz beklemesi için zaman aþýmý ekleyin (Backoff)
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Math.Pow(2, payload.RetryCount))
                });
                return false; // Baþarýsýz, tekrar denenecek.
            }
            else
            {
                // Maksimum deneme sayýsýna ulaþýldý. Görevi silmeden logla (Manuel müdahale gerektirir)
                _logger.LogError($"HSYS Bildirimi ÝPTAL EDÝLDÝ: App ID {payload.VaccineApplicationId}. Maksimum {MaxRetries} denemeye ulaþýldý.");

                // Gerçek uygulamalarda: Buradan Dead Letter Queue (Ölü Mektup Kuyruðu)'na gönderilir.
                return false; // Baþarýsýz, bir daha denenmeyecek.
            }
        }
    }
}
