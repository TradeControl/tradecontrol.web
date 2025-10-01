# Models

This folder contains the Entity Framework Core models used by the Trade Control web application.

## Purpose

The models in this folder represent the application's data structures and are mapped to the underlying SQL Server schema. They are used throughout the application for data access, business logic, and UI binding.

## Naming Convention

Model class names in this folder use an underscore to separate the schema and object name (e.g., `Object_tbMirror`). This convention improves traceability between C# models and SQL Server schema objects. All other classes in the solution use standard PascalCase.

## Migration Intent

> **Statement of Intent:**  
> The current models are based on the legacy schema (`tcNodeDb3`). As part of the migration to the new schema (`tcNodeDb4`), all models and data access code will be updated to align with the new naming conventions and structure. This will ensure consistency, maintainability, and alignment with the underlying [Production Theory](https://tradecontrol.github.io/articles/tc_production/).

## Migration Mapping

The following table summarizes the key name changes between the legacy schema (V3) and the new schema (V4). All model and property names will be updated accordingly:

| V3 Name           | V4 Name           |
|-------------------|-------------------|
| Object          | Object            |
| Subject      | Subject           |
| Subject               | Subject           |
| Task              | Project           |
| tbMode            | tbPolarity        |
| CashMode          | CashPolarity      |
| AccountCode       | SubjectCode       |
| AccountName       | SubjectName       |
| AccountSource     | SubjectSource     |
| DefaultAccountCode| DefaultSubjectCode|
| AccountLookup     | SubjectLookup     |
| CashAccountCode   | AccountCode       |
| CashAccountName   | AccountName       |
| Task.proc_IsProject | Project.proc_IsProjected |
| Task.proc_Project   | Project.proc_Root      |
| Task.proc_Mode      | Task.proc_Polarity    |

For a complete list and further details, see [../Schema/changelog.md](../Schema/changelog.md).

## Next Steps

- Review and update all model class and property names to match the new schema.
- Refactor data access code and queries to use the updated names.
- Update documentation and tests to reflect the new model structure.

## Guidance

- All new development should use the updated naming conventions and target the `tcNodeDb4` schema.
- Refer to the [changelog](../Schema/changelog.md) for detailed migration guidance.


---
