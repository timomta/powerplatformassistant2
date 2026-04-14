# Data Model: Power Platform Assistant MVP

## Purpose

This document defines the core entities, relationships, and lifecycle constraints needed to implement the Power Platform Assistant MVP while remaining aligned with the project constitution.

## Entities

### User Session

Represents the authenticated tenant user's active assistant session.

**Attributes**

- SessionId
- UserId
- TenantId
- StartedAt
- LastActivityAt
- ExperienceLevel
- CurrentFlowType (`onboarding`, `new-app`, `existing-app`, `debugging`)
- ScopeAcknowledged
- CurrentConversationId

**Rules**

- A user session must be associated with an authenticated organizational user.
- A user session must not imply client-side authoritative state.
- Session lifetime and retention policy are organization-defined and must be validated before production use.

### Conversation

Represents a bounded assistant conversation within a user session.

**Attributes**

- ConversationId
- SessionId
- CreatedAt
- UpdatedAt
- Status (`active`, `completed`, `abandoned`)
- ActiveStoryContext
- LastConfirmedStep

**Rules**

- A conversation belongs to exactly one user session.
- A conversation must preserve enough server-side context to support incremental, confirmable guidance.

### Conversation Turn

Represents one user-assistant exchange.

**Attributes**

- TurnId
- ConversationId
- SenderType (`user`, `assistant`, `system`)
- MessageText
- CreatedAt
- HasClarifyingQuestion
- HasChecklistOutput
- HasScopeBoundaryMessage

**Rules**

- Conversation turns must remain attributable to the current session and conversation.
- Assistant turns must not imply unsupported features, hidden actions, or unseen environment access.

### Tenant Context

Represents tenant-aware information that constrains available guidance.

**Attributes**

- TenantContextId
- TenantId
- EnvironmentId
- EnvironmentType
- Region
- LicensingSignals
- CapabilityNotes
- GovernancePolicyNotes
- LastVerifiedAt

**Rules**

- Tenant context may be partial, stale, or unresolved.
- When tenant context is incomplete, the assistant must prefer clarifying questions over assumptions.

### App Context

Represents the current Power Apps task target.

**Attributes**

- AppContextId
- ConversationId
- FlowType (`new-app`, `existing-app`)
- AppName
- AppIdentifier
- ScreenName
- CurrentGoal
- IsRouteConfirmed

**Rules**

- The route must distinguish new-app and existing-app guidance.
- Mid-conversation route changes must preserve enough state for safe redirection.

### Guidance Checklist

Represents beginner-oriented, confirmable guidance steps.

**Attributes**

- ChecklistId
- ConversationId
- ChecklistTitle
- ChecklistStatus (`active`, `completed`, `dismissed`)
- CreatedAt

### Guidance Checklist Step

Represents a single confirmable step within a checklist.

**Attributes**

- StepId
- ChecklistId
- StepOrder
- StepText
- ConfirmationStatus (`pending`, `confirmed`, `skipped`)
- ConfirmedAt

**Rules**

- Checklist steps must be sequential and user-confirmable.
- Checklist output must remain guidance, not hidden execution.

### Naming Preference

Represents user-provided naming choices that should persist across the conversation.

**Attributes**

- NamingPreferenceId
- ConversationId
- ArtifactType (`app`, `screen`, `control`, `variable`, `other`)
- PreferredName
- CreatedAt
- UpdatedAt

**Rules**

- User-provided names take precedence over assistant-suggested defaults.
- Suggested names remain editable and non-authoritative.

### Data Source Context

Represents the known or unresolved state of the user's relevant Power Apps data source.

**Attributes**

- DataSourceContextId
- ConversationId
- DataSourceName
- DataSourceCategory
- ResolutionStatus (`present`, `planned`, `unknown`, `misconfigured`)
- ClarificationNotes
- LastUpdatedAt

**Rules**

- Data-source-dependent guidance must not proceed on unresolved assumptions.
- Unknown or conflicting data-source states should trigger clarification.

### Screenshot Attachment

Represents an uploaded screenshot used for debugging context.

**Attributes**

- ScreenshotAttachmentId
- ConversationId
- UploadedAt
- OriginalFileName
- ContentType
- FileSize
- StorageReference
- ReviewStatus (`pending`, `reviewed`, `rejected`)
- RejectionReason

**Rules**

- Screenshot attachments are untrusted inputs.
- Screenshot usage is limited to debugging and UI-review context.
- Storage, retention, and disposal must follow validated organizational policy before production use.

## Relationships

- One User Session has many Conversations.
- One Conversation has many Conversation Turns.
- One Conversation may have zero or one active App Context.
- One Conversation may have many Naming Preferences.
- One Conversation may have zero or one active Guidance Checklist.
- One Guidance Checklist has many Guidance Checklist Steps.
- One Conversation may have many Screenshot Attachments.
- One Conversation may have zero or one active Data Source Context.
- One User Session may reference one or more Tenant Context snapshots over time.

## Lifecycle Notes

- User Session starts at authentication, remains server-authoritative during interaction, and ends by timeout, sign-out, or explicit completion.
- Conversation starts when the user engages the assistant, accumulates turns and clarifications, and closes when abandoned or explicitly completed.
- Screenshot Attachment begins as untrusted uploaded input, is either rejected or reviewed, and must be handled according to validated retention and disposal policy.
- Data Source Context and Tenant Context may move between unresolved and resolved states as clarifying information arrives.

## Open Validation Items

- Access-control rules for which users may view or reuse session history remain organization-defined.
- Retention and disposal requirements for chat history and screenshot metadata remain organization-defined.
- Compliance obligations for tenant data, screenshots, and troubleshooting records remain organization-defined.