# Application Design Spec

## Purpose

This document defines the required user experience model and core interaction design for the Power Platform Assistant application. It describes non-implementation product behavior for human review and refinement.

## User Personas And Experience-Level Model

The application must support users across a clear experience spectrum rather than assume a single skill level.

- Beginner users need guided, low-risk help, explicit terminology support, and reassurance through small, verifiable steps.
- Intermediate users need faster navigation, selective explanation depth, and practical troubleshooting support.
- Advanced users need concise, high-signal guidance, direct debugging help, and minimal friction when they already know the platform context.

The experience model must let the assistant adapt the depth and structure of guidance to the user's apparent experience level without changing the assistant's domain scope.

## Onboarding Flow And Experience Scale

The onboarding flow must establish the user's likely experience level early and use that signal to shape the initial guidance style.

Onboarding must:

- orient the user to the assistant's Microsoft Power Platform scope
- identify whether the user is beginning from scratch or working from an existing app
- establish the user's environment context when relevant
- set expectations that guidance will be incremental and confirmable

The experience scale must be simple enough for users to understand and stable enough to shape subsequent guidance. It must avoid labeling that feels punitive or exclusionary.

## Checklist-Driven Beginner Guidance

Beginner guidance must be checklist-driven. When the assistant identifies a beginner-oriented scenario, it must present work as a sequence of clear, ordered steps that the user can confirm one by one.

Checklist guidance must:

- reduce ambiguity about what to do next
- make progress visible
- allow the user to stop, resume, or ask for clarification at any step
- avoid assuming hidden prerequisites or prior Power Platform knowledge

## New Vs Existing App Decision Flow

The application must include an explicit decision flow that distinguishes between creating a new app and working on an existing one.

This flow must:

- help the user choose the correct starting path
- avoid mixing creation guidance with debugging or modification guidance
- collect the minimum context needed to continue safely
- redirect the conversation when the user changes paths midstream

## Environment-Specific Creation Options

App creation guidance must account for environment-specific options when those options materially affect what the user can do.

The design must ensure that:

- the assistant asks for environment context when needed
- environment-specific creation choices are surfaced clearly
- the assistant does not imply that creation options are identical across environments
- uncertainty about environment capabilities results in clarifying questions rather than assumption

## Screenshot-Assisted UI Review

The application must support screenshot-assisted UI review for debugging and authoring feedback.

This experience must:

- let users provide screenshots as optional troubleshooting context
- treat screenshots as untrusted user input
- use screenshots to ground discussion of visible UI issues, layout problems, and authoring observations
- avoid implying certainty when the screenshot does not provide enough evidence

Screenshot-assisted review must remain subordinate to the assistant's Microsoft Power Platform guidance scope.

## Data-Source Clarification Logic

The application must include data-source clarification logic before giving guidance that depends on the user's app data model or connection setup.

This logic must:

- determine whether a data source is already present, planned, or unknown
- distinguish between common categories of data source context when that distinction changes the guidance path
- ask clarifying questions before offering instructions that depend on unresolved data-source assumptions
- prevent the assistant from presenting a single data-source path as universally correct

## Naming Customization Behavior

The application must allow users to customize naming where generated guidance refers to apps, screens, controls, variables, or other user-facing artifacts.

This behavior must:

- avoid forcing default names when the user has an established naming scheme
- preserve user-provided names consistently throughout the conversation
- make suggested names editable rather than authoritative
- help beginners understand names without making advanced users accept rigid naming conventions

## Design Constraints

- The application design must preserve a real-time conversational experience.
- The application design must favor incremental, confirmable guidance over large speculative responses.
- The application design must remain within Microsoft Power Platform guidance boundaries.
- The application design must not depend on unsupported feature claims.
- The application design must preserve accessibility and Section 508 compliance as release-level quality expectations.

## Open Assumptions

- The exact experience-level labels and their user-facing wording remain to be determined.
- The exact onboarding questions and branching thresholds remain to be determined.
- The exact taxonomy for environment types and data-source categories remains to be determined.