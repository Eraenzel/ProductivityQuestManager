namespace ProductivityQuestManager.Data
{
    public class QuestLine
    {
        public int Id { get; set; }
        public QuestCategory Category { get; set; }
        public string Name { get; set; }
        public int RequiredPoints { get; set; }   // points needed to unlock next tier
        public int RewardXp { get; set; }
        public int RewardResource { get; set; }  // wood, gold, etc.
    }

}
