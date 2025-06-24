namespace ProductivityQuestManager.Data
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; } = false;
    }
}
