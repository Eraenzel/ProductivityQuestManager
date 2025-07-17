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
        public List<TimeEntryTag> TimeEntryTags { get; set; } = new();
    }

    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public List<TimeEntryTag> TimeEntryTags { get; set; } = new();
    }

    public class TimeEntryTag
    {
        public int TimeEntryId { get; set; }
        public TimeEntry TimeEntry { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
