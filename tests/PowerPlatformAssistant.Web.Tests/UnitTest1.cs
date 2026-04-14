using PowerPlatformAssistant.Web.Security;

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
}
