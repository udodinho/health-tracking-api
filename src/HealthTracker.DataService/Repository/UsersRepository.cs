using HealthTracker.DataService.IRepository;
using HealtTracker.DataService.Data;
using HealtTracker.Entities.DbSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthTracker.DataService.Repository
{
    public class UsersRepository : GenericRepository<User>, IUsersRepository
    {
        public UsersRepository(AppDbContext dbContext, ILogger logger) : base(dbContext, logger)
        {
        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            try
            {
                return await _dbSet.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "{Repo} GetAllUsers method has generated an error", typeof(UsersRepository));
                return new List<User>();
            }
        }

        public async Task<bool> UpdateUserProfile(User user)
        {
            try
            {
                var existingUser = await _dbSet.Where(x => x.Status == 1 && x.Id == user.Id).FirstOrDefaultAsync();

                if (existingUser == null) return false;
                
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Address = user.Address;
                existingUser.MobileNumber = user.MobileNumber;
                existingUser.UpdateDate = DateTime.UtcNow;
                existingUser.Gender = user.Gender;
                existingUser.Phone = user.Phone;

                return true;

            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "{Repo} UpdateUserProfile method has generated an error", typeof(UsersRepository));
                return false;
            }
        }

        public async Task<User> GetByIdentityId(Guid identityId)
        {
            try
            {
                return await _dbSet.Where(x => x.Status == 1 && x.IdentityId == identityId).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "{Repo} GetByIdentityId method has generated an error", typeof(UsersRepository));
                return null;
            }
        }
    }
}