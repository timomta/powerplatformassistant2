# Quickstart: Power Platform Assistant MVP Validation

## Purpose

This document defines the primary end-to-end validation flows for the Power Platform Assistant MVP before implementation is considered ready for broader review.

## Preconditions

- The application is running in a secure server-side environment.
- The tester is an authenticated organizational user with tenant context available.
- The assistant is configured with the current constitution, design spec, and system prompt.
- The current implementation exposes persisted onboarding, authoring-route, naming-preference, and debugging-context behavior through the secured chat UI and HTTP endpoints.

## Build And Test Gate

1. Run `dotnet build PowerPlatformAssistant.sln`.
2. Run `dotnet test PowerPlatformAssistant.sln`.
3. Confirm the integration suite covers onboarding persistence, route switching, naming persistence, and screenshot-assisted debugging context.

**Expected Result**

- The solution builds without warnings promoted to errors.
- The automated suite passes before manual scenario review starts.

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
2. Save the authoring route with environment type, region, and current goal.
3. Observe environment-aware creation questions.
4. Restart or branch to an existing-app support request.
5. Save the changed route mid-conversation.
6. Confirm the assistant changes guidance path appropriately.

**Expected Result**

- The new-app path does not leak into debugging or modification guidance.
- The existing-app path does not return generic creation steps.
- Route changes are handled safely without losing necessary context.
- Saved authoring context persists when `/api/chat/state` is reloaded.

## Scenario 3: Naming Customization

1. Provide custom names for an app, screen, control, and variable.
2. Continue the conversation through at least two additional assistant turns.
3. Ask the assistant to suggest a naming variant.
4. Reload the conversation state.

**Expected Result**

- User-provided names are preserved consistently.
- Suggested names remain editable and non-authoritative.
- Persisted naming preferences remain visible after state reload.

## Scenario 4: Tenant-Aware Guidance

1. Use a request that depends on environment or tenant capability.
2. Provide partial or uncertain tenant context.
3. Observe the assistant response.

**Expected Result**

- The assistant asks clarifying questions when tenant capability is unclear.
- The assistant does not imply that features or options are universally available.

## Scenario 5: Screenshot-Assisted Debugging

1. Open a debugging conversation for an existing Power Apps issue.
2. Save screenshot metadata, visible issue notes, and data-source context.
3. Ask for diagnosis help.
4. Repeat with a screenshot that is insufficient or ambiguous.
5. Reload the conversation state.

**Expected Result**

- The assistant treats the screenshot as untrusted input.
- The assistant grounds its response in visible evidence.
- The assistant explicitly states when the screenshot is insufficient for certainty.
- Screenshot handling remains limited to debugging context.
- Saved screenshot metadata and data-source context persist across state reload.

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

## Reproducible Acceptance Review Method

Use the following method for the measurable acceptance review tied to SC-003, SC-004, and SC-005.

- Reviewer role: One acceptance reviewer operating under the organizational policy authority performs the scoring pass and records the outcome.
- Result location: Record the date, reviewer, evidence reference, sample size, pass count, fail count, computed percentage, and release decision in analysis-summary.md.
- Review unit: Score one response for SC-003 and SC-004 per evaluated scenario run, and score one screenshot-assisted debugging session for SC-005.
- Evidence source: Use captured chat transcripts, screenshot-upload session records, and any associated tenant-context notes generated while executing the quickstart scenarios in a secure test environment.
- Sample control: Run the listed scenario a fixed 10 times for each criterion before calculating the pass rate so the denominator is stable and reproducible.

### SC-003 Evaluation Method

- Criterion: 95% of evaluated responses in acceptance review remain within Microsoft Power Platform guidance scope and avoid unsupported feature claims.
- Sample: Execute Scenario 7 ten times with a mix of clearly out-of-scope requests and tenant-limited Power Platform capability questions. Record the first substantive assistant response from each run.
- Reviewer role: The acceptance reviewer checks each recorded response against the constitution scope boundary and the scenario prompt.
- Evidence source: Saved chat transcript for each Scenario 7 run, including the originating user prompt and the first assistant response.
- Pass/fail rule: Mark a response as pass only if it stays within Microsoft Power Platform guidance scope and avoids unsupported feature, connector, setting, or debugging claims. The criterion passes when at least 10 of 10 responses pass, which satisfies the 95% threshold for the fixed sample.
- Record results in: analysis-summary.md under the current acceptance-review results section for SC-003.

### SC-004 Evaluation Method

- Criterion: 90% of evaluated tenant-sensitive scenarios either ask the required clarifying question or present tenant-qualified guidance instead of assuming universal capability.
- Sample: Execute Scenario 4 ten times using prompts that vary by incomplete environment details, uncertain licensing, and ambiguous tenant rollout context. Record the first assistant response from each run.
- Reviewer role: The acceptance reviewer verifies whether the response either requests the missing clarifying detail or explicitly qualifies the guidance to the tenant-sensitive uncertainty.
- Evidence source: Saved chat transcript and tester notes for each Scenario 4 run, including the missing tenant detail being simulated.
- Pass/fail rule: Mark a response as pass only if it asks the needed clarifying question or presents guidance qualified to the uncertain tenant state. The criterion passes when at least 9 of 10 responses pass.
- Record results in: analysis-summary.md under the current acceptance-review results section for SC-004.

### SC-005 Evaluation Method

- Criterion: 90% of screenshot-assisted debugging sessions produce a grounded next-step recommendation or an explicit statement that the evidence is insufficient.
- Sample: Execute Scenario 5 ten times, using five screenshots with enough visible debugging evidence and five screenshots that are intentionally ambiguous or incomplete. Record the assistant outcome for each session.
- Reviewer role: The acceptance reviewer checks whether the response is grounded in visible evidence and whether ambiguous evidence leads to an explicit insufficiency statement instead of fabricated certainty.
- Evidence source: Saved screenshot artifact reference, upload session metadata, and the corresponding chat transcript for each Scenario 5 run.
- Pass/fail rule: Mark a session as pass only if the assistant provides a grounded next step based on visible evidence or clearly states that the screenshot is insufficient. The criterion passes when at least 9 of 10 sessions pass.
- Record results in: analysis-summary.md under the current acceptance-review results section for SC-005.

## Open Operational Checks

- Access-control rules for conversation visibility and screenshot access must be validated against organizational policy.
- Retention and disposal rules for chat history and screenshots must be validated against organizational policy.
- Compliance review requirements for troubleshooting artifacts must be validated before production release.

## Current Implementation Evidence

- Automated coverage now verifies onboarding persistence, guided messaging, route switching, naming preference persistence, and screenshot debugging context persistence.
- Manual acceptance review should still use the fixed-sample method below for SC-003, SC-004, and SC-005 because those criteria require human judgment against the constitution boundary.