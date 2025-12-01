# Schema Legacy

This folder contains legacy database schema and configuration tools from previous versions of Trade Control.

## Contents

- **tcNodeDb3**: The legacy SQL Server schema project (Version 3).
- **tcNode**: .NET Framework 4.8 project containing business logic for schema management and upgrades.
- **tcNodeSetup**: .NET Framework 4.8 WPF application for configuration and initialization, including `MainWindow.xaml` for business logic UI.

## Purpose

These projects are retained for historical reference and to support migration from Version 3 (`tcNodeDb3`) to the current schema (`tcNodeDb4`).  
No new development or updates are planned for these projects.

## Migration and Naming Changes

For a detailed mapping of schema and object name changes between Version 3 and Version 4, see [../changelog.md](../changelog.md).

## Guidance

- All new development should target the active schema in `../tcNodeDb4`.
- Use this folder only for reference or when migrating legacy data and logic.

---