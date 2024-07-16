using System;
using HealthTracker.DataService.IConfiguration;
using HealthTracker.DataService.IRepository;
using HealthTracker.DataService.Repository;
using HealtTracker.DataService.Data;
using Microsoft.Extensions.Logging;

namespace HealthTracker.DataService.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext dbContext;
        private readonly ILogger logger;

        public IUsersRepository Users { get; private set; }

        public IRefreshTokensRepository RefreshTokens {get; private set;}

        public IHealthDataRepository HealthData { get; private set; }

        public UnitOfWork(AppDbContext dbContext, ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            logger = loggerFactory.CreateLogger("db_logs");

            Users = new UsersRepository(dbContext, logger);
            RefreshTokens = new RefreshTokensRepository(dbContext, logger);
            HealthData = new HealthDataRepository(dbContext, logger);
        } 

        public async Task CompleteAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
           dbContext.Dispose();
        }
    }
}