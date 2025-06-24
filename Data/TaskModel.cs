namespace ProductivityQuestManager.Data
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; } = false;
        public bool IsRepeatable { get; set; } = false;
        public DateTime? LastCompletedAt { get; set; }
        public int CooldownMinutes { get; set; } = 5; // default cooldown
        public bool IsRunning { get; set; } = false;
        public DateTime? StartedAt { get; set; }
    }
}
