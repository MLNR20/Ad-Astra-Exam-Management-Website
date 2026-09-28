using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.AuditLogs;

[Authorize(Roles = AppRoles.Admin)]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public IndexModel(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public IList<AuditLogDto> AuditLogs { get; set; } = new List<AuditLogDto>();

    public async Task OnGetAsync()
    {
        var auditLogs = await _db.AuditLogs
            .Include(a => a.CreatedBy)
            .OrderByDescending(a => a.DateCreated)
            .ToListAsync();

        AuditLogs = _mapper.Map<IList<AuditLogDto>>(auditLogs);
    }
}
