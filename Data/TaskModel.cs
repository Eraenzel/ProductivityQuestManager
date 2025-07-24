namespace ProductivityQuestManager.Data
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public TaskType Type { get; set; } = TaskType.Timer;
        public int DurationMinutes { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? LastCompletedAt { get; set; }
        public bool IsRunning { get; set; } = false;
        public List<QuestResult> QuestResults { get; set; } = new();
        public List<TaskTag> TaskTags { get; set; } = new();
        
    }
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<TaskTag> TaskTags { get; set; } = new();
    }
    public class TaskTag
    {
        public int TaskModelId { get; set; }
        public TaskModel TaskModel { get; set; } = null!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }

    public enum TaskType { Timer, Tracker}

    public class CompletedTaskDTO
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = "";
        public TaskType Type { get; set; }
        public TimeSpan Duration { get; set; }           // actual elapsed
        public bool WasCancelled { get; set; }
        public DateTime CompletedAt { get; set; }        // or CancelledAt
        public List<string> Tags { get; set; } = new();
        public int ExperienceGained { get; set; }
        public string? Loot { get; set; }
        public string UnitName { get; set; } = "";
    }
}
