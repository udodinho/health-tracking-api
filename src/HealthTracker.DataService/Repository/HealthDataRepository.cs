using HealthTracker.DataService.IRepository;
using HealthTracker.Entities.DbSet;
using HealtTracker.DataService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthTracker.DataService.Repository
{
    public class HealthDataRepository : GenericRepository<HealthData>, IHealthDataRepository
    {
        public HealthDataRepository(AppDbContext dbContext, ILogger logger) : base(dbContext, logger)
        {
        }

        public override async Task<IEnumerable<HealthData>> GetAll()
        {
            try
            {
                return await _dbSet.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "{Repo} GetAllHealthData method has generated an error", typeof(HealthDataRepository));
                return new List<HealthData>();
            }
        }

        public async Task<bool> UpdateHealthData(HealthData healthData)
        {
            try
            {
                var existingHealthData = await _dbSet.Where(x => x.Status == 1 && x.Id == healthData.Id).FirstOrDefaultAsync();

                if (existingHealthData == null) return false;
                
                existingHealthData.Height = healthData.Height;
                existingHealthData.Weight = healthData.Weight;
                existingHealthData.BloodType = healthData.BloodType;
                existingHealthData.Race = healthData.Race;
                existingHealthData.UseGlasses = healthData.UseGlasses;
                existingHealthData.UpdateDate = DateTime.UtcNow; 

                return true;

            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "{Repo} UpdateHealthData method has generated an error", typeof(HealthDataRepository));
                return false;
            }
        }
    }
}