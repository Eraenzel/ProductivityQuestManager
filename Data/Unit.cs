namespace ProductivityQuestManager.Data
{
    public class Unit
    {
        public int Id { get; set; }  // Primary key

        // Basic info
        public string Name { get; set; }
        public UnitClass Class { get; set; }  // e.g. Warrior, Mage, Rogue
        public bool IsActive { get; set; } = false;

        // Stats
        public int Level { get; set; } = 1;
        public int Health { get; set; } = 100;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public int Speed { get; set; } = 5;

        // Experience and progression
        public int Experience { get; set; } = 0;
        public int ExperienceToNextLevel { get; set; } = 100;

        // Quest state
        public bool IsOnQuest { get; set; } = false;
        public List<QuestResult> QuestResults { get; set; }
    }

    public enum UnitClass
    {
        Warrior,
        Mage,
        Archer,
        Rogue
    }
}
