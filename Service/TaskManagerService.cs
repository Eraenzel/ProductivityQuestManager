using Microsoft.EntityFrameworkCore;
using ProductivityQuestManager.Data;
using System.Text.Json;
using System.Timers;


public class TaskManagerService : IDisposable
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly System.Timers.Timer _timer;
    private readonly Random _rng = new();

    private List<string> _firstNames = new();
    private List<string> _surnames = new();

    public event Action? OnChange;

    public List<TaskModel> Tasks { get; private set; } = new();
    public TaskModel? ActiveTask { get; private set; }
    public QuestResult? LastResult { get; private set; }
    public Func<Task>? InvokeStateHasChangedAsync { get; set; }
    public Unit? ActiveUnit { get; private set; }

    public class NamePool
    {
        public List<string> FirstNames { get; set; } = new();
        public List<string> Surnames { get; set; } = new();
    }

    public TaskManagerService(AppDbContext db,
            IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
        LoadNamePool();
        LoadState();        

        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (s, e) => NotifyStateChanged();
        _timer.Start();
    }

    #region Initialization

    private void LoadNamePool()
    {
        var webRoot = _env.WebRootPath;
        var path = Path.Combine(
            webRoot,
            "data",
            "names.json");

        Console.WriteLine($"[LoadNamePool] Looking for names.json at: {path}");
        Console.WriteLine($"[LoadNamePool] File exists: {File.Exists(path)}");

        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var pool = JsonSerializer.Deserialize<NamePool>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); 
                if (pool is not null)
                {
                    _firstNames = pool.FirstNames;
                    _surnames = pool.Surnames;
                    Console.WriteLine($"[LoadNamePool] Loaded {pool.FirstNames.Count} first names and {pool.Surnames.Count} surnames.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadNamePool] Error deserializing names.json: {ex.Message}");
                // Ignore malformed JSON, fallback below
            }
        }

        // Fallback default names
        if (_firstNames.Count == 0)
            _firstNames = new List<string> { "Aldric", "Branwen", "Cedric" };

        if (_surnames.Count == 0)
            _surnames = new List<string> { "Brightwood", "Crowhaven", "Duskblade" };
    }


    public void LoadState()
    {
        Tasks = _db.Tasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .ToList();
        ActiveTask = Tasks.FirstOrDefault(t => t.IsRunning);
        ActiveUnit = _db.Units.FirstOrDefault(u => u.IsActive);
        CleanupExpiredTask();
    }

    #endregion

    public List<Unit> GetUnits() => _db.Units.ToList();

    public string GetTimeRemaining(TaskModel task)
    {
        if (task.StartedAt == null) return "unknown";
        var end = task.StartedAt.Value.AddMinutes(task.DurationMinutes);
        var remaining = end - DateTime.UtcNow;
        return remaining <= TimeSpan.Zero ? "Done" : $"{remaining.Minutes} min {remaining.Seconds} sec";
    }

    public async Task StartTaskAsync(TaskModel task, IEnumerable<string> tagNames, TaskType type)
    {
        if (ActiveTask != null) return;

        //AssignTagsToTask(task.Id, tagNames);

        task.IsRunning = true;
        task.StartedAt = DateTime.UtcNow;
        task.Type = type;
        ActiveTask = task;
        _db.Update(task);
        await _db.SaveChangesAsync();
        LoadState();        // repopulates Tasks, including TaskTags via your Include
        NotifyStateChanged();

        if (task.Type == TaskType.Timer)
        {
            // wait out the duration
            await Task.Delay(task.DurationMinutes * 60 * 1000);
            await CompleteTaskAsync(task);
        }
        // else: Tracker mode waits for explicit StopTaskAsync
    }
    public async Task StopTaskAsync()
    {
        if (ActiveTask == null) return;

        if (ActiveTask.Type == TaskType.Tracker)
            await CompleteTaskAsync(ActiveTask);
        else  // Timer
            await CancelTimerTask(ActiveTask);
    }
    private async Task CancelTimerTask(TaskModel task)
    {
        // reuse your existing “cancel” flow
        await CancelActiveTask();  // or inline its logic here
    }
    private async Task CompleteTaskAsync(TaskModel task)
    {
        var unit = _db.Units.FirstOrDefault(u => u.Id == ActiveUnit!.Id);
        if (unit == null) return;

        if (!task.StartedAt.HasValue)
            throw new InvalidOperationException($"Cannot complete task {task.Id} because it never started.");

        // finalize
        var started = task.StartedAt.Value;
        task.IsRunning = false;
        task.LastCompletedAt = DateTime.UtcNow;
        _db.Update(task);

        // spawn Quest & result
        var quest = new Quest { Name = task.Title, DurationMinutes = task.DurationMinutes, StartedAt = started };
        _db.Quests.Add(quest);
        await _db.SaveChangesAsync();

        // 3) Compute reward based on type
        int xp, lootTier;
        string lootItem, summary;
        if (task.Type == TaskType.Timer)
        {
            xp = 10 + _rng.Next(10);
            lootTier = 1;
            lootItem = "Basic Loot Chest";
            summary = $"Timer '{task.Title}' completed automatically.";
        }
        else  // Tracker
        {
            xp = 5 + _rng.Next(5);
            lootTier = 0;
            lootItem = "Tracker Reward Bag";
            summary = $"Tracker '{task.Title}' stopped manually.";
        }

        var result = new QuestResult
        {
            QuestId = quest.Id,
            UnitId = ActiveUnit!.Id,
            WasSuccessful = true,
            CompletedAt = DateTime.UtcNow,
            OutcomeSummary = summary,
            ExperienceGained = xp,
            Loot = lootItem
        };

        unit.Experience += result.ExperienceGained;
        if (unit.Experience >= unit.ExperienceToNextLevel)
        {
            unit.Level++;
            unit.Experience = 0;
            unit.ExperienceToNextLevel += 50;
        }
        _db.Update(unit);
        LastResult = result;
        _db.QuestResults.Add(result);
        await _db.SaveChangesAsync();

        ActiveTask = null;
        NotifyStateChanged();


    }

    public void AddTask(string title, int duration, IEnumerable<string> tagNames)
    {
        var task = new TaskModel
        {
            Title = title,
            DurationMinutes = duration
        };
        _db.Tasks.Add(task);
        _db.SaveChanges();

        AssignTagsToTask(task.Id, tagNames);

        LoadState();
        NotifyStateChanged();
    }

    public void DeleteTask(TaskModel task)
    {
        if (ActiveTask != null && task.Id == ActiveTask.Id) return;
        _db.Tasks.Remove(task);
        _db.SaveChanges();
        LoadState();
        NotifyStateChanged();
    }

    public async Task Debug_ForceCompleteActiveTask()
    {
        if (ActiveTask == null || ActiveUnit == null || ActiveTask.Type == TaskType.Tracker) return;

        var unit = ActiveUnit;
        var task = ActiveTask;

        var quest = new Quest
        {
            Name = $"Quest for {task.Title}",
            DurationMinutes = task.DurationMinutes,
            StartedAt = DateTime.UtcNow
        };
        _db.Quests.Add(quest);
        await _db.SaveChangesAsync();

        task.IsRunning = false;
        task.StartedAt = null;
        task.LastCompletedAt = DateTime.UtcNow;
        _db.Update(task);

        var result = new QuestResult
        {
            QuestId = quest.Id,
            UnitId = unit.Id,
            WasSuccessful = true,
            CompletedAt = DateTime.UtcNow,
            OutcomeSummary = $"Task '{task.Title}' was force completed by {unit.Name}.",
            ExperienceGained = 10 + _rng.Next(10),
            Loot = "Manual Completion Reward"
        };
        _db.QuestResults.Add(result);        
        LastResult = result;
        unit.QuestResults.Add(result);

        unit.Experience += result.ExperienceGained;
        if (unit.Experience >= unit.ExperienceToNextLevel)
        {
            unit.Level++;
            unit.Experience = 0;
            unit.ExperienceToNextLevel += 50;
        }
        _db.Update(unit);

        ActiveTask = null;
        await _db.SaveChangesAsync();
        LoadState();
        NotifyStateChanged();
    }

    public async Task CancelActiveTask()
    {
        if (ActiveTask == null || ActiveUnit == null) return;

        var task = ActiveTask;
        task.IsRunning = false;
        task.StartedAt = null;
        _db.Update(task);

        var quest = new Quest
        {
            Name = $"Quest {task.Title} cancelled.",
            DurationMinutes = task.DurationMinutes,
            StartedAt = DateTime.UtcNow
        };
        _db.Quests.Add(quest);
        await _db.SaveChangesAsync();

        var result = new QuestResult
        {
            QuestId = quest.Id,
            UnitId = ActiveUnit.Id,
            WasSuccessful = false,
            CompletedAt = DateTime.UtcNow,
            OutcomeSummary = $"Task '{task.Title}' was cancelled.",
            ExperienceGained = 0,
            Loot = "None"
        };
        _db.QuestResults.Add(result);
        LastResult = result;

        ActiveTask = null;
        _db.SaveChanges();
        LoadState();
        NotifyStateChanged();
    }

    public void ClearResult()
    {
        LastResult = null;
        NotifyStateChanged();
    }

    private void CleanupExpiredTask()
    {
        if (ActiveTask != null && ActiveTask.StartedAt != null)
        {
            var endTime = ActiveTask.StartedAt.Value.AddMinutes(ActiveTask.DurationMinutes);
            if (DateTime.UtcNow >= endTime)
            {
                ActiveTask.IsRunning = false;
                ActiveTask.StartedAt = null;
                _db.Update(ActiveTask);
                _db.SaveChanges();
                ActiveTask = null;
            }
        }
    }

    private void NotifyStateChanged() => _ = InvokeStateHasChangedAsync?.Invoke();

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }

    public void SetActiveUnit(int unitId)
    {
        // clear old
        var old = _db.Units.FirstOrDefault(u => u.IsActive);
        if (old != null)
        {
            old.IsActive = false;
            _db.Update(old);
        }

        // set new
        var u = _db.Units.Find(unitId);
        if (u != null)
        {
            u.IsActive = true;
            _db.Update(u);
            ActiveUnit = u;
        }

        _db.SaveChanges();
        NotifyStateChanged();
    }

    public void AddUnit(string name, UnitClass unitClass)
    {
        Console.WriteLine($"[AddUnit] Adding unit: {name}");
        if (string.IsNullOrWhiteSpace(name)) return;

        var unit = new Unit
        {
            Name = name,
            //Id = 5,
            Class = unitClass,
            Level = 1,
            Experience = 0,
            ExperienceToNextLevel = 100
        };
        _db.Units.Add(unit);
        _db.SaveChanges();
        NotifyStateChanged();
    }

    public void DeleteUnit(int id)
    {
        //Console.WriteLine($"[Service] Deleting unit {id}");
        var unit = _db.Units.FirstOrDefault(u => u.Id == id);
        
        if (unit != null)
        {
            _db.Units.Remove(unit);
            _db.SaveChanges();
            NotifyStateChanged();
        }
        else
        {
            Console.WriteLine("[Service] Unit not found!");
        }
    }
    public void AddRandomUnit()
    {
        // pick a random class
        var classes = Enum.GetValues<UnitClass>();
        var cls = classes[_rng.Next(classes.Length)];

        var first = _firstNames[_rng.Next(_firstNames.Count)];
        var last = _surnames[_rng.Next(_surnames.Count)];
        var name = $"{first} {last}";
        // generate a name (or use a list of fantasy names)
        //var name = $"Hero{_rng.Next(1000, 9999)}";

        var unit = new Unit
        {
            Name = name,
            Class = cls,
            Level = 1,
            Experience = 0,
            ExperienceToNextLevel = 100
        };

        _db.Units.Add(unit);
        _db.SaveChanges();
        NotifyStateChanged();
    }

    // Edit an existing entry (in case you forgot to start/stop)
    public void UpdateEntry(int id, string description, DateTime start, DateTime? stop)
    {
        var entry = _db.Tasks.Find(id);
        if (entry == null) return;
        entry.Title = description;
        entry.StartedAt = start;
        entry.LastCompletedAt = stop;
        _db.Update(entry);
        _db.SaveChanges();
        LoadState();
        NotifyStateChanged();
    }

    public void UpdateTask(int taskId, string title, int duration, IEnumerable<string> tagNames)
    {
        var task = _db.Tasks.Find(taskId);
        if (task == null) return;
        task.Title = title;
        task.DurationMinutes = duration;
        _db.Update(task);
        _db.SaveChanges();

        AssignTagsToTask(taskId, tagNames);

        LoadState();
        NotifyStateChanged();
    }

    // Pull recent entries for display
    public List<TaskModel> GetRecentEntries(int count = 10)
    {
        return _db.Tasks                
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .OrderByDescending(t => t.StartedAt)
                .Take(count)
                .ToList();
    }

    public TimeSpan GetActiveTimerElapsed()
    {
        if (ActiveTask == null || !ActiveTask.StartedAt.HasValue)
            return TimeSpan.Zero;

        return DateTime.UtcNow - ActiveTask.StartedAt.Value;
    }

    public TaskModel? ActiveTimer { get; private set; }

    public List<Tag> GetAllTags() =>
    _db.Tags
      .Include(t => t.TaskTags)
      .OrderBy(t => t.Name)
      .ToList();

    public void AddTagIfNotExists(string name)
    {
        if (_db.Tags.Any(t => t.Name == name)) return;
        _db.Tags.Add(new Tag { Name = name });
        _db.SaveChanges();
    }

    public void AssignTagsToTask(int taskId, IEnumerable<string> tagNames)
    {
        // 2a) ensure tags exist
        foreach (var name in tagNames)
            AddTagIfNotExists(name);

        // 2b) load the task and its join-rows
        var task = _db.Tasks
                      .Include(t => t.TaskTags)
                          .ThenInclude(tt => tt.Tag)
                      .FirstOrDefault(t => t.Id == taskId);
        if (task == null) return;

        // 2c) clear old
        task.TaskTags.Clear();

        // 2d) re-assign
        var tags = _db.Tags.Where(t => tagNames.Contains(t.Name)).ToList();
        foreach (var tag in tags)
        {
            task.TaskTags.Add(new TaskTag
            {
                TaskModelId = task.Id,
                TagId = tag.Id,
                TaskModel = task,
                Tag = tag
            });
        }

        _db.SaveChanges();
        NotifyStateChanged();
    }

    public void CreateTag(string name)
    {
        if (_db.Tags.Any(t => t.Name == name)) return;
        _db.Tags.Add(new Tag { Name = name });
        _db.SaveChanges();
        NotifyStateChanged();
    }

    public void RenameTag(int id, string newName)
    {
        var tag = _db.Tags.Find(id);
        if (tag == null) return;
        tag.Name = newName;
        _db.SaveChanges();
        NotifyStateChanged();
    }

    public void DeleteTag(int id)
    {
        var tag = _db.Tags
                     .Include(t => t.TaskTags)
                     .FirstOrDefault(t => t.Id == id);
        if (tag == null) return;
        // remove all join entries
        _db.RemoveRange(tag.TaskTags);
        _db.Tags.Remove(tag);
        _db.SaveChanges();
        NotifyStateChanged();
    }
}
