using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages;

public class SignupModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IGenericRepository<Applicant> _applicantRepository;
    private readonly IMapper _mapper;

    public SignupModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IGenericRepository<Applicant> applicantRepository,
        IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _applicantRepository = applicantRepository;
        _mapper = mapper;
    }

    [BindProperty]
    public ApplicantRegisterDto Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var existingUser = await _userManager.FindByNameAsync(Input.UserName);
        if (existingUser is not null)
        {
            ModelState.AddModelError(string.Empty, "An account with that username already exists.");
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.UserName,
            Email = Input.Email,
            EmailConfirmed = true,
            FirstName = Input.FirstName,
            LastName = Input.LastName
        };

        var createResult = await _userManager.CreateAsync(user, Input.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        var roleResult = await _userManager.AddToRoleAsync(user, AAExamManagementSystem.Models.Entities.Roles.Applicant);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty, "Failed to complete registration. Please try again.");
            return Page();
        }

        var applicant = _mapper.Map<Applicant>(Input);
        applicant.Email = Input.Email;
        applicant.UserId = user.Id;

        try
        {
            await _applicantRepository.AddAsync(applicant);
            await _applicantRepository.SaveChangesAsync();
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty, "Failed to save your application. Please try again.");
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToPage("/ApplicantPortal");
    }
}
