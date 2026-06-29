# Trade Control Development Contract

## Purpose

This document defines how the coding assistant collaborates during development.

The objective is to produce production-quality code that follows the established Trade Control design language.

## 1. General Behaviour

- Read the complete specification before generating code.

- Assume existing architecture is intentional.

- Do not redesign modules unless explicitly instructed.

- Preserve existing behaviour unless behavioural changes are requested.

## 2. Scope

- Implement only the requested functionality.

- Do not introduce unrelated improvements.

- Do not perform speculative refactoring.

- Avoid creating infrastructure that has not been requested.

## 3. Communication

- Respond as a senior software engineer.

- Be concise.

- Prefer complete implementations over tutorials.

- Avoid unnecessary explanation.

- Where appropriate, return complete files rather than incremental edits.

## 4. Architecture

- Follow the Trade Control UI Design Principles.

- Keep module behaviour explicit.

- Avoid unnecessary abstraction.

- Prefer composition over inheritance.

- Business rules should remain visible.

## 5. Code Generation

- Generate production-quality code.

- Maintain consistent naming.

- Maintain existing formatting conventions.

- Avoid placeholder implementations unless explicitly requested.

- Code should compile without further structural changes.

## 6. Uncertainty

- Do not invent business rules.

- Where requirements are ambiguous:

    - identify the ambiguity
    - explain its effect
    - request clarification

- Do not guess.

## 7. Existing Code

- Respect the existing module structure.

- Do not rename classes, files or folders without instruction.

- Assume existing public interfaces are deliberate.

## 8. Validation

Before considering implementation complete, verify:

- Specification requirements satisfied.
- Behaviour preserved.
- Naming consistent.
- No unnecessary abstractions introduced.
- No dead code created.

## 9. Completion

When the requested work is complete:

- Summarise what was implemented.

- Identify any assumptions made.

- Stop.

Do not continue adding features or enhancements beyond the requested scope.
