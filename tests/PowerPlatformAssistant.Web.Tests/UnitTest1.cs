using PowerPlatformAssistant.Web.Security;
using PowerPlatformAssistant.Web.Services.Guidance;
using PowerPlatformAssistant.Web.Services.Screenshots;

namespace PowerPlatformAssistant.Web.Tests;

public class UnitTest1
{
    [Fact]
    public void ValidateChatMessage_Rejects_EmptyInput()
    {
        var guard = new UntrustedInputGuard();

        var exception = Assert.Throws<InputGuardException>(() => guard.ValidateChatMessage("   "));

        Assert.Contains("messageText", exception.Errors.Keys);
    }

    [Fact]
    public void ValidateScreenshotMetadata_Rejects_UnsupportedContentType()
    {
        var guard = new UntrustedInputGuard();

        var exception = Assert.Throws<InputGuardException>(() => guard.ValidateScreenshotMetadata("issue.bmp", "image/bmp", 128));

        Assert.Contains("screenshotContentType", exception.Errors.Keys);
    }

    [Fact]
    public void BuildChecklist_ForBeginnerNewApp_CreatesSequentialSteps()
    {
        var onboardingService = new OnboardingService();

        var checklist = onboardingService.BuildChecklist(new Models.OnboardingState
        {
            ExperienceLevel = "beginner",
            FlowType = "new-app",
            AppContext = "Expense app"
        }, Guid.NewGuid());

        Assert.Equal("Beginner New App checklist", checklist.ChecklistTitle);
        Assert.Equal(3, checklist.Steps.Count);
        Assert.Equal(1, checklist.Steps[0].StepOrder);
        Assert.Equal("pending", checklist.Steps[0].ConfirmationStatus);
    }

    [Fact]
    public void AuthoringFlowService_IncludesPinnedNamesAndUncertainty()
    {
        var service = new AuthoringFlowService();

        var message = service.BuildSystemMessage(
            new Models.AppContext
            {
                FlowType = "new-app",
                AppName = "Inspections",
                CurrentGoal = "Create the inspection shell"
            },
            new Models.EnvironmentContext
            {
                EnvironmentType = "sandbox",
                Region = "united-states",
                HasCreationCapabilityUncertainty = true
            },
            [new Models.NamingPreference { ArtifactType = "screen", PreferredName = "scrInspectionHome" }],
            routeChanged: true);

        Assert.Contains("Inspections", message);
        Assert.Contains("scrInspectionHome", message);
        Assert.Contains("uncertain", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ScreenshotIntakeService_CreatesScopedStorageReference()
    {
        var service = new ScreenshotIntakeService();

        var result = await service.CreateAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"), new Models.DebuggingContextRequest
        {
            ScreenshotFileName = "issue.png",
            ScreenshotContentType = "image/png",
            ScreenshotFileSize = 1024,
            ScreenshotContent = Enumerable.Repeat((byte)7, 1024).ToArray(),
            VisibleIssueSummary = "Gallery is empty",
            DataSourceName = "Accounts",
            DataSourceCategory = "Dataverse",
            ResolutionStatus = "connected"
        });

        Assert.Contains("debugging/11111111111111111111111111111111/artifacts", result.ScreenshotAttachment.StorageReference);
        Assert.Equal("connected", result.DataSourceContext.ResolutionStatus);
        Assert.True(result.ScreenshotAttachment.HasStoredArtifact);
        Assert.NotEmpty(result.ScreenshotAttachment.Sha256Hash);
    }
}
