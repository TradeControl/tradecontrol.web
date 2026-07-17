# Trade Control UI Design Principles

## Purpose

These principles define the architectural style used throughout the Trade Control web application.

They are intended to produce modules that are predictable, maintainable, and consistent. The objective is not to produce clever code, but to produce code that reveals its purpose clearly.

## 1. Module Ownership

- Each module is responsible for its own behaviour.
- Modules should be self-contained wherever practical. They should not depend upon generic frameworks to implement module-specific behaviour.
- Shared code should only exist where genuine duplication has been demonstrated.

## 2. Explicit Behaviour

Code should reveal intent.

Prefer:

    ShowRegister()
    OpenInvoice()
    PostInvoice()

over generic dispatch mechanisms or command processors.

Business workflows should be readable from the code without navigating multiple abstraction layers.

## 3. Composition Before Inheritance

- Prefer composing modules from focused components.
- Avoid deep inheritance hierarchies.
- Generic base classes should only exist where they remove significant duplication without obscuring behaviour.

## 4. User Interface Components

- UI components are responsible for presentation.
- Business rules belong elsewhere.
- Components should receive state and raise events.
- Components should not mutate application state directly.

## 5. Navigation

- Navigation belongs to the workspace shell.
- Child components should request navigation by raising events.
- They should never decide application workflow.

## 6. State

- Application state should have a single owner.
- Duplicate copies of state should be avoided.
- Parameters should be treated as read-only.

## 7. Generic Code

- Generic code must justify its existence.
- Do not create abstractions for anticipated future use.
- Extract common functionality only after repeated implementation demonstrates a genuine pattern.

## 8. Readability

- Optimise for readability.
- Future developers should understand module behaviour without studying framework internals.
- Prefer straightforward code over sophisticated abstractions.

## 9. Consistency

- Modules should follow established TradeControl patterns.
- Consistency is generally more valuable than novelty.
- Existing architectural decisions should be assumed intentional unless explicitly changed.

## 10. Implementation Philosophy

- The application is organised around modules rather than frameworks.
- The framework exists to support modules.
- Modules do not exist to demonstrate the framework.
