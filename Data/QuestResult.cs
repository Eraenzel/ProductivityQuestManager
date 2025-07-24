namespace ProductivityQuestManager.Data
{
    public class QuestResult
    {
        public int Id { get; set; }  // Primary key

        // Link to the original quest
        public int QuestId { get; set; }
        public Quest Quest { get; set; }

        // Link to the unit
        public int UnitId { get; set; }
        public Unit Unit { get; set; }

        public int? TaskModelId { get; set; }
        public TaskModel? TaskModel { get; set; } 

        // Outcome data
        public bool WasSuccessful { get; set; }
        public string OutcomeSummary { get; set; }  // e.g. "Completed in 43 mins", or "Failed: timer interrupted"
        public DateTime CompletedAt { get; set; }

        // Rewards
        public int ExperienceGained { get; set; }
        public string Loot { get; set; }  // Optional: "Iron Sword", "Potion", etc.

    }
}
