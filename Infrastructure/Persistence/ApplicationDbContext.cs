using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<MissionVote> MissionVotes => Set<MissionVote>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamVote> TeamVotes => Set<TeamVote>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Game>(entity =>
        {
            entity.HasMany(g => g.Players)
                .WithOne(p => p.Game)
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(g => g.Rounds)
                .WithOne(r => r.Game)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            //Set unique constraint on ConnectionCode for games in lobby or InProgress
            entity.HasIndex(g => g.ConnectionCode)
                .HasFilter($"[Status] IN ({(int)GameStatus.Lobby}, {(int)GameStatus.InProgress})")
                .IsUnique();
                
        });

        builder.Entity<GamePlayer>()
            .HasOne<ApplicationUser>()
                .WithMany(u => u.GamePlayers)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Round>(entity =>
        {
            entity.HasOne(r => r.Game)
                .WithMany(g => g.Rounds)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Teams)
                .WithOne(t => t.Round)
                .HasForeignKey(t => t.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.MissionVotes)
                .WithOne(mv => mv.Round)
                .HasForeignKey(mv => mv.RoundId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MissionVote>()
            .HasOne(mv => mv.Round)
            .WithMany(r => r.MissionVotes)
            .HasForeignKey(mv => mv.RoundId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Team>(entity =>
        {
            entity.HasOne(t => t.Round)
                .WithMany(r => r.Teams)
                .HasForeignKey(t => t.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Members)
                .WithOne(tm => tm.Team)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Votes)
                .WithOne(tv => tv.Team)
                .HasForeignKey(tv => tv.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<TeamVote>()
            .HasOne(tv => tv.Team)
            .WithMany(t => t.Votes)
            .HasForeignKey(tv => tv.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}
