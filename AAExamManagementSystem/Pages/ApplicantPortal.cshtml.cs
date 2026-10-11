using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages;

[Authorize(Roles = AppRoles.Applicant)]
public class ApplicantPortalModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public ApplicantPortalModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    public ApplicantDto? Applicant { get; set; }
    public int AttemptCount { get; set; }
    public AttemptListItemDto? LatestAttempt { get; set; }

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        var applicant = await _context.Applicants
            .Include(a => a.Attempts)
            .ThenInclude(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (applicant is null)
        {
            return;
        }

        Applicant = _mapper.Map<ApplicantDto>(applicant);
        AttemptCount = applicant.Attempts.Count;

        var attempt = applicant.Attempts.OrderByDescending(a => a.DateCreated).FirstOrDefault();
        if (attempt is not null)
        {
            LatestAttempt = new AttemptListItemDto
            {
                Id = attempt.Id,
                ApplicantName = $"{applicant.FirstName} {applicant.LastName}",
                StudentNo = applicant.StudentNo,
                Section = attempt.Section,
                DateTaken = attempt.DateTaken,
                TotalScore = attempt.TotalScore,
                MaxScore = attempt.Answers.Sum(ans => ans.Question.Score),
                PendingEvaluationCount = attempt.Answers.Count(ans => ans.IsCorrect == null),
                IsApproved = attempt.IsApproved,
                DateCreated = attempt.DateCreated,
            };
        }
    }
}
