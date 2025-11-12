using Application.Interfaces;
using Application.Services;
using Application.UseCases.GamePlayers;
using Application.UseCases.Games;
using Application.UseCases.Rounds;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Web.Helpers;
using Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

//Removed server header to limit project information to clients.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
        ));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireDigit = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(6);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<SessionHelper>();

builder.Services.AddScoped<CreateGameFeature>();

builder.Services.AddScoped<CreateGamePlayerFeature>();

builder.Services.AddScoped<StartGameFeature>();

builder.Services.AddScoped<CreateRoundFeature>();

builder.Services.AddScoped<GameOrchestrator>();

builder.Services.AddScoped<IGameRepository, GameRepository>();

builder.Services.AddScoped<IGamePlayerRepository, GamePlayerRepository>();

builder.Services.AddScoped<IRoundRepository, RoundRepository>();

builder.Services.AddScoped<ITeamRepository, TeamRepository>();

builder.Services.AddScoped<ITeamVoteRepository, TeamVoteRepository>();

builder.Services.AddScoped<IMissionVoteRepository, MissionVoteRepository>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<GameHub>("/gameHub");

app.Run();
