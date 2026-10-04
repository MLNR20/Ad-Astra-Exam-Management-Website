using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Repository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class WebPagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public WebPagesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WebPageDto>>> GetAll()
    {
        var pages = await _context.WebPages
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<WebPageDto>>(pages));
    }
}
