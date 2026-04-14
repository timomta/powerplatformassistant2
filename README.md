# powerplatformassistant2

Power Platform Assistant is a server-side Blazor application that keeps Microsoft Power Platform guidance tenant-aware, incremental, and bounded by governance rules.

## Runtime Shape

- ASP.NET Core Blazor Web App on .NET 10 with interactive server components.
- Production authentication uses Negotiate.
- Testing uses a header-based authentication handler so integration coverage can exercise the secured API surface.
- Conversation state persists through EF Core with SQLite outside testing and in-memory storage during tests.

## Current Guided Flows

- Onboarding captures experience level, route selection, and tenant-bound guidance acknowledgement.
- Authoring mode persists new-app versus existing-app routing, app context, environment details, and naming preferences.
- Debugging mode persists screenshot metadata and data-source context while treating screenshot evidence as untrusted and debugging-only.
- Assistant responses remain scoped to Microsoft Power Platform and ask clarifying questions when tenant or data-source context is incomplete.

## Local Verification

```powershell
dotnet build PowerPlatformAssistant.sln
dotnet test PowerPlatformAssistant.sln
```

The HTTP surface currently includes `/api/chat/state`, `/api/chat/onboarding`, `/api/chat/messages`, `/api/chat/authoring-context`, `/api/chat/naming-preferences`, and `/api/chat/debugging-context`.
