using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Data;

public static class IdentitySeeder
{
    private const string AdminUserName = "admin";
    private const string AdminEmail = "admin@aaexams.local";
    private const string AdminPassword = "P@ssword2026!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        foreach (var sectionName in Departments.All)
        {
            var exists = dbContext.Sections.Any(s => s.Name == sectionName);
            if (!exists)
            {
                dbContext.Sections.Add(new Section { Name = sectionName });
            }
        }

        foreach (var questionTypeName in QuestionTypes.All)
        {
            var exists = dbContext.QuestionTypes.Any(qt => qt.Name == questionTypeName);
            if (!exists)
            {
                dbContext.QuestionTypes.Add(new QuestionType { Name = questionTypeName });
            }
        }

        await dbContext.SaveChangesAsync();

        var sections = dbContext.Sections.ToList();

        var adminUser = await userManager.FindByNameAsync(AdminUserName);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminUserName,
                Email = AdminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator"
            };

            var result = await userManager.CreateAsync(adminUser, AdminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }

        await SeedDemoUsersAsync(userManager, sections);
        await SeedWebDevelopmentQuestionsAsync(dbContext);
        await SeedProgrammingLanguageIdentificationQuestionsAsync(dbContext);
        await SeedProgrammingLanguageIconQuestionsAsync(dbContext);
        await SeedSqlQueryQuestionsAsync(dbContext);
    }

    private static readonly (string QuestionTitle, string[] Choices, int CorrectIndex)[] WebDevelopmentQuestions =
    {
        ("What does HTML stand for?",
            new[] { "Hyper Text Markup Language", "High Text Machine Language", "Hyperlink and Text Markup Language", "Home Tool Markup Language" }, 0),
        ("Which CSS property is used to change the text color of an element?",
            new[] { "font-color", "text-color", "color", "background-color" }, 2),
        ("Which HTML tag is used to define an internal style sheet?",
            new[] { "<css>", "<script>", "<style>", "<link>" }, 2),
        ("In JavaScript, which keyword declares a block-scoped variable that can be reassigned?",
            new[] { "const", "let", "var", "static" }, 1),
        ("Which HTTP method is typically used to submit data to be processed to a specified resource?",
            new[] { "GET", "POST", "HEAD", "OPTIONS" }, 1),
        ("What does CSS stand for?",
            new[] { "Cascading Style Sheets", "Computer Style Sheets", "Creative Style System", "Colorful Style Sheets" }, 0),
        ("Which of the following is a JavaScript framework/library for building user interfaces?",
            new[] { "Laravel", "Django", "React", "Symfony" }, 2),
        ("Which HTML element is used to create a hyperlink?",
            new[] { "<link>", "<a>", "<href>", "<nav>" }, 1),
        ("Which status code indicates a successful HTTP response?",
            new[] { "200", "301", "404", "500" }, 0),
        ("Which CSS layout module is designed for one-dimensional layouts like rows or columns?",
            new[] { "CSS Grid", "Flexbox", "Float", "Position" }, 1)
    };

    private static async Task SeedWebDevelopmentQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.WebDevelopment);
        if (section is null)
        {
            return;
        }

        var multipleChoiceType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.MultipleChoice);
        if (multipleChoiceType is null)
        {
            return;
        }

        foreach (var (questionTitle, choices, correctIndex) in WebDevelopmentQuestions)
        {
            var exists = dbContext.Questions.Any(q => q.QuestionTitle == questionTitle && q.SectionId == section.Id);
            if (exists)
            {
                continue;
            }

            var question = new Question
            {
                QuestionTypeId = multipleChoiceType.Id,
                SectionId = section.Id,
                QuestionTitle = questionTitle,
                Score = 1
            };

            dbContext.Questions.Add(question);

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = new Choice
                {
                    ChoiceText = choices[i],
                    IsCorrect = i == correctIndex
                };

                dbContext.Choices.Add(choice);
                dbContext.QuestionAndChoices.Add(new QuestionAndChoice
                {
                    Question = question,
                    Choice = choice
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static readonly string[] ProgrammingLanguageIdentificationQuestions =
    {
        "Identify the programming language of the following snippet:\n\ndef greet(name):\n    print(f\"Hello, {name}!\")",
        "Identify the programming language of the following snippet:\n\nconst greet = (name) => console.log(`Hello, ${name}!`);",
        "Identify the programming language of the following snippet:\n\npublic class Main {\n    public static void main(String[] args) {\n        System.out.println(\"Hello\");\n    }\n}",
        "Identify the programming language of the following snippet:\n\npublic class Program {\n    static void Main() {\n        Console.WriteLine(\"Hello\");\n    }\n}",
        "Identify the programming language of the following snippet:\n\n#include <stdio.h>\nint main() {\n    printf(\"Hello\\n\");\n    return 0;\n}",
        "Identify the programming language of the following snippet:\n\n#include <iostream>\nint main() {\n    std::cout << \"Hello\" << std::endl;\n}",
        "Identify the programming language of the following snippet:\n\ndef greet(name)\n  puts \"Hello, #{name}!\"\nend",
        "Identify the programming language of the following snippet:\n\n<?php\nfunction greet($name) {\n    echo \"Hello, $name!\";\n}\n?>",
        "Identify the programming language of the following snippet:\n\npackage main\nimport \"fmt\"\nfunc main() {\n    fmt.Println(\"Hello\")\n}",
        "Identify the programming language of the following snippet:\n\nSELECT FirstName, LastName FROM Users WHERE IsActive = 1;"
    };

    private static async Task SeedProgrammingLanguageIdentificationQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.WebDevelopment);
        if (section is null)
        {
            return;
        }

        var textBasedType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.TextBased);
        if (textBasedType is null)
        {
            return;
        }

        Console.WriteLine("Seeding programming language identification questions:");

        foreach (var questionTitle in ProgrammingLanguageIdentificationQuestions)
        {
            var exists = dbContext.Questions.Any(q => q.QuestionTitle == questionTitle && q.SectionId == section.Id);
            if (exists)
            {
                Console.WriteLine($"  - (already seeded) {questionTitle.Split('\n')[0]}");
                continue;
            }

            dbContext.Questions.Add(new Question
            {
                QuestionTypeId = textBasedType.Id,
                SectionId = section.Id,
                QuestionTitle = questionTitle,
                Score = 1
            });

            Console.WriteLine($"  - {questionTitle.Split('\n')[0]}");
        }

        await dbContext.SaveChangesAsync();
    }

    private const string ProgrammingLanguageIconQuestionTitle = "Identify the programming language shown in the icon below:";

    private static readonly (string Icon, string Answer)[] ProgrammingLanguageIconQuestions =
    {
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/python/python-original.svg", "Python"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/javascript/javascript-original.svg", "JavaScript"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/java/java-original.svg", "Java"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/csharp/csharp-original.svg", "C#"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/c/c-original.svg", "C"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/cplusplus/cplusplus-original.svg", "C++"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/ruby/ruby-original.svg", "Ruby"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/php/php-original.svg", "PHP"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/go/go-original.svg", "Go"),
        ("https://cdn.jsdelivr.net/gh/devicons/devicon/icons/rust/rust-plain.svg", "Rust")
    };

    private static async Task SeedProgrammingLanguageIconQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.WebDevelopment);
        if (section is null)
        {
            return;
        }

        var identificationType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.Identification);
        if (identificationType is null)
        {
            return;
        }

        Console.WriteLine("Seeding programming language icon questions:");

        foreach (var (icon, answer) in ProgrammingLanguageIconQuestions)
        {
            var existing = await dbContext.Questions
                .Include(q => q.QuestionAndChoices)
                .ThenInclude(qc => qc.Choice)
                .FirstOrDefaultAsync(q =>
                    q.QuestionTitle == ProgrammingLanguageIconQuestionTitle && q.Image == icon && q.SectionId == section.Id);

            if (existing is not null)
            {
                if (existing.QuestionTypeId != identificationType.Id || existing.QuestionAndChoices.Count != 1)
                {
                    existing.QuestionTypeId = identificationType.Id;
                    dbContext.Choices.RemoveRange(existing.QuestionAndChoices.Select(qc => qc.Choice));
                    dbContext.QuestionAndChoices.RemoveRange(existing.QuestionAndChoices);
                    AddIdentificationAnswer(dbContext, existing, answer);
                    Console.WriteLine($"  - (converted to identification) {answer}");
                }
                else
                {
                    Console.WriteLine($"  - (already seeded) {answer}");
                }
                continue;
            }

            var question = new Question
            {
                QuestionTypeId = identificationType.Id,
                SectionId = section.Id,
                QuestionTitle = ProgrammingLanguageIconQuestionTitle,
                Image = icon,
                Score = 1
            };

            dbContext.Questions.Add(question);
            AddIdentificationAnswer(dbContext, question, answer);

            Console.WriteLine($"  - {answer}");
        }

        await dbContext.SaveChangesAsync();
    }

    private static readonly (string Image, string QuestionTitle, string Answer)[] SqlQueryQuestions =
    {
        ("/images/sql-schema-highest-score.svg",
            "Given the schema below (Staffers, Scores, Subscriptions), write the SQL query to find the name of the staffer with the highest score.",
            "SELECT s.Name FROM Staffers s JOIN Scores sc ON sc.StafferId = s.Id ORDER BY sc.Score DESC LIMIT 1;"),
        ("/images/sql-schema-subscription-count.svg",
            "Given the schema below (Staffers, Scores, Subscriptions), write the SQL query to find the total subscription count for each staffer.",
            "SELECT s.Name, COUNT(*) AS TotalSubscriptions FROM Staffers s JOIN Subscriptions sub ON sub.StafferId = s.Id GROUP BY s.Name;")
    };

    private static async Task SeedSqlQueryQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.WebDevelopment);
        if (section is null)
        {
            return;
        }

        var identificationType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.Identification);
        if (identificationType is null)
        {
            return;
        }

        foreach (var (image, questionTitle, answer) in SqlQueryQuestions)
        {
            var exists = dbContext.Questions.Any(q => q.QuestionTitle == questionTitle && q.SectionId == section.Id);
            if (exists)
            {
                continue;
            }

            var question = new Question
            {
                QuestionTypeId = identificationType.Id,
                SectionId = section.Id,
                QuestionTitle = questionTitle,
                Image = image,
                Score = 1
            };

            dbContext.Questions.Add(question);
            AddIdentificationAnswer(dbContext, question, answer);
        }

        await dbContext.SaveChangesAsync();
    }

    // Stored once per question; the simulate page compares the applicant's typed answer against
    // this choice's text using a case-insensitive comparison rather than an exact match.
    private static void AddIdentificationAnswer(ApplicationDbContext dbContext, Question question, string correctAnswer)
    {
        var choice = new Choice
        {
            ChoiceText = correctAnswer,
            IsCorrect = true
        };

        dbContext.Choices.Add(choice);
        dbContext.QuestionAndChoices.Add(new QuestionAndChoice { Question = question, Choice = choice });
    }


    private const string DemoPassword = "P@ssword2026!";

    private static readonly (string UserName, string FirstName, string LastName, string Role)[] DemoUsers =
    {
        ("jsmith", "John", "Smith", Roles.Instructor),
        ("mgarcia", "Maria", "Garcia", Roles.Instructor),
        ("achen", "Alice", "Chen", Roles.Student),
        ("bwilliams", "Ben", "Williams", Roles.Student),
        ("cjohnson", "Chris", "Johnson", Roles.Student),
        ("dlee", "Diana", "Lee", Roles.Applicant),
        ("ekim", "Ethan", "Kim", Roles.Applicant),
        ("fpatel", "Fatima", "Patel", Roles.Staffer),
        ("gnguyen", "Grace", "Nguyen", Roles.Editor),
        ("hbrown", "Henry", "Brown", Roles.Guest)
    };

    private static async Task SeedDemoUsersAsync(UserManager<ApplicationUser> userManager, IList<Section> sections)
    {
        var random = new Random();

        foreach (var (userName, firstName, lastName, role) in DemoUsers)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = $"{userName}@aaexams.local",
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    SectionId = sections.Count > 0 ? sections[random.Next(sections.Count)].Id : null
                };

                var result = await userManager.CreateAsync(user, DemoPassword);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to seed demo user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
