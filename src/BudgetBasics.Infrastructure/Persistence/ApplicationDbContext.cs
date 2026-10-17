using BudgetBasics.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetBasics.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SiteStatistic> SiteStatistics => Set<SiteStatistic>();
    public DbSet<ExpenseEntry> ExpenseEntries => Set<ExpenseEntry>();
    public DbSet<FeedbackSubmission> FeedbackSubmissions => Set<FeedbackSubmission>();
    public DbSet<LearningTip> LearningTips => Set<LearningTip>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.FamilyId).HasMaxLength(64).IsRequired();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.FamilyId });
            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApplicationUser>()
            .Property(user => user.DisplayName)
            .HasMaxLength(80);

        builder.Entity<SiteStatistic>(entity =>
        {
            entity.ToTable("SiteStatistics");
            entity.HasKey(statistic => statistic.Id);
        });

        builder.Entity<ExpenseEntry>(entity =>
        {
            entity.ToTable("ExpenseEntries");
            entity.HasKey(expense => expense.Id);
            entity.Property(expense => expense.Category).HasMaxLength(30).IsRequired();
            entity.Property(expense => expense.Description).HasMaxLength(80).IsRequired();
            entity.Property(expense => expense.Amount).HasPrecision(18, 2);
            entity.HasIndex(expense => new { expense.UserId, expense.ExpenseDate });
            entity.HasOne(expense => expense.User)
                .WithMany()
                .HasForeignKey(expense => expense.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FeedbackSubmission>(entity =>
        {
            entity.ToTable("FeedbackSubmissions");
            entity.HasKey(feedback => feedback.Id);
            entity.Property(feedback => feedback.Name).HasMaxLength(80).IsRequired();
            entity.Property(feedback => feedback.Email).HasMaxLength(254).IsRequired();
            entity.Property(feedback => feedback.Comments).HasMaxLength(500).IsRequired();
            entity.HasIndex(feedback => feedback.SubmittedUtc);
            entity.HasOne(feedback => feedback.User)
                .WithMany()
                .HasForeignKey(feedback => feedback.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<LearningTip>(entity =>
        {
            entity.ToTable("LearningTips");
            entity.HasKey(tip => tip.Id);
            entity.Property(tip => tip.HabitKey).HasMaxLength(30).IsRequired();
            entity.Property(tip => tip.Title).HasMaxLength(80).IsRequired();
            entity.Property(tip => tip.Message).HasMaxLength(300).IsRequired();
            entity.HasIndex(tip => new { tip.HabitKey, tip.IsActive });
            entity.HasData(
                new LearningTip { Id = 1, HabitKey = "all", Title = "Use a weekly money check-in", Message = "Choose one fixed day to review what came in, what went out, and what needs attention next week.", SortOrder = 10, IsActive = true },
                new LearningTip { Id = 2, HabitKey = "all", Title = "Save before spending", Message = "Move even a small planned amount to savings as soon as your income arrives instead of waiting for leftovers.", SortOrder = 20, IsActive = true },
                new LearningTip { Id = 3, HabitKey = "food", Title = "Set a snack limit", Message = "Give snacks and takeaway a weekly amount. When it is used, choose a home-prepared option until the next week.", SortOrder = 30, IsActive = true },
                new LearningTip { Id = 4, HabitKey = "food", Title = "Plan two easy meals", Message = "Keep ingredients for two low-cost meals ready so an unplanned takeaway is less tempting on busy days.", SortOrder = 40, IsActive = true },
                new LearningTip { Id = 5, HabitKey = "transport", Title = "Compare the full weekly cost", Message = "Compare passes, shared rides, fuel, and parking over a whole week rather than judging one trip at a time.", SortOrder = 50, IsActive = true },
                new LearningTip { Id = 6, HabitKey = "transport", Title = "Combine nearby trips", Message = "Group errands in the same area and check safe public transport options to reduce repeated travel costs.", SortOrder = 60, IsActive = true },
                new LearningTip { Id = 7, HabitKey = "impulse", Title = "Try the 24-hour pause", Message = "Save the item instead of buying immediately. Recheck it tomorrow and confirm it still fits your wants budget.", SortOrder = 70, IsActive = true },
                new LearningTip { Id = 8, HabitKey = "impulse", Title = "Remove one-click temptation", Message = "Delete saved card details and promotional alerts so every purchase becomes a deliberate decision.", SortOrder = 80, IsActive = true },
                new LearningTip { Id = 9, HabitKey = "subscriptions", Title = "Run a monthly subscription audit", Message = "List every recurring charge, its next billing date, and the last time you used it. Cancel what no longer helps.", SortOrder = 90, IsActive = true },
                new LearningTip { Id = 10, HabitKey = "subscriptions", Title = "Avoid overlapping services", Message = "Keep one entertainment service at a time and rotate only after the current billing period ends.", SortOrder = 100, IsActive = true },
                new LearningTip { Id = 11, HabitKey = "saving", Title = "Name your savings goal", Message = "A specific goal and date make progress easier to track than a general intention to save more.", SortOrder = 110, IsActive = true },
                new LearningTip { Id = 12, HabitKey = "saving", Title = "Start an emergency buffer", Message = "Build a small first target for unexpected transport, medicine, or study costs, then increase it gradually.", SortOrder = 120, IsActive = true });
        });
    }
}
