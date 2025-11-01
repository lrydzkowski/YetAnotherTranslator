# YetAnotherTranslator Constitution

## Core Principles

### I. Single Responsibility

Each component has one clear purpose. No multi-purpose utilities or God classes. If a class needs explanation, it's doing too much.

### II. Testability

All business logic must be testable without external dependencies. Use dependency injection. Interfaces over concrete implementations.

### III. CLI Standards

Command-line interface follows standard conventions: arguments via args, output to stdout, errors to stderr. Exit codes: 0 for success, non-zero for errors.

### IV. Error Handling

Fail fast with descriptive messages. Include context for debugging. No silent failures. Handle errors at the appropriate level.

### V. Simplicity

Choose boring solutions over clever ones. Explicit over implicit. If there's a simpler approach that works, use it.

## Technical Standards

.NET 10 version only. Standard C# conventions and formatting. No magic strings or numbers. Configuration via files or environment variables. Validation done with FluentValidation. Always implement integration tests.

## Development Workflow

Incremental changes that compile and pass tests. Fix broken tests immediately. No commented-out code in commits.

## Governance

This constitution guides all development decisions. When in doubt, choose the simpler, more testable approach. Changes to this document require clear justification.

**Version**: 1.0.0 | **Ratified**: 2025-11-01 | **Last Amended**: 2025-11-01

