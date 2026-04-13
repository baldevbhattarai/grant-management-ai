using GrantManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantManagement.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Grant> Grants { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportSection> ReportSections { get; set; }
    public DbSet<AIUsageLog> AIUsageLogs { get; set; }
    public DbSet<AIApprovedContent> AIApprovedContent { get; set; }
    public DbSet<ChatConversation> ChatConversations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.UserId);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Role).HasMaxLength(50).IsRequired();
            e.Property(u => u.OrganizationName).HasMaxLength(255);
        });

        modelBuilder.Entity<Grant>(e =>
        {
            e.HasKey(g => g.GrantId);
            e.HasIndex(g => g.GrantNumber).IsUnique();
            e.Property(g => g.GrantNumber).HasMaxLength(50).IsRequired();
            e.Property(g => g.GrantType).HasMaxLength(50).IsRequired();
            e.Property(g => g.ProgramName).HasMaxLength(255).IsRequired();
            e.Property(g => g.Status).HasMaxLength(50).HasDefaultValue("Active");
            e.Property(g => g.FundingAmount).HasColumnType("decimal(18,2)");
            e.HasOne(g => g.User).WithMany(u => u.Grants).HasForeignKey(g => g.UserId);
        });

        modelBuilder.Entity<Report>(e =>
        {
            e.HasKey(r => r.ReportId);
            e.Property(r => r.ReportingQuarter).HasMaxLength(10).IsRequired();
            e.Property(r => r.ReportType).HasMaxLength(50).IsRequired();
            e.Property(r => r.Status).HasMaxLength(50).HasDefaultValue("Draft");
            e.HasOne(r => r.Grant).WithMany(g => g.Reports).HasForeignKey(r => r.GrantId);
        });

        modelBuilder.Entity<ReportSection>(e =>
        {
            e.HasKey(s => s.SectionId);
            e.Property(s => s.SectionName).HasMaxLength(100).IsRequired();
            e.Property(s => s.SectionTitle).HasMaxLength(255).IsRequired();
            e.Property(s => s.ResponseType).HasMaxLength(50).IsRequired();
            e.Property(s => s.ResponseNumber).HasColumnType("decimal(18,2)");
            e.HasOne(s => s.Report).WithMany(r => r.Sections).HasForeignKey(s => s.ReportId);
        });

        modelBuilder.Entity<AIUsageLog>(e =>
        {
            e.ToTable("AI_UsageLog");
            e.HasKey(l => l.LogId);
            e.Property(l => l.FeatureType).HasMaxLength(50).IsRequired();
            e.Property(l => l.ModelName).HasMaxLength(50).IsRequired();
            e.Property(l => l.UserAction).HasMaxLength(50);
            e.Property(l => l.EstimatedCost).HasColumnType("decimal(10,6)");
            e.HasOne(l => l.User).WithMany(u => u.AIUsageLogs).HasForeignKey(l => l.UserId).IsRequired(false);
        });

        modelBuilder.Entity<AIApprovedContent>(e =>
        {
            e.ToTable("AI_ApprovedContent");
            e.HasKey(c => c.ContentId);
            e.Property(c => c.SectionName).HasMaxLength(100).IsRequired();
            e.Property(c => c.GrantType).HasMaxLength(50);
            e.Property(c => c.Keywords).HasMaxLength(500);
            e.HasOne(c => c.Grant).WithMany(g => g.ApprovedContent).HasForeignKey(c => c.GrantId);
            e.HasIndex(c => new { c.ProgramTypeCode, c.SectionName });
            e.HasIndex(c => c.ReviewerRating);
        });

        modelBuilder.Entity<ChatConversation>(e =>
        {
            e.HasKey(c => c.ConversationId);
            e.Property(c => c.Role).HasMaxLength(50).IsRequired();
        });
    }
}
