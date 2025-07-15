namespace ProductivityQuestManager.Data
{
    public class TimeEntry
    {
        public int Id { get; set; }
        public int? TaskId { get; set; }    // optional link to a TaskModel
        public TaskModel? Task { get; set; }

        public string Description { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? StoppedAt { get; set; }

        public TimeSpan? Duration =>
            StoppedAt.HasValue
              ? StoppedAt.Value - StartedAt
              : null;
    }
}
