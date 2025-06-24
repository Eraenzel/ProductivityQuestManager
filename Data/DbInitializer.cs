using Microsoft.EntityFrameworkCore;

namespace ProductivityQuestManager.Data
{
    public class DbInitializer
    {
        public static void Seed(AppDbContext db)
        {
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
                new TaskModel { Title = "Write CV", DurationMinutes = 30 },
                new TaskModel { Title = "Clean Desk", DurationMinutes = 15 },
                new TaskModel { Title = "Learn Blazor", DurationMinutes = 45 }
            };
            db.Tasks.AddRange(tasks);

            db.SaveChanges();
        }
    }
}
