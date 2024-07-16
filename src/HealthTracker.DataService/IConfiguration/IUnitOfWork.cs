
using HealthTracker.DataService.IRepository;

namespace HealthTracker.DataService.IConfiguration
{
    public interface IUnitOfWork
    {
        // Task<IEnumerable<T>> All();
        IUsersRepository Users{ get; }
        IHealthDataRepository HealthData{ get; }
        IRefreshTokensRepository RefreshTokens{ get; }

        Task CompleteAsync();
    }
}