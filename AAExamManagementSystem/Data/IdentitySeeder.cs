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

    private static readonly (string Name, string Url, string Icon, string Category, int DisplayOrder)[] WebPages =
    {
        ("Dashboard", "/Dashboard", "bi-grid-1x2", "Overview", 0),
        ("Applicant Portal", "/ApplicantPortal", "bi-mortarboard", "Overview", 1),
        ("Analytics", "/Analytics/Index", "bi-bar-chart-line", "Overview", 2),
        ("Departments", "/Departments/Index", "bi-building", "Academics", 3),
        ("Sections", "/Sections/Index", "bi-diagram-3", "Academics", 4),
        ("Courses", "/Courses/Index", "bi-journal-bookmark", "Academics", 5),
        ("Questions & Choices", "/Questions/Index", "bi-question-circle", "Academics", 6),
        ("Users", "/Users/Index", "bi-people", "Administration", 7),
        ("Roles", "/Roles/Index", "bi-person-badge", "Administration", 8),
        ("Role Assignments", "/RoleAssignments/Index", "bi-person-check", "Administration", 9),
        ("Audit Logs", "/AuditLogs/Index", "bi-clipboard-data", "Administration", 10),
        ("Announcements", "/Announcements/Index", "bi-megaphone", "Academics", 11)
    };

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

        foreach (var (name, url, icon, category, displayOrder) in WebPages)
        {
            var page = dbContext.WebPages.FirstOrDefault(p => p.Url == url);
            if (page is null)
            {
                dbContext.WebPages.Add(new WebPage
                {
                    Name = name,
                    Url = url,
                    Icon = icon,
                    Category = category,
                    DisplayOrder = displayOrder
                });
            }
            else
            {
                page.Name = name;
                page.Icon = icon;
                page.Category = category;
                page.DisplayOrder = displayOrder;
                page.IsActive = true;
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
        await SeedDepartmentEditorsAsync(userManager, sections);
        await SeedWebDevelopmentQuestionsAsync(dbContext);
        await SeedProgrammingLanguageIdentificationQuestionsAsync(dbContext);
        await SeedProgrammingLanguageIconQuestionsAsync(dbContext);
        await SeedSqlQueryQuestionsAsync(dbContext);
        await SeedPersonalityQuestionsAsync(dbContext);
        await SeedGeneralKnowledgeQuestionsAsync(dbContext);
        await SeedJavaScriptCodeSnippetQuestionsAsync(dbContext);
        await SeedMarketingQuestionsAsync(dbContext);
        await SeedContentDevelopmentSpellingQuestionsAsync(dbContext);
        await SeedContentDevelopmentEssayQuestionsAsync(dbContext);
        await SeedPhotoQuestionsAsync(dbContext);
        await SeedArtAndDesignQuestionsAsync(dbContext);
        await SeedVideoQuestionsAsync(dbContext);
        await SeedPhotoAdobeProductQuestionsAsync(dbContext);
        await SeedArtAndDesignAdobeProductQuestionsAsync(dbContext);
        await SeedCustomerSupportEssayQuestionsAsync(dbContext);
        await SeedManagingEssayQuestionsAsync(dbContext);
        await SeedWebDevelopmentEssayQuestionsAsync(dbContext);
    }

    private static readonly string[] PersonalityQuestions =
    {
        "What is your greatest strength?"
    };

    private static async Task SeedPersonalityQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.Personality);
        if (section is null)
        {
            return;
        }

        var essayType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.Essay);
        if (essayType is null)
        {
            return;
        }

        foreach (var questionTitle in PersonalityQuestions)
        {
            var exists = dbContext.Questions.Any(q => q.QuestionTitle == questionTitle && q.SectionId == section.Id);
            if (exists)
            {
                continue;
            }

            dbContext.Questions.Add(new Question
            {
                QuestionTypeId = essayType.Id,
                SectionId = section.Id,
                QuestionTitle = questionTitle,
                IsUpToEvaluation = true,
                Score = 1
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static readonly (string QuestionTitle, string[] Choices, int CorrectIndex)[] GeneralKnowledgeQuestions =
    {
        ("What does SPU stand for?",
            new[] { "Student Publication Unit", "Student Press Union", "Student Program Unit", "Student Publishing Union" }, 0),
        ("What is Ad Astra's official socio-cultural magazine for Frosh students called?",
            new[] { "AD ASTRA", "Embark", "BEYOND", "Benildean Yearbook" }, 1),
        ("Ad Astra, through the Student Publications Unit (SPU), falls under which office?",
            new[] { "Center for Student Life", "Center for Restorative Discipline", "Center for Counseling and Educational Psychology", "Center for Social Action" }, 0),
        ("What does \"DLS-CSB\" stand for?",
            new[] { "De La Salle-College of Saint Benilde", "De La Salle-City of Saint Benilde", "De La Salle-Center for Saint Benilde", "De La Salle-Catholic School of Benilde" }, 0),
        ("DLS-CSB is named after which De La Salle Brother and saint?",
            new[] { "Saint Benilde Romançon", "Saint John Baptist de La Salle", "Saint Miguel Febres Cordero", "Saint Mutien-Marie Wiaux" }, 0),
        ("What is the commonly used short name for De La Salle-College of Saint Benilde?",
            new[] { "Benilde", "Saint Benilde University", "CSB College", "La Salle Benilde" }, 0),
        ("In which city is DLS-CSB located?",
            new[] { "Manila", "Quezon City", "Taguig", "Makati" }, 0),
        ("What is Ad Astra's sister organization within the Student Publications Unit (SPU)?",
            new[] { "Benildean Press Corps (BPC)", "The Benildean (TB)", "Benilde Student Council", "Benilde Media Society" }, 0),
        ("What does \"BPC\" stand for?",
            new[] { "Benildean Press Corps", "Benilde Public Communications", "Benildean Publication Council", "Benilde Press Committee" }, 0),
        ("What does \"SDA\" stand for, as one of DLS-CSB's schools?",
            new[] { "School of Design and Arts", "School of Development and Arts", "School of Digital Animation", "School of Dance and Arts" }, 0),
        ("What does \"SMIT\" stand for, as one of DLS-CSB's schools?",
            new[] { "School of Management and Information Technology", "School of Media and Information Technology", "School of Marketing and Industrial Technology", "School of Management and Innovative Trade" }, 0),
        ("What does \"SHRIM\" stand for, as one of DLS-CSB's schools?",
            new[] { "School of Hotel, Restaurant and Institution Management", "School of Hospitality, Retail and International Marketing", "School of Hotel and Restaurant Industry Management", "School of Hospitality and Resort Management" }, 0),
        ("What does \"SMS\" stand for, as one of DLS-CSB's schools?",
            new[] { "School of Multidisciplinary Studies", "School of Media Studies", "School of Management Sciences", "School of Marketing Strategies" }, 0),
        ("What does \"SDEAS\" stand for, as one of DLS-CSB's schools?",
            new[] { "School of Deaf Education and Applied Studies", "School of Design Education and Applied Science", "School of Development Education and Advocacy Studies", "School of Digital Education and Applied Systems" }, 0),
        ("DLS-CSB belongs to which Lasallian network of schools in the Philippines?",
            new[] { "De La Salle Philippines (DLSP)", "Ateneo Network", "Jesuit Education Association", "Catholic Educational Association of the Philippines (CEAP)" }, 0),
        ("According to Ad Astra, what is BEYOND?",
            new[] { "The organization's online content on various social media platforms", "The College's official yearbook", "The College's official socio-cultural magazine for Frosh students", "DLS-CSB's official alumni newsletter" }, 0),
        ("Which of the following is NOT one of Ad Astra's three major publications?",
            new[] { "Benildean Press Corps (BPC)", "AD ASTRA", "EMBARK", "BEYOND" }, 0)
    };

    private static async Task SeedGeneralKnowledgeQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.GeneralKnowledge);
        if (section is null)
        {
            return;
        }

        var multipleChoiceType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.MultipleChoice);
        if (multipleChoiceType is null)
        {
            return;
        }

        foreach (var (questionTitle, choices, correctIndex) in GeneralKnowledgeQuestions)
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

    private static readonly (string QuestionTitle, string[] Choices, int CorrectIndex)[] MarketingQuestions =
    {
        ("What does SEO stand for?",
            new[] { "Search Engine Optimization", "Site Engagement Overview", "Search Experience Output", "Social Engagement Optimization" }, 0),
        ("Which social media metric measures how many people saw a post?",
            new[] { "Reach", "Engagement rate", "Click-through rate", "Conversion rate" }, 0),
        ("What is the primary goal of a call-to-action (CTA) in marketing content?",
            new[] { "To prompt the audience to take a specific action", "To entertain the audience", "To summarize the brand's history", "To list product specifications" }, 0),
        ("Which of the following best describes a 'target audience'?",
            new[] { "The specific group of people a campaign is designed to reach", "Every social media follower a brand has", "The marketing team's internal staff", "A competitor's customer base" }, 0),
        ("What does 'engagement rate' measure on social media?",
            new[] { "The level of interaction (likes, comments, shares) relative to reach or followers", "The total number of posts published", "The number of followers gained in a day", "The cost per click of an ad" }, 0)
    };

    private static async Task SeedMarketingQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.Marketing);
        if (section is null)
        {
            return;
        }

        var multipleChoiceType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.MultipleChoice);
        if (multipleChoiceType is null)
        {
            return;
        }

        foreach (var (questionTitle, choices, correctIndex) in MarketingQuestions)
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

    private static readonly (string QuestionTitle, string[] Choices, int CorrectIndex)[] PhotoQuestions =
    {
        ("Which camera setting controls the amount of light entering through the lens opening?",
            new[] { "Aperture", "Shutter Speed", "ISO", "White Balance" }, 0),
        ("What does ISO measure in photography?",
            new[] { "The sensor's sensitivity to light", "The lens focal length", "The shutter speed", "The image file size" }, 0),
        ("Which composition technique divides the frame into a 3x3 grid to guide subject placement?",
            new[] { "Rule of Thirds", "Golden Ratio", "Leading Lines", "Framing" }, 0),
        ("What file format captures unprocessed image data directly from the camera sensor?",
            new[] { "RAW", "JPEG", "PNG", "GIF" }, 0),
        ("In photography, what term describes the area of an image that appears acceptably sharp?",
            new[] { "Depth of Field", "Exposure Triangle", "White Balance", "Vignette" }, 0)
    };

    private static async Task SeedPhotoQuestionsAsync(ApplicationDbContext dbContext)
    {
        await SeedMultipleChoiceQuestionsAsync(dbContext, Departments.Photo, PhotoQuestions);
    }

    private static readonly (string QuestionTitle, string[] Choices, int CorrectIndex)[] ArtAndDesignQuestions =
    {
        ("Which color model is used for designing print materials?",
            new[] { "CMYK", "RGB", "HEX", "HSL" }, 0),
        ("What term describes the empty space around and between design elements?",
            new[] { "White Space", "Kerning", "Bleed", "Gutter" }, 0),
        ("What is the recommended file format for a scalable logo that needs to be resized without losing quality?",
            new[] { "Vector graphic (e.g., SVG/AI)", "JPEG", "PNG", "GIF" }, 0),
        ("What design principle refers to the visual weight balance between elements on a page?",
            new[] { "Balance", "Contrast", "Alignment", "Proximity" }, 0),
        ("In print design, what term refers to extending artwork past the trim edge to avoid white borders after cutting?",
            new[] { "Bleed", "Margin", "Gutter", "Crop Mark" }, 0)
    };

    private static async Task SeedArtAndDesignQuestionsAsync(ApplicationDbContext dbContext)
    {
        await SeedMultipleChoiceQuestionsAsync(dbContext, Departments.ArtAndDesign, ArtAndDesignQuestions);
    }

    private static readonly (string QuestionTitle, string[] Choices, int CorrectIndex)[] VideoQuestions =
    {
        ("What term describes the number of frames displayed per second in a video?",
            new[] { "Frame Rate", "Bit Rate", "Resolution", "Aspect Ratio" }, 0),
        ("Which editing technique alternates between two different scenes to build tension or show simultaneous action?",
            new[] { "Cross-cutting", "Jump Cut", "Match Cut", "L-Cut" }, 0),
        ("What does 'aspect ratio' refer to in video production?",
            new[] { "The proportional relationship between a video's width and height", "The number of frames per second", "The audio sampling rate", "The compression level" }, 0),
        ("What audio editing technique lets the sound from the next scene begin before the picture cuts to it?",
            new[] { "J-Cut", "L-Cut", "Jump Cut", "Match Cut" }, 0),
        ("What is the standard aspect ratio used for widescreen video content?",
            new[] { "16:9", "4:3", "1:1", "9:16" }, 0)
    };

    private static async Task SeedVideoQuestionsAsync(ApplicationDbContext dbContext)
    {
        await SeedMultipleChoiceQuestionsAsync(dbContext, Departments.Video, VideoQuestions);
    }

    private static async Task SeedMultipleChoiceQuestionsAsync(
        ApplicationDbContext dbContext,
        string departmentName,
        (string QuestionTitle, string[] Choices, int CorrectIndex)[] questionSet)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == departmentName);
        if (section is null)
        {
            return;
        }

        var multipleChoiceType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.MultipleChoice);
        if (multipleChoiceType is null)
        {
            return;
        }

        foreach (var (questionTitle, choices, correctIndex) in questionSet)
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

    private static readonly (string QuestionTitle, string Answer)[] PhotoAdobeProductQuestions =
    {
        ("Name the Adobe application most commonly used for pixel-based photo retouching and compositing.", "Photoshop"),
        ("Name the Adobe application built specifically for organizing, editing, and batch-processing RAW photos.", "Lightroom"),
        ("Name the Adobe application that lets you organize and preview your photo and media files before importing them into other apps.", "Bridge"),
        ("Name the Adobe engine that powers RAW photo processing inside Photoshop and Lightroom.", "Camera Raw"),
        ("Name the Adobe mobile app that turns your phone into a portable document and photo scanner.", "Adobe Scan"),
        ("Name the free, mobile version of Photoshop used for quick photo edits on a phone.", "Photoshop Express"),
        ("Name the Adobe cloud subscription platform that gives you access to apps like Photoshop and Lightroom.", "Creative Cloud")
    };

    private static async Task SeedPhotoAdobeProductQuestionsAsync(ApplicationDbContext dbContext)
    {
        await SeedIdentificationQuestionsAsync(dbContext, Departments.Photo, PhotoAdobeProductQuestions);
    }

    private static readonly (string QuestionTitle, string Answer)[] ArtAndDesignAdobeProductQuestions =
    {
        ("Name the Adobe application built for creating scalable vector graphics such as logos and icons.", "Illustrator"),
        ("Name the Adobe application used for multi-page layout design such as brochures, magazines, and yearbooks.", "InDesign"),
        ("Name the Adobe application used for pixel-based graphic design and photo compositing.", "Photoshop"),
        ("Name the Adobe application built for designing and prototyping user interfaces and user experiences.", "Adobe XD"),
        ("Name the Adobe application that combines raster and vector drawing for illustration on touch devices.", "Fresco"),
        ("Name the Adobe application used for creating 3D renders and photorealistic scenes from 2D designs.", "Dimension"),
        ("Name the Adobe service that gives Creative Cloud subscribers access to a library of fonts for their designs.", "Adobe Fonts"),
        ("Name the Adobe application used for editing and sharing PDF documents.", "Acrobat"),
        ("Name the Adobe application that lets you capture colors, patterns, and shapes from the real world using your phone camera to use in your designs.", "Adobe Capture"),
        ("Name the Adobe application used for video editing, often used by designers to produce promotional and social media video content.", "Premiere Pro")
    };

    private static async Task SeedArtAndDesignAdobeProductQuestionsAsync(ApplicationDbContext dbContext)
    {
        await SeedIdentificationQuestionsAsync(dbContext, Departments.ArtAndDesign, ArtAndDesignAdobeProductQuestions);
    }

    private static async Task SeedIdentificationQuestionsAsync(
        ApplicationDbContext dbContext,
        string departmentName,
        (string QuestionTitle, string Answer)[] questionSet)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == departmentName);
        if (section is null)
        {
            return;
        }

        var identificationType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.Identification);
        if (identificationType is null)
        {
            return;
        }

        foreach (var (questionTitle, answer) in questionSet)
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
                Score = 1
            };

            dbContext.Questions.Add(question);
            AddIdentificationAnswer(dbContext, question, answer);
        }

        await dbContext.SaveChangesAsync();
    }

    private static readonly string[] CustomerSupportEssayQuestions =
    {
        "Tough Scenario: An applicant is upset that they weren't accepted into their first-choice department and sends an angry message accusing Ad Astra of favoritism. How would you respond to de-escalate the situation while staying professional?",
        "Tough Scenario: A staffer repeatedly misses yearbook submission deadlines and blames the customer support team for not reminding them enough. How would you handle this recurring issue?",
        "Email Drafting: Draft a short, professional email informing an applicant that their application has moved forward to the next round of the selection process.",
        "How would you handle a situation where you don't know the answer to a question an applicant is asking you?",
        "What steps would you take to ensure a satisfying applicant experience from first contact to resolution?",
        "Describe how you would prioritize multiple support requests that come in at the same time.",
        "How would you communicate a delay in the release of exam results to a group of anxious applicants?",
        "What would you do if you received negative feedback about Ad Astra's customer support on social media?",
        "How do you handle a customer support request that falls outside your department's scope?",
        "Describe your approach to following up with an applicant after resolving their concern."
    };

    private static async Task SeedCustomerSupportEssayQuestionsAsync(ApplicationDbContext dbContext)
    {
        await SeedEssayQuestionsAsync(dbContext, Departments.CustomerSupport, CustomerSupportEssayQuestions, score: 10);
    }

    private static readonly string[] ManagingEssayQuestions =
    {
        "Conflict Resolution: Two staffers under your supervision disagree strongly on the creative direction of a feature spread, and the disagreement is affecting team morale. How would you resolve this conflict?",
        "Policy Issue: A staffer violates Ad Astra's confidentiality policy by sharing unpublished yearbook content outside the organization. How would you handle this policy violation?",
        "Scheduling: Two major deadlines, a photo shoot and a layout submission, fall on the same week, and your team is short-staffed. How would you manage the schedule to meet both deadlines?",
        "How would you motivate a team member who seems disengaged from their tasks?",
        "Describe how you would onboard a new staffer into your department.",
        "How do you balance maintaining quality standards with meeting tight deadlines?",
        "What would you do if a team member consistently underperforms despite feedback?",
        "How would you handle a situation where a decision you made is unpopular with your team?",
        "Describe your approach to delegating tasks effectively across a team with different skill levels.",
        "How would you evaluate whether your department met its goals for an academic year?"
    };

    private static async Task SeedManagingEssayQuestionsAsync(ApplicationDbContext dbContext)
    {
        await SeedEssayQuestionsAsync(dbContext, Departments.Managing, ManagingEssayQuestions, score: 10);
    }

    private static readonly string[] WebDevelopmentEssayQuestions =
    {
        "Done is better than perfect?",
        "How do you tell your editor what features to prioritize and work on?"
    };

    private static async Task SeedWebDevelopmentEssayQuestionsAsync(ApplicationDbContext dbContext)
    {
        await SeedEssayQuestionsAsync(dbContext, Departments.WebDevelopment, WebDevelopmentEssayQuestions, score: 1);
    }

    private static async Task SeedEssayQuestionsAsync(
        ApplicationDbContext dbContext,
        string departmentName,
        string[] questionTitles,
        int score)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == departmentName);
        if (section is null)
        {
            return;
        }

        var essayType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.Essay);
        if (essayType is null)
        {
            return;
        }

        foreach (var questionTitle in questionTitles)
        {
            var exists = dbContext.Questions.Any(q => q.QuestionTitle == questionTitle && q.SectionId == section.Id);
            if (exists)
            {
                continue;
            }

            dbContext.Questions.Add(new Question
            {
                QuestionTypeId = essayType.Id,
                SectionId = section.Id,
                QuestionTitle = questionTitle,
                IsUpToEvaluation = true,
                Score = score
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static readonly (string QuestionTitle, string[] Choices, int CorrectIndex)[] ContentDevelopmentSpellingQuestions =
    {
        ("Which spelling is correct for the word meaning 'absolutely required'?",
            new[] { "necessary", "neccessary", "necesary", "naccessary" }, 0),
        ("Which spelling is correct for the word meaning 'an event or instance of something happening'?",
            new[] { "occurrence", "occurence", "occurrance", "ocurrence" }, 0),
        ("Which spelling is correct for the word meaning 'without doubt'?",
            new[] { "definitely", "definately", "definitly", "defiantly" }, 0),
        ("Which spelling is correct for the word meaning 'set apart from each other'?",
            new[] { "separate", "seperate", "saparate", "seperrate" }, 0),
        ("Which spelling is correct for the word meaning 'to provide lodging or make room for'?",
            new[] { "accommodate", "accomodate", "acommodate", "accomadate" }, 0),
        ("Which spelling is correct for the word meaning 'to make someone feel ashamed'?",
            new[] { "embarrass", "embarass", "embarras", "imbarrass" }, 0),
        ("Which spelling is correct for the word meaning 'in a manner observable by others'?",
            new[] { "publicly", "publically", "publicaly", "publickly" }, 0),
        ("Which spelling is correct for the word describing a strong, regular, repeated pattern of sound?",
            new[] { "rhythm", "rythm", "rhythem", "rhytm" }, 0),
        ("Which spelling is correct for the word meaning 'careful and thorough in one's work'?",
            new[] { "conscientious", "consciencious", "conscientous", "conscientius" }, 0),
        ("Which spelling is correct for the word meaning 'a set of printed questions for gathering information'?",
            new[] { "questionnaire", "questionaire", "questionnairre", "questionare" }, 0)
    };

    private static async Task SeedContentDevelopmentSpellingQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.ContentDevelopment);
        if (section is null)
        {
            return;
        }

        var multipleChoiceType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.MultipleChoice);
        if (multipleChoiceType is null)
        {
            return;
        }

        foreach (var (questionTitle, choices, correctIndex) in ContentDevelopmentSpellingQuestions)
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

    private static readonly string[] ContentDevelopmentEssayQuestions =
    {
        "Write a short content brief (goal, target audience, key message, and format) for an upcoming Ad Astra feature article.",
        "How would you pitch a new content concept to the editorial team? Describe your approach."
    };

    private static async Task SeedContentDevelopmentEssayQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.ContentDevelopment);
        if (section is null)
        {
            return;
        }

        var essayType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.Essay);
        if (essayType is null)
        {
            return;
        }

        foreach (var questionTitle in ContentDevelopmentEssayQuestions)
        {
            var exists = dbContext.Questions.Any(q => q.QuestionTitle == questionTitle && q.SectionId == section.Id);
            if (exists)
            {
                continue;
            }

            dbContext.Questions.Add(new Question
            {
                QuestionTypeId = essayType.Id,
                SectionId = section.Id,
                QuestionTitle = questionTitle,
                IsUpToEvaluation = true,
                Score = 1
            });
        }

        await dbContext.SaveChangesAsync();
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

    private static readonly string[] JavaScriptCodeSnippetQuestions =
    {
        "Basic Challenge: The following JavaScript function should return the sum of two numbers, but it has a bug. Identify the bug and provide the corrected code.\n\nfunction add(a, b) {\n  return a - b;\n}",
        "Basic Challenge: The following JavaScript loop should print the numbers 1 to 5, but it has a bug. Identify the bug and provide the corrected code.\n\nfor (let i = 1; i <= 5; i--) {\n  console.log(i);\n}"
    };

    private static async Task SeedJavaScriptCodeSnippetQuestionsAsync(ApplicationDbContext dbContext)
    {
        var section = dbContext.Sections.FirstOrDefault(s => s.Name == Departments.WebDevelopment);
        if (section is null)
        {
            return;
        }

        var essayType = dbContext.QuestionTypes.FirstOrDefault(qt => qt.Name == QuestionTypes.Essay);
        if (essayType is null)
        {
            return;
        }

        Console.WriteLine("Seeding JavaScript code snippet challenge questions:");

        foreach (var questionTitle in JavaScriptCodeSnippetQuestions)
        {
            var exists = dbContext.Questions.Any(q => q.QuestionTitle == questionTitle && q.SectionId == section.Id);
            if (exists)
            {
                Console.WriteLine($"  - (already seeded) {questionTitle.Split('\n')[0]}");
                continue;
            }

            dbContext.Questions.Add(new Question
            {
                QuestionTypeId = essayType.Id,
                SectionId = section.Id,
                QuestionTitle = questionTitle,
                IsUpToEvaluation = true,
                Score = 1
            });

            Console.WriteLine($"  - {questionTitle.Split('\n')[0]}");
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

    private static readonly (string UserName, string FirstName, string LastName, string DepartmentName)[] DepartmentEditors =
    {
        ("wdeditor", "Noah", "Reyes", Departments.WebDevelopment),
        ("adeditor", "Mika", "Santos", Departments.ArtAndDesign)
    };

    private static async Task SeedDepartmentEditorsAsync(UserManager<ApplicationUser> userManager, IList<Section> sections)
    {
        foreach (var (userName, firstName, lastName, departmentName) in DepartmentEditors)
        {
            var section = sections.FirstOrDefault(s => s.Name == departmentName);

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
                    SectionId = section?.Id
                };

                var result = await userManager.CreateAsync(user, DemoPassword);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to seed '{departmentName}' editor user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else if (user.SectionId != section?.Id)
            {
                user.SectionId = section?.Id;
                await userManager.UpdateAsync(user);
            }

            if (!await userManager.IsInRoleAsync(user, Roles.Editor))
            {
                await userManager.AddToRoleAsync(user, Roles.Editor);
            }
        }
    }
}
