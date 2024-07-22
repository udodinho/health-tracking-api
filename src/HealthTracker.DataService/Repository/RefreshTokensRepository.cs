using HealthTracker.DataService.IRepository;
using HealtTracker.DataService.Data;
using HealtTracker.Entities.DbSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthTracker.DataService.Repository
{
    public class RefreshTokensRepository : GenericRepository<RefreshToken>, IRefreshTokensRepository
    {
        public RefreshTokensRepository(AppDbContext dbContext, ILogger logger) : base(dbContext, logger)
        {
        }

        public override async Task<IEnumerable<RefreshToken>> GetAll()
        {
            try
            {
                return await _dbSet.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "{Repo} GetAllRefreshToken method has generated an error", typeof(RefreshTokensRepository));
                return new List<RefreshToken>();
            }
        }

        public async Task<RefreshToken> GetByRefreshToken(string refreshToken)
        {
             try
            {
                return await _dbSet.Where(x => x.Token.ToLower() == refreshToken.ToLower()).AsNoTracking().FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "{Repo} GetByRefreshToken method has generated an error", typeof(RefreshTokensRepository));
                return null;
            }
        }

        public async Task<bool> MarkRefreshTokenAsUsed(RefreshToken refreshToken)
        {
             try
            {
                var token = await _dbSet.Where(x => x.Token.ToLower() == refreshToken.Token.ToLower()).AsNoTracking().FirstOrDefaultAsync();

                if (token == null) return false;

                token.IsUsed = refreshToken.IsUsed;
                return true;
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "{Repo} MarkRefreshTokenAsUsed method has generated an error", typeof(RefreshTokensRepository));
                return false;
            }
        }
    }
}