using ExamBurcu.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using VaccineExam.Core;
using VaccineExam.Repository;

namespace VaccineExam.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly IDistributedCache _cache; 
        private readonly Dictionary<string, object> _repositories = new();
          
        public UnitOfWork(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache; 
        } 

        public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            var typeName = typeof(TEntity).Name;

            if (_repositories.ContainsKey(typeName))
                return (IRepository<TEntity, TKey>)_repositories[typeName];

            var repositoryInstance = new Repository<TEntity, TKey>(_context);
            _repositories[typeName] = repositoryInstance;

            return repositoryInstance;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
        {
            if (_transaction is not null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction is null) return;

            try
            {
                // 1. Veritabanındaki değişiklikleri kaydet (Commit işlemine dahil)
                await _context.SaveChangesAsync();

                // 2. Redis güncelleme (Transaction'ı kullanmayan/kapatmayan işlemler)
                // Eğer bu işlem başarısız olursa, Commit çağrılmaz ve Rollback'e düşülür.
                await SyncRedisCacheAsync();

                // 3. İşlemi veritabanı tarafında tamamla (NpgsqlTransaction'ı kapatan asıl çağrı)
                await _transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // 4. Hata durumunda işlemi geri al ve logla (Filter zaten Rollback'i yakalıyor olabilir, 
                // ancak güvende olmak için Rollback'i çağırın.)
                await RollbackAsync(); // Eğer işlem hala açıksa geri alınır.
                throw; // Hatayı yukarı fırlat
            }
            finally
            {
                // 5. İşlem nesnesini KESİNLİKLE en sonda serbest bırakın ve null yapın
                if (_transaction is not null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

        private async Task SyncRedisCacheAsync()
        {
            // Örnek: tüm VaccineSchedule'ları cache’e at
            var entityList = await _context.vaccineschedules.ToListAsync();
            foreach (var entity in entityList)
            {
                var key = $"stock_{entity.id}";
                var json = JsonSerializer.Serialize(entity);
                await _cache.SetStringAsync(key, json);
            }
        }
    }
}