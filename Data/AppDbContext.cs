using Microsoft.EntityFrameworkCore;

namespace ProductivityQuestManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<QuestResult> QuestResults { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
