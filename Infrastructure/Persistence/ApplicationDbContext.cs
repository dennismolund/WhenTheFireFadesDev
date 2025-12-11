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

        builder.Entity<GamePlayer>(entity =>
        {
            // Foreign key index for JOIN operations
            entity.HasIndex(gp => gp.GameId);

            // Composite index for seat assignment queries
            // Optimizes: GetNextAvailableSeatAsync - Where(gp => gp.GameId == gameId).OrderBy(gp => gp.Seat)
            entity.HasIndex(gp => new { gp.GameId, gp.Seat });
        });

        builder.Entity<GamePlayer>()
            .HasOne<ApplicationUser>()
                .WithMany(u => u.GamePlayers)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);

        // Index on nullable foreign key for user lookups
        builder.Entity<GamePlayer>()
            .HasIndex(gp => gp.UserId);

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

            // Foreign key index for JOIN operations
            entity.HasIndex(r => r.GameId);

            // Composite index for round snapshot queries
            // Optimizes: GetCurrentRoundSnapshot - SingleAsync(r => r.GameId == gameId && r.RoundNumber == roundNumber)
            entity.HasIndex(r => new { r.GameId, r.RoundNumber });
        });

        builder.Entity<MissionVote>(entity =>
        {
            entity.HasOne(mv => mv.Round)
                .WithMany(r => r.MissionVotes)
                .HasForeignKey(mv => mv.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key index for vote retrieval
            // Optimizes: GetByRoundIdAsync - Where(v => v.RoundId == roundId)
            entity.HasIndex(mv => mv.RoundId);
        });
        
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

            // Composite index for active team lookups
            // Optimizes: GetByRoundIdAsync & GetActiveByRoundIdAsync - Where(t => t.RoundId == roundId && t.IsActive)
            // RoundId first because it's more selective (many rounds) than IsActive (boolean)
            entity.HasIndex(t => new { t.RoundId, t.IsActive });
        });

        builder.Entity<TeamMember>(entity =>
        {
            entity.HasOne(tm => tm.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key index for JOIN operations when including team members
            entity.HasIndex(tm => tm.TeamId);
        });


        builder.Entity<TeamVote>(entity =>
        {
            entity.HasOne(tv => tv.Team)
                .WithMany(t => t.Votes)
                .HasForeignKey(tv => tv.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key index for vote retrieval
            // Optimizes: GetByTeamAsync - Where(v => v.TeamId == teamId)
            entity.HasIndex(tv => tv.TeamId);
        });
        
    }
}
