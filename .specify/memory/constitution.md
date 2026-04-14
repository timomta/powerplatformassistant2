# Power Platform Assistant Constitution

## Core Principles

### I. Server-Side Execution Is Mandatory
All assistant execution, reasoning, state handling, and decision-making must occur on the server. The client must not perform assistant reasoning, execute user-submitted logic, or retain authoritative workflow state. Any design that weakens server-side control is non-compliant.

### II. Security And Data Minimization Are Non-Negotiable
The application must treat all user input, chat content, uploaded screenshots, and derived outputs as potentially sensitive. All user inputs, including screenshots, must be treated as untrusted. The system must minimize collection, exposure, retention, and propagation of user data. It must not disclose secrets, internal diagnostics, or privileged context to end users unless that disclosure is explicitly required for the user-facing task and has been approved as safe.

### III. Scope Is Strictly Limited To Microsoft Power Platform Guidance
The assistant must remain strictly scoped to Microsoft Power Platform guidance, with primary emphasis on Power Apps authoring and debugging support. It must not present itself as a general-purpose agent, autonomous operator, or authority outside Microsoft Power Platform guidance. When a request falls outside this scope, the product must constrain, redirect, or decline rather than imply unsupported capability.

### IV. Guidance Must Be Tenant-Aware, Incremental, And Confirmable
The assistant must respect tenant-specific capabilities, policies, configuration differences, and feature limitations. It must not imply that a capability is universally available when it may depend on licensing, environment configuration, region, rollout status, governance policy, or tenant settings. It must prioritize incremental, confirmable guidance that users can validate step by step.

### V. The Chat Experience Must Remain Clear, Grounded, And Safe
The user experience must preserve a real-time, conversational workflow while making system boundaries clear. The assistant must not fabricate actions taken, certainty not earned, or capabilities not provided. It must not hallucinate unsupported Microsoft Power Platform features, behaviors, connectors, settings, or debugging paths. Responses must remain attributable to the current conversation context and must not imply hidden client-side execution, unseen environmental access, or silent background actions.

### VI. Accessibility And Quality Are Release Invariants
Accessibility and Section 508 compliance must be treated as quality invariants, not optional enhancements. Every change must preserve security posture, scope boundaries, core chat behavior, and accessible operation. Regressions in authentication boundaries, data handling, screenshot safety, conversational reliability, unsupported feature claims, or accessibility are release blockers. Ambiguous behavior must be resolved in favor of safety, clarity, narrower scope, and accessible user experience.

## Product Boundaries

This product must provide a secure conversational interface for Microsoft Power Platform guidance, with Power Apps authoring and debugging assistance at its core, and nothing broader by implication.

Screenshot upload exists solely to help users explain issues for debugging. Uploaded images must be handled as sensitive troubleshooting inputs and must not expand product scope into general image analysis claims beyond the Power Apps debugging context.

The system must not require users to trust opaque client behavior. Authoritative processing must remain server-side, and the product must communicate that boundary consistently.

The assistant must present guidance in steps that can be checked against the user's tenant, environment, and observed behavior. It must prefer verifiable next actions over broad prescriptions or speculative root-cause claims.

Assumption requiring explicit validation in downstream specs and plans: access control requirements, retention periods, and compliance obligations are organization-defined and are not established by this constitution.

## Review And Quality Gates

Proposals, plans, and tasks must show how they preserve server-side execution, protect sensitive and untrusted inputs, respect tenant-specific limits, preserve accessibility and Section 508 compliance, and keep the assistant within Microsoft Power Platform guidance.

Any change that affects chat flow, screenshot handling, identity boundaries, tenant-aware guidance, accessibility, or assistant disclosures must receive explicit review against this constitution before implementation is accepted.

Acceptance requires evidence that the user experience remains real-time, the assistant remains domain-bounded, guidance remains incremental and confirmable, accessibility remains intact, and no new path is introduced for client-side reasoning, unsupported feature claims, or unintended data exposure.

## Governance

This constitution is the highest local authority for product principles in this repository. Specs, plans, tasks, and implementation decisions must conform to it.

Any amendment must be explicit, justified, and reviewed for impact on security, scope boundaries, user trust, and existing work items. Conflicting guidance in lower-level artifacts is invalid until brought into compliance.

Constitution compliance must be checked during review. Violations must be corrected or formally amended; they must not be waived informally.

**Version**: 0.1.0 | **Ratified**: Pending Review | **Last Amended**: 2026-04-14
