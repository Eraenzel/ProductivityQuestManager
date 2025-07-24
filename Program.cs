using Microsoft.EntityFrameworkCore;
using ProductivityQuestManager.Components;
using ProductivityQuestManager.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();    

// This creates a local SQLite file
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddScoped<TaskManagerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    /*var path = Path.Combine(
    app.Environment.ContentRootPath,
    "app.db");

    if (File.Exists(path))
        File.Delete(path);*/

    db.Database.Migrate(); // Ensure DB is up-to-date
    DbInitializer.Seed(db, false); // Seed mock data
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
//app.UseRouting();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
