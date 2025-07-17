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
        Tasks = _db.Tasks.ToList();
        ActiveTask = Tasks.FirstOrDefault(t => t.IsRunning);
        ActiveUnit = _db.Units.FirstOrDefault(u => u.IsActive);
        CleanupExpiredTask();
    }

    #endregion

    public List<Unit> GetUnits() => _db.Units.ToList();

    public bool IsCoolingDown(TaskModel task)
    {
        if (!task.IsRepeatable || task.LastCompletedAt == null) return false;
        return DateTime.UtcNow < task.LastCompletedAt.Value.AddMinutes(task.CooldownMinutes);
    }

    public string GetTimeRemaining(TaskModel task)
    {
        if (task.StartedAt == null) return "unknown";
        var end = task.StartedAt.Value.AddMinutes(task.DurationMinutes);
        var remaining = end - DateTime.UtcNow;
        return remaining <= TimeSpan.Zero ? "Done" : $"{remaining.Minutes} min {remaining.Seconds} sec";
    }

    public async Task StartTaskAsync(TaskModel task, int unitId)
    {
        if (IsCoolingDown(task) || ActiveTask != null) return;

        var unit = _db.Units.FirstOrDefault(u => u.Id == unitId);
        if (unit == null) return;

        task.IsRunning = true;
        task.StartedAt = DateTime.UtcNow;
        ActiveTask = task;
        _db.Update(task);

        var quest = new Quest
        {
            Name = $"Quest for {task.Title}",
            DurationMinutes = task.DurationMinutes,
            StartedAt = DateTime.UtcNow
        };
        _db.Quests.Add(quest);
        await _db.SaveChangesAsync();

        await Task.Delay(task.DurationMinutes * 60 * 1000); // Simulate duration (1 sec = 1 min for test) 

        task.IsRunning = false;
        task.StartedAt = null;
        task.LastCompletedAt = DateTime.UtcNow;
        if (!task.IsRepeatable) task.IsCompleted = true;
        _db.Update(task);

        var result = new QuestResult
        {
            QuestId = quest.Id,
            UnitId = unit.Id,
            WasSuccessful = true,
            CompletedAt = DateTime.UtcNow,
            OutcomeSummary = $"Task '{task.Title}' completed on time by {unit.Name}.",
            ExperienceGained = 10 + _rng.Next(10),
            Loot = "Basic Loot Chest"
        };

        _db.QuestResults.Add(result);
        LastResult = result;

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

    public void AddTask(string title, int duration, bool isRepeatable)
    {
        var task = new TaskModel
        {
            Title = title,
            DurationMinutes = duration,
            IsRepeatable = isRepeatable,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.Tasks.Add(task);
        _db.SaveChanges();
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

    public void CancelCooldown(TaskModel task)
    {
        task.LastCompletedAt = null;
        _db.Update(task);
        _db.SaveChanges();
        LoadState();
        NotifyStateChanged();
    }

    public async Task ForceCompleteActiveTask()
    {
        if (ActiveTask == null || ActiveUnit == null) return;

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
        if (!task.IsRepeatable) task.IsCompleted = true;
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

    // Start a new time entry
    public void StartTimer(string? description = null, int? taskId = null)
    {
        // if there’s already a live timer, ignore or stop it first
        if (ActiveTimer != null && ActiveTimer.StoppedAt == null) return;

        var entry = new TimeEntry
        {
            TaskId = taskId,
            Description = description ?? string.Empty,
            StartedAt = DateTime.UtcNow
        };
        _db.TimeEntries.Add(entry);
        _db.SaveChanges();
        ActiveTimer = entry;
        NotifyStateChanged();
    }

    // Stop the running timer
    public void StopTimer()
    {
        if (ActiveTimer == null || ActiveTimer.StoppedAt != null) return;

        ActiveTimer.StoppedAt = DateTime.UtcNow;
        _db.Update(ActiveTimer);
        _db.SaveChanges();
        NotifyStateChanged();
    }

    // Edit an existing entry (in case you forgot to start/stop)
    public void UpdateEntry(int id, string description, DateTime start, DateTime? stop)
    {
        var entry = _db.TimeEntries.Find(id);
        if (entry == null) return;
        entry.Description = description;
        entry.StartedAt = start;
        entry.StoppedAt = stop;
        _db.Update(entry);
        _db.SaveChanges();
        LoadState();
        NotifyStateChanged();
    }

    // Pull recent entries for display
    public List<TimeEntry> GetRecentEntries(int count = 10)
    {
        return _db.TimeEntries
                .Include(e => e.Task)
                .Include(e => e.TimeEntryTags)
                .ThenInclude(tet => tet.Tag)
                .OrderByDescending(e => e.StartedAt)
                .Take(count)
                .ToList();
    }

    public TimeSpan GetActiveTimerElapsed()
    {
        if (ActiveTimer == null)
            return TimeSpan.Zero;

        // not stopped yet, so measure against Now
        var end = ActiveTimer.StoppedAt ?? DateTime.UtcNow;
        return end - ActiveTimer.StartedAt;
    }

    public TimeEntry? ActiveTimer { get; private set; }

    public List<Tag> GetAllTags() =>
    _db.Tags.OrderBy(t => t.Name).ToList();

    public void AddTagIfNotExists(string name)
    {
        if (_db.Tags.Any(t => t.Name == name)) return;
        _db.Tags.Add(new Tag { Name = name });
        _db.SaveChanges();
    }

    public void AssignTags(int entryId, IEnumerable<string> tagNames)
    {
        var entry = _db.TimeEntries
            .Include(te => te.TimeEntryTags)
            .FirstOrDefault(te => te.Id == entryId);
        if (entry == null) return;

        // ensure tags exist
        foreach (var name in tagNames)
            AddTagIfNotExists(name);

        // clear existing
        entry.TimeEntryTags.Clear();

        // re-fetch tags & assign
        var tags = _db.Tags.Where(t => tagNames.Contains(t.Name)).ToList();
        foreach (var tag in tags)
            entry.TimeEntryTags.Add(new TimeEntryTag
            {
                TimeEntry = entry,
                Tag = tag
            });

        _db.Update(entry);
        _db.SaveChanges();
        LoadState();
        NotifyStateChanged();
    }
}
