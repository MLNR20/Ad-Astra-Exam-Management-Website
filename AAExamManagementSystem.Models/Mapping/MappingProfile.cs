using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AutoMapper;

namespace AAExamManagementSystem.Models.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Department, DepartmentDto>();
        CreateMap<DepartmentCreateUpdateDto, Department>();

        CreateMap<Announcement, AnnouncementDto>();
        CreateMap<AnnouncementCreateUpdateDto, Announcement>();

        CreateMap<Course, CourseDto>();
        CreateMap<CourseCreateUpdateDto, Course>();

        CreateMap<Section, SectionDto>();
        CreateMap<SectionCreateUpdateDto, Section>();

        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.QuestionTypeName, opt => opt.Ignore())
            .ForMember(dest => dest.SectionName, opt => opt.Ignore())
            .ForMember(dest => dest.Choices, opt => opt.Ignore());
        CreateMap<QuestionCreateUpdateDto, Question>();

        CreateMap<Choice, ChoiceDto>();
        CreateMap<ChoiceCreateUpdateDto, Choice>();

        CreateMap<ApplicationRole, RoleDto>();

        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.SectionName, opt => opt.Ignore());

        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                src.CreatedBy == null ? null : $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}".Trim()));

        CreateMap<Notification, NotificationDto>();

        CreateMap<WebPage, WebPageDto>();

        CreateMap<Applicant, ApplicantDto>();
        CreateMap<ApplicantRegisterDto, Applicant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
            .ForMember(dest => dest.DateUpdated, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Attempts, opt => opt.Ignore())
            .ForMember(dest => dest.Answers, opt => opt.Ignore());
    }
}
