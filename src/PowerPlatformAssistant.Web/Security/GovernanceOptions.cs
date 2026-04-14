namespace PowerPlatformAssistant.Web.Security;

public sealed class GovernanceOptions
{
    public const string SectionName = "Governance";

    public string PolicyAuthority { get; set; } = "organizational policy authority";

    public string ConversationAccessPolicy { get; set; } = "AuthenticatedSessionOnly";

    public string ScreenshotAccessPolicy { get; set; } = "PolicyAuthorizedReviewersOnly";

    public int ChatRetentionDays { get; set; }

    public int ScreenshotRetentionDays { get; set; }

    public string DisposalWorkflow { get; set; } = "PolicyDefinedDisposalRequired";

    public bool RequireComplianceReview { get; set; } = true;
}