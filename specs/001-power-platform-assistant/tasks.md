# Tasks: Power Platform Assistant MVP

**Input**: Design documents from `/specs/001-power-platform-assistant/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), data-model.md, quickstart.md

**Tests**: No TDD-first task set is required by the current specification, but targeted validation and evidence-producing test tasks are included where they are needed to satisfy constitution and acceptance requirements.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the baseline solution and repository structure for the Blazor Server MVP.

- [ ] T001 Create the solution file at c:\Users\tomta\source\repos\powerplatformassistant2\PowerPlatformAssistant.sln
- [ ] T002 Create the Blazor Server web project at c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\PowerPlatformAssistant.Web.csproj
- [ ] T003 [P] Create the unit test project at c:\Users\tomta\source\repos\powerplatformassistant2\tests\PowerPlatformAssistant.Web.Tests\PowerPlatformAssistant.Web.Tests.csproj
- [ ] T004 [P] Create the integration test project at c:\Users\tomta\source\repos\powerplatformassistant2\tests\PowerPlatformAssistant.Web.IntegrationTests\PowerPlatformAssistant.Web.IntegrationTests.csproj
- [ ] T005 [P] Add shared build properties and repository-wide settings in c:\Users\tomta\source\repos\powerplatformassistant2\Directory.Build.props
- [ ] T006 [P] Establish the initial source folders under c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\ and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build the shared server-side shell, tenant-aware context plumbing, prompt governance, and security/compliance baseline required by all user stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T007 Configure authenticated tenant-user application startup in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Program.cs
- [ ] T008 [P] Create shared conversation and session models in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\ConversationTurn.cs and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\UserSession.cs
- [ ] T009 [P] Implement tenant context resolution services in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Tenant\TenantContextService.cs
- [ ] T010 [P] Implement prompt-governance loading for the constitution, design spec, and system prompt in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Prompts\PromptCompositionService.cs
- [ ] T011 [P] Implement untrusted-input validation for chat and screenshot submissions in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Security\UntrustedInputGuard.cs
- [ ] T012 Implement the shared server-side conversation orchestration service in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Chat\ConversationService.cs
- [ ] T013 Implement the reusable chat shell components in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Chat\ConversationPanel.razor and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Chat\AssistantMessage.razor
- [ ] T014 Capture access-control, retention, and compliance decisions in c:\Users\tomta\source\repos\powerplatformassistant2\specs\001-power-platform-assistant\plan.md and c:\Users\tomta\source\repos\powerplatformassistant2\specs\001-power-platform-assistant\data-model.md
- [ ] T015 [P] Implement governance and retention configuration models in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Security\GovernanceOptions.cs
- [ ] T016 [P] Add foundational integration coverage for authenticated startup, scope enforcement, and untrusted-input handling in c:\Users\tomta\source\repos\powerplatformassistant2\tests\PowerPlatformAssistant.Web.IntegrationTests\FoundationalFlowTests.cs

**Checkpoint**: Foundation ready; user story implementation can begin.

---

## Phase 3: User Story 1 - Guided Chat Onboarding (Priority: P1) 🎯 MVP

**Goal**: Deliver a secure, real-time onboarding and chat entry flow that establishes experience level, tenant context, and scope-safe assistant behavior.

**Independent Test**: Sign in as a tenant user, complete onboarding, choose a guidance path, and receive a server-side assistant response that is scoped to Microsoft Power Platform guidance and presented as incremental, confirmable help.

- [ ] T017 [P] [US1] Create onboarding and checklist models in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\OnboardingState.cs and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\GuidanceChecklist.cs
- [ ] T018 [P] [US1] Build onboarding UI components in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Onboarding\ExperienceLevelSelector.razor and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Onboarding\OnboardingFlow.razor
- [ ] T019 [US1] Implement onboarding orchestration and experience-level adaptation in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Guidance\OnboardingService.cs
- [ ] T020 [US1] Implement scoped assistant response generation and clarifying-question behavior in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Chat\AssistantResponseService.cs
- [ ] T021 [US1] Wire the onboarding flow into the real-time chat entry experience in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Chat\ConversationPanel.razor
- [ ] T022 [US1] Add explicit scope-boundary and uncertainty messaging states in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Chat\AssistantMessage.razor
- [ ] T023 [US1] Handle stale and contradictory tenant or environment context in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Tenant\TenantContextRefreshService.cs
- [ ] T024 [US1] Add onboarding completion and first-response evidence coverage in c:\Users\tomta\source\repos\powerplatformassistant2\tests\PowerPlatformAssistant.Web.IntegrationTests\OnboardingExperienceTests.cs

**Checkpoint**: User Story 1 is independently functional and demoable as the MVP.

---

## Phase 4: User Story 2 - Guided Power Apps Authoring (Priority: P2)

**Goal**: Route users through new-app versus existing-app authoring help, preserve naming choices, and account for environment-specific creation options.

**Independent Test**: Run both a new-app and existing-app conversation path, verify that the assistant asks environment-aware clarifying questions when needed, and confirm that user-provided names are preserved across later responses.

- [ ] T025 [P] [US2] Create authoring-flow models in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\AppContext.cs, c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\EnvironmentContext.cs, and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\NamingPreference.cs
- [ ] T026 [P] [US2] Build authoring routing components in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Authoring\NewOrExistingAppFlow.razor and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Authoring\NamingPreferencesPanel.razor
- [ ] T027 [US2] Implement new-versus-existing-app routing and environment-aware guidance in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Guidance\AuthoringFlowService.cs
- [ ] T028 [US2] Implement naming customization persistence in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Naming\NamingPreferenceService.cs
- [ ] T029 [US2] Integrate authoring routing and naming behavior into the active conversation flow in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Chat\ConversationPanel.razor
- [ ] T030 [US2] Handle mid-conversation route switching between new-app and existing-app flows in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Guidance\AppRouteTransitionService.cs

**Checkpoint**: User Story 2 is independently functional and can be demonstrated on top of the P1 MVP.

---

## Phase 5: User Story 3 - Screenshot-Assisted Debugging (Priority: P3)

**Goal**: Support screenshot-assisted debugging and data-source clarification while keeping user inputs untrusted and guidance grounded in visible evidence.

**Independent Test**: Upload a screenshot for a Power Apps issue, verify that the assistant uses only bounded visual evidence, asks data-source clarifying questions before assuming context, and presents tenant-aware debugging guidance.

- [ ] T031 [P] [US3] Create debugging-domain models in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\ScreenshotAttachment.cs and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Models\DataSourceContext.cs
- [ ] T032 [P] [US3] Build screenshot upload and review UI components in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Debugging\ScreenshotUploadPanel.razor and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Debugging\ScreenshotReviewPanel.razor
- [ ] T033 [US3] Implement secure screenshot intake and metadata handling in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Screenshots\ScreenshotIntakeService.cs
- [ ] T034 [US3] Implement data-source clarification and debugging guidance orchestration in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Services\Guidance\DebuggingGuidanceService.cs
- [ ] T035 [US3] Integrate screenshot-assisted debugging into the shared conversation experience in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Chat\ConversationPanel.razor

**Checkpoint**: User Story 3 is independently functional and can be validated without changing the earlier authoring flow.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalize accessibility, documentation, acceptance evidence, and constitution review across all delivered stories.

- [ ] T036 [P] Harden accessibility semantics, focus handling, and status messaging in c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Chat\ConversationPanel.razor, c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Onboarding\OnboardingFlow.razor, c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Authoring\NewOrExistingAppFlow.razor, and c:\Users\tomta\source\repos\powerplatformassistant2\src\PowerPlatformAssistant.Web\Components\Debugging\ScreenshotUploadPanel.razor
- [ ] T037 [P] Document MVP setup and validation flows in c:\Users\tomta\source\repos\powerplatformassistant2\README.md and c:\Users\tomta\source\repos\powerplatformassistant2\specs\001-power-platform-assistant\quickstart.md
- [ ] T038 Produce constitution compliance review evidence in c:\Users\tomta\source\repos\powerplatformassistant2\specs\001-power-platform-assistant\analysis-summary.md and c:\Users\tomta\source\repos\powerplatformassistant2\specs\001-power-platform-assistant\quickstart.md
- [ ] T039 Measure onboarding completion timing and first-response latency evidence in c:\Users\tomta\source\repos\powerplatformassistant2\tests\PowerPlatformAssistant.Web.IntegrationTests\PerformanceEvidenceTests.cs and c:\Users\tomta\source\repos\powerplatformassistant2\specs\001-power-platform-assistant\quickstart.md
- [ ] T040 Validate the full acceptance path and record follow-up issues in c:\Users\tomta\source\repos\powerplatformassistant2\specs\001-power-platform-assistant\quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; starts immediately.
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all story work.
- **User Story 1 (Phase 3)**: Depends on Foundational completion.
- **User Story 2 (Phase 4)**: Depends on Foundational completion and uses the chat baseline established in User Story 1.
- **User Story 3 (Phase 5)**: Depends on Foundational completion and uses the shared conversation shell established earlier.
- **Polish (Phase 6)**: Depends on the selected user stories being complete.

### User Story Dependencies

- **US1**: No dependency on other user stories; this is the MVP slice.
- **US2**: Depends on the shared chat and onboarding baseline but remains independently testable once integrated.
- **US3**: Depends on the shared chat baseline but remains independently testable once integrated.

### Within Each User Story

- Models before services.
- Services before UI integration.
- Shared conversation files touched by multiple stories must be edited sequentially.
- Evidence-producing validation tasks follow the implementation tasks they verify.

### Parallel Opportunities

- Setup tasks `T003`, `T004`, `T005`, and `T006` can run in parallel after `T001` and `T002` establish the solution baseline.
- Foundational tasks `T008`, `T009`, `T010`, `T011`, `T015`, and `T016` can run in parallel once startup wiring begins.
- In **US1**, tasks `T017` and `T018` can run in parallel.
- In **US2**, tasks `T025` and `T026` can run in parallel.
- In **US3**, tasks `T031` and `T032` can run in parallel.
- In the final phase, tasks `T036`, `T037`, and `T038` can run in parallel.

---

## Parallel Example: User Story 1

```text
Task: "Create onboarding and checklist models in ...\Models\OnboardingState.cs and ...\Models\GuidanceChecklist.cs"
Task: "Build onboarding UI components in ...\Components\Onboarding\ExperienceLevelSelector.razor and ...\Components\Onboarding\OnboardingFlow.razor"
```

## Parallel Example: User Story 2

```text
Task: "Create authoring-flow models in ...\Models\AppContext.cs, ...\Models\EnvironmentContext.cs, and ...\Models\NamingPreference.cs"
Task: "Build authoring routing components in ...\Components\Authoring\NewOrExistingAppFlow.razor and ...\Components\Authoring\NamingPreferencesPanel.razor"
```

## Parallel Example: User Story 3

```text
Task: "Create debugging-domain models in ...\Models\ScreenshotAttachment.cs and ...\Models\DataSourceContext.cs"
Task: "Build screenshot upload and review UI components in ...\Components\Debugging\ScreenshotUploadPanel.razor and ...\Components\Debugging\ScreenshotReviewPanel.razor"
```

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete User Story 1.
3. Validate onboarding, tenant-aware scope framing, stale-context handling, and the first real-time assistant response.
4. Stop here if only the MVP slice is needed.

### Incremental Delivery

1. Deliver US1 as the first usable conversational slice.
2. Add US2 to unlock guided authoring support, including route-transition safety.
3. Add US3 to unlock screenshot-assisted debugging.
4. Finish with accessibility, constitution evidence, and operational validation.

### Suggested MVP Scope

User Story 1 only.