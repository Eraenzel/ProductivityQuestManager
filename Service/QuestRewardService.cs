using System;
using ProductivityQuestManager.Data;

namespace ProductivityQuestManager.Service
{
    /// <summary>
    /// Encapsulates XP, loot generation, and level-up logic for quests.
    /// </summary>
    public interface IQuestRewardService
    {
        /// <summary>
        /// Calculates experience based on task type and duration.
        /// </summary>
        int CalculateExperience(TaskType type, int durationMinutes);

        /// <summary>
        /// Generates a random loot item based on task type.
        /// </summary>
        string GenerateLoot(TaskType type);

        /// <summary>
        /// Attempts to level up the given unit if XP threshold reached.
        /// Returns true if a level-up occurred.
        /// </summary>
        bool TryLevelUp(Unit unit);
    }

    public class QuestRewardService : IQuestRewardService
    {
        private readonly Random _rng = new();

        public int CalculateExperience(TaskType type, int durationMinutes)
        {
            // Example: Timer gives more XP than Tracker
            return type switch
            {
                TaskType.Timer => 10 * durationMinutes + _rng.Next(0, durationMinutes),
                TaskType.Tracker => 5 * durationMinutes + _rng.Next(0, durationMinutes / 2),
                _ => 0
            };
        }

        public string GenerateLoot(TaskType type)
        {
            // Loot pools per type
            var timerLoot = new[] { "Health Potion", "Iron Sword", "Gold Coin" };
            var trackerLoot = new[] { "XP Scroll", "Bronze Coin", "Minor Reagent" };

            var pool = type == TaskType.Timer ? timerLoot : trackerLoot;
            return pool[_rng.Next(pool.Length)];
        }

        public bool TryLevelUp(Unit unit)
        {
            if (unit.Experience < unit.ExperienceToNextLevel)
                return false;

            // Level up
            unit.Level++;
            unit.Experience -= unit.ExperienceToNextLevel;
            unit.ExperienceToNextLevel += unit.Level * 50; // scale threshold
            return true;
        }
    }
}
