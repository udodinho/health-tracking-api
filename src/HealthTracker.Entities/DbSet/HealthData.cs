using HealtTracker.Entities.DbSet;

namespace HealthTracker.Entities.DbSet
{
   public class HealthData : BaseEntity
    {
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public string BloodType { get; set; }
        public string Race { get; set; }
        public bool UseGlasses { get; set; }
    }
}