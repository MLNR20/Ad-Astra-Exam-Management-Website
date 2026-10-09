namespace AAExamManagementSystem.Models.Entities;

public static class Departments
{
    public const string CustomerSupport = "Customer Support";
    public const string WebDevelopment = "Web Development";
    public const string ContentDevelopment = "Content Development";
    public const string Photo = "Photo";
    public const string Video = "Video";
    public const string ArtAndDesign = "Art and Design";
    public const string Managing = "Managing";
    public const string Marketing = "Marketing";
    public const string Personality = "Personality";
    public const string GeneralKnowledge = "General Knowledge";

    public static readonly string[] All =
    {
        CustomerSupport, WebDevelopment, ContentDevelopment, Photo, Video, ArtAndDesign, Managing, Marketing,
        Personality, GeneralKnowledge
    };
}
