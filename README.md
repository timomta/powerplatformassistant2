# powerplatformassistant2

Power Platform Assistant is a server-side Blazor application that keeps Microsoft Power Platform guidance tenant-aware, incremental, and bounded by governance rules.

## Runtime Shape

- ASP.NET Core Blazor Web App on .NET 10 with interactive server components.
- Development uses a local authentication profile so localhost testing works without Negotiate setup.
- Production authentication uses Negotiate.
- Testing uses a header-based authentication handler so integration coverage can exercise the secured API surface.
- Conversation state persists through EF Core with SQLite outside testing and in-memory storage during tests.

## Current Guided Flows

- Onboarding captures experience level, route selection, and tenant-bound guidance acknowledgement.
- Authoring mode persists new-app versus existing-app routing, app context, environment details, and naming preferences.
- Debugging mode uploads screenshot artifacts to server-side storage, persists screenshot metadata and data-source context, and keeps screenshot evidence untrusted and debugging-only.
- Assistant responses remain scoped to Microsoft Power Platform and ask clarifying questions when tenant or data-source context is incomplete.
- Accessibility hardening now includes live status updates, labeled regions, and safer focus handling across onboarding, authoring, and debugging flows.

## Local Verification

```powershell
dotnet build PowerPlatformAssistant.sln
dotnet test PowerPlatformAssistant.sln
dotnet run --project src/PowerPlatformAssistant.Web
```

The HTTP surface currently includes `/api/chat/state`, `/api/chat/onboarding`, `/api/chat/messages`, `/api/chat/authoring-context`, `/api/chat/naming-preferences`, and `/api/chat/debugging-context`.

When you run the app locally in the default Development environment, it signs you in automatically using the values in `appsettings.Development.json`. Negotiate remains the active authentication mode outside Development and Testing.

The UI also shows a visible Development banner when the local auto-sign-in profile is active so you do not confuse localhost testing with the production authentication path.

## Manual Smoke Test

1. Start the app with `dotnet run --project src/PowerPlatformAssistant.Web`, open `http://localhost:5089`, and confirm the Development banner appears above the home page.
2. Complete onboarding with any experience level and route selection, then confirm the assistant shows tenant and environment context instead of restarting the flow.
3. In authoring mode, save a `new-app` route with app and screen names, ask an authoring question, and confirm the response references the saved route and naming context.
4. Change the authoring route to `existing-app`, ask another authoring question, and confirm the guidance changes instead of continuing the `new-app` path.
5. In debugging mode, upload a small screenshot, save an issue summary plus unresolved data-source context, and confirm the screenshot review panel and clarifying-response behavior both appear.
6. Ask an out-of-scope question and confirm the assistant responds with a Microsoft Power Platform scope boundary instead of general-purpose advice.

The latest validation run passed 17 tests and recorded onboarding completion at 13.90 ms plus first assistant response latency at 144.19 ms in the test host.
