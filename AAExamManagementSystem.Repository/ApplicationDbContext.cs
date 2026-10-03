using AAExamManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Repository;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>,
    ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionType> QuestionTypes => Set<QuestionType>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<QuestionAndChoice> QuestionAndChoices => Set<QuestionAndChoice>();
    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<WebPage> WebPages => Set<WebPage>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("AAExam");

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Section)
            .WithMany(s => s.Users)
            .HasForeignKey(u => u.SectionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Course)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Exam>()
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Question>()
            .HasOne(q => q.QuestionType)
            .WithMany(qt => qt.Questions)
            .HasForeignKey(q => q.QuestionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Question>()
            .HasOne(q => q.Section)
            .WithMany(s => s.Questions)
            .HasForeignKey(q => q.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuestionAndChoice>()
            .HasOne(qc => qc.Question)
            .WithMany(q => q.QuestionAndChoices)
            .HasForeignKey(qc => qc.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuestionAndChoice>()
            .HasOne(qc => qc.Choice)
            .WithMany(c => c.QuestionAndChoices)
            .HasForeignKey(qc => qc.ChoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Answer>()
            .HasOne(a => a.Applicant)
            .WithMany(ap => ap.Answers)
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Answer>()
            .HasOne(a => a.Attempt)
            .WithMany(at => at.Answers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Attempt>()
            .HasOne(a => a.Applicant)
            .WithMany(ap => ap.Attempts)
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Applicant>()
            .HasOne(a => a.User)
            .WithOne(u => u.Applicant)
            .HasForeignKey<Applicant>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Applicant>()
            .HasIndex(a => a.UserId)
            .IsUnique();

        builder.Entity<AuditLog>()
            .HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        builder.Entity<WebPage>()
            .HasIndex(w => w.Url)
            .IsUnique();

        builder.Entity<ApplicationUserRole>()
            .HasQueryFilter(ur => ur.IsActive);
    }
}
