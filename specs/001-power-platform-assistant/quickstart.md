# Quickstart: Power Platform Assistant MVP Validation

## Purpose

This document defines the primary end-to-end validation flows for the Power Platform Assistant MVP before implementation is considered ready for broader review.

## Preconditions

- The application is running in a secure server-side environment.
- The tester is an authenticated organizational user with tenant context available.
- The assistant is configured with the current constitution, design spec, and system prompt.

## Scenario 1: First-Session Onboarding

1. Sign in as an authenticated tenant user.
2. Open the assistant for the first time.
3. Confirm the onboarding flow establishes:
   - assistant scope as Microsoft Power Platform guidance
   - user experience level
   - whether the user is working on a new app or existing app
   - tenant and environment context when needed
4. Submit a beginner-oriented request.
5. Verify the assistant responds with incremental, confirmable guidance rather than a large speculative answer.

**Expected Result**

- The first scoped assistant response is delivered within the intended onboarding flow.
- The assistant does not imply general-purpose capability.
- The assistant does not imply client-side reasoning or hidden actions.

## Scenario 2: New-App Versus Existing-App Routing

1. Start a conversation for creating a new app.
2. Observe environment-aware creation questions.
3. Restart or branch to an existing-app support request.
4. Confirm the assistant changes guidance path appropriately.
5. Mid-conversation, switch from one route to the other.

**Expected Result**

- The new-app path does not leak into debugging or modification guidance.
- The existing-app path does not return generic creation steps.
- Route changes are handled safely without losing necessary context.

## Scenario 3: Naming Customization

1. Provide custom names for an app, screen, and control.
2. Continue the conversation through at least two additional assistant turns.
3. Ask the assistant to suggest a naming variant.

**Expected Result**

- User-provided names are preserved consistently.
- Suggested names remain editable and non-authoritative.

## Scenario 4: Tenant-Aware Guidance

1. Use a request that depends on environment or tenant capability.
2. Provide partial or uncertain tenant context.
3. Observe the assistant response.

**Expected Result**

- The assistant asks clarifying questions when tenant capability is unclear.
- The assistant does not imply that features or options are universally available.

## Scenario 5: Screenshot-Assisted Debugging

1. Open a debugging conversation for an existing Power Apps issue.
2. Upload a screenshot showing the visible problem.
3. Ask for diagnosis help.
4. Repeat with a screenshot that is insufficient or ambiguous.

**Expected Result**

- The assistant treats the screenshot as untrusted input.
- The assistant grounds its response in visible evidence.
- The assistant explicitly states when the screenshot is insufficient for certainty.
- Screenshot handling remains limited to debugging and UI-review context.

## Scenario 6: Data-Source Clarification

1. Ask a debugging question that depends on app data-source details.
2. Withhold whether the data source is present, planned, or misconfigured.
3. Observe the assistant response.

**Expected Result**

- The assistant asks clarifying questions before giving dependent instructions.
- The assistant does not assume a single data-source path is correct.

## Scenario 7: Scope Boundary And Unsupported Features

1. Ask for guidance outside Microsoft Power Platform scope.
2. Ask about a Power Platform capability that is unsupported or unavailable in the tenant.

**Expected Result**

- The assistant constrains, redirects, or declines out-of-scope requests.
- The assistant does not hallucinate unsupported Power Platform features, connectors, settings, or debugging paths.

## Scenario 8: Accessibility And Status Messaging

1. Navigate the onboarding and chat experience using keyboard-only interaction.
2. Verify screen-reader-friendly labels and status updates.
3. Trigger a clarifying-question state, a checklist state, and a screenshot-upload state.

**Expected Result**

- Focus order is logical and visible.
- Status changes are announced clearly.
- Core P1 onboarding and chat flows remain usable with accessible interaction patterns.

## Constitution Review Checklist

Before implementation is considered ready for acceptance review, confirm evidence exists for:

- server-side execution only
- untrusted input handling for chat and screenshots
- Microsoft Power Platform-only scope boundaries
- tenant-aware, incremental, confirmable guidance
- no unsupported-feature hallucination
- accessibility and Section 508 expectations

## Open Operational Checks

- Access-control rules for conversation visibility and screenshot access must be validated against organizational policy.
- Retention and disposal rules for chat history and screenshots must be validated against organizational policy.
- Compliance review requirements for troubleshooting artifacts must be validated before production release.