using PowerPlatformAssistant.Web.Security;
using PowerPlatformAssistant.Web.Services.Guidance;

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
}
