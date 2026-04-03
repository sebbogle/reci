# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Reci is a privacy-first, client-side recipe manager built with **Blazor WebAssembly (.NET 10)** and **Microsoft Fluent UI**. There is no backend server — all data is stored locally on the user's device via the File System Access API. Each recipe is an individual `.reci` JSON file organized in subdirectories by group.

## Commands

```bash
# Dev server (http://localhost:5265)
dotnet run

# Build
dotnet build -c Release

# Format check (enforced by CI on push/PR to dev)
dotnet format Reci.slnx --verify-no-changes

# Auto-fix formatting
dotnet format Reci.slnx

# Run E2E tests (AppFixture starts the app automatically)
dotnet test --project Tests/Tests.csproj

# Update golden screenshots after intentional UI changes
UPDATE_SNAPSHOTS=true dotnet test --project Tests/Tests.csproj

# One-time Playwright browser install
dotnet build Tests/Tests.csproj && pwsh Tests/bin/Debug/net10.0/playwright.ps1 install
```

## Architecture

**Layered structure:**
- `Pages/` — Routable pages (`Contents.razor` at `/`, `RecipePage.razor` at `/recipe/{slug}`)
- `Layout/` — App shell (`MainLayout`, `NavMenu`, `Header`)
- `Components/` — Reusable Razor components split into `RecipeDisplay/` and `RecipeEditor/`
- `Services/` — Business logic, all registered as **Scoped** in `Program.cs`, coded to interfaces in `Services/Interfaces/`
- `Data/Repositories/` — `FileSystemRecipeRepository` with in-memory cache + semaphore locking
- `Data/Models/` — Record types (`Recipe`, `Ingredient`, `Instruction`, etc.)
- `Core/` — Utilities (`Result<T>` railway-oriented error handling, extension methods)

**Key patterns:**
- Cross-component state sync via `IRecipeStateNotifier` (pub-sub); components implement `IAsyncDisposable` to unsubscribe
- Error handling uses `Result<T>` — prefer `Result.Success()`/`Result.Failure()` over exceptions for expected failures
- JS interop through `wwwroot/js/fileSystemHelper.js` bridged via `FileSystemAccessService`
- File names: `{SanitizedName}.reci` (see `IFilePathService` / `FilePathService`)
- JSON serialization: camelCase, indented, null values omitted

## Code Style

- C# nullable reference types and implicit usings enabled
- Models use **records** for immutability (`Ingredient`, `Instruction`, `RecipeSummary`); `Recipe` is a mutable class
- PascalCase for C#, `_camelCase` for private fields, camelCase for JSON, kebab-case for CSS
- Component-scoped CSS via `.razor.css` files; global styles in `wwwroot/css/app.css`
- `IGroupable` interface for items supporting grouping (ingredients, instructions)
- Async throughout with `CancellationToken` support
- Prefer explicit types over `var` (enforced by .editorconfig at warning level)
- UI uses Fluent UI components (`FluentButton`, `FluentDialog`, etc.)
- Global usings declared in `Reci.csproj` `<Using>` items

## Testing

- **xunit.v3** + **Microsoft.Playwright** for E2E browser automation
- `AppFixture` (shared via `AppCollection`) manages the app process lifecycle
- `ReciPage` base class provides Playwright page helpers
- `[ReciPageState]` attribute injects JSON state before tests
- `ScreenshotAssert` does golden-image visual regression against `Tests/Resources/Golden Screenshots/`
