namespace ProductivityQuestManager.Data
{
    public class Quest
    {
        public int Id { get; set; }  
        public string Name { get; set; }
        public DateTime StartedAt { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsSuccessful { get; set; }
        public List<QuestResult> Results { get; set; }
    }
}
