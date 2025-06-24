using Microsoft.EntityFrameworkCore;

namespace ProductivityQuestManager.Data
{
    public class DbInitializer
    {
        public static void Seed(AppDbContext db, bool forceReset = false)
        {
            if (forceReset)
            {
                db.QuestResults.RemoveRange(db.QuestResults);
                db.Quests.RemoveRange(db.Quests);
                db.Tasks.RemoveRange(db.Tasks);
                db.Units.RemoveRange(db.Units);
                db.SaveChanges();
            }

            // Only seed if nothing exists yet
            if (db.Units.Any() || db.Tasks.Any()) return;

            // Add Units
            var units = new List<Unit>
            {
                new Unit { Name = "Aria", Class = "Warrior", Attack = 12, Defense = 8 },
                new Unit { Name = "Lyn", Class = "Mage", Attack = 15, Defense = 3 },
                new Unit { Name = "Tor", Class = "Rogue", Attack = 10, Defense = 5, Speed = 8 }
            };
            db.Units.AddRange(units);

            // Add Tasks
            var tasks = new List<TaskModel>
            {
                new TaskModel { Title = "Write CV", DurationMinutes = 30, IsRepeatable = false, IsCompleted = false },
                new TaskModel { Title = "Clean Desk", DurationMinutes = 15, IsRepeatable = false, IsCompleted = false  },
                new TaskModel { Title = "Learn Blazor", DurationMinutes = 45, IsRepeatable = false, IsCompleted = false },
                new TaskModel { Title = "Daily Planning", DurationMinutes = 5, IsRepeatable = true, CooldownMinutes = 15 },
                new TaskModel { Title = "Stretching", DurationMinutes = 2, IsRepeatable = true, CooldownMinutes = 10 }
            };
            db.Tasks.AddRange(tasks);

            db.SaveChanges();
        }
    }
}
