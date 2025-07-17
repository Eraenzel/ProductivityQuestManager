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
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TimeEntryTag> TimeEntryTags { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TimeEntryTag>()
                .HasKey(tet => new { tet.TimeEntryId, tet.TagId });

            modelBuilder.Entity<TimeEntryTag>()
                .HasOne(tet => tet.TimeEntry)
                .WithMany(te => te.TimeEntryTags)
                .HasForeignKey(tet => tet.TimeEntryId);

            modelBuilder.Entity<TimeEntryTag>()
                .HasOne(tet => tet.Tag)
                .WithMany(t => t.TimeEntryTags)
                .HasForeignKey(tet => tet.TagId);
        }
    }
}
