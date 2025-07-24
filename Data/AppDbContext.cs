using Microsoft.EntityFrameworkCore;

namespace ProductivityQuestManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<QuestResult> QuestResults { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new { tt.TaskModelId, tt.TagId });

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.TaskModel)
                .WithMany(tm => tm.TaskTags)
                .HasForeignKey(tmt => tmt.TaskModelId);

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TagId);

            modelBuilder.Entity<QuestResult>()
                  .HasOne(q => q.TaskModel)
                  .WithMany(t => t.QuestResults)
                  .HasForeignKey(q => q.TaskModelId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
