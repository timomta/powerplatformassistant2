using Microsoft.EntityFrameworkCore;
using PowerPlatformAssistant.Web.Models;

namespace PowerPlatformAssistant.Web.Data;

public sealed class PowerPlatformAssistantDbContext(DbContextOptions<PowerPlatformAssistantDbContext> options) : DbContext(options)
{
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<ConversationTurn> ConversationTurns => Set<ConversationTurn>();

    public DbSet<OnboardingState> OnboardingStates => Set<OnboardingState>();

    public DbSet<GuidanceChecklist> GuidanceChecklists => Set<GuidanceChecklist>();

    public DbSet<GuidanceChecklistStep> GuidanceChecklistSteps => Set<GuidanceChecklistStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSession>()
            .HasKey(session => session.SessionId);

        modelBuilder.Entity<UserSession>()
            .HasIndex(session => session.UserId)
            .IsUnique();

        modelBuilder.Entity<UserSession>()
            .HasMany(session => session.Conversations)
            .WithOne()
            .HasForeignKey(conversation => conversation.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Conversation>()
            .HasKey(conversation => conversation.ConversationId);

        modelBuilder.Entity<ConversationTurn>()
            .HasKey(turn => turn.TurnId);

        modelBuilder.Entity<OnboardingState>()
            .HasKey(state => state.OnboardingStateId);

        modelBuilder.Entity<GuidanceChecklist>()
            .HasKey(checklist => checklist.ChecklistId);

        modelBuilder.Entity<GuidanceChecklistStep>()
            .HasKey(step => step.StepId);

        modelBuilder.Entity<Conversation>()
            .HasMany(conversation => conversation.Turns)
            .WithOne()
            .HasForeignKey(turn => turn.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuidanceChecklist>()
            .HasMany(checklist => checklist.Steps)
            .WithOne()
            .HasForeignKey(step => step.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationTurn>()
            .Property(turn => turn.SenderType)
            .HasMaxLength(32);

        modelBuilder.Entity<OnboardingState>()
            .Property(state => state.ExperienceLevel)
            .HasMaxLength(32);

        modelBuilder.Entity<OnboardingState>()
            .Property(state => state.FlowType)
            .HasMaxLength(32);
    }
}