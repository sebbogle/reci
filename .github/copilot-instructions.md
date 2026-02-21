# Reci AI Coding Instructions

## Big Picture Architecture
- `Reci` is a **Blazor WebAssembly-only** app (no backend API). App setup and DI live in `Program.cs`.
- UI flow is route/page-driven (`Pages/Contents.razor`, `Pages/RecipePage.razor`) and relies on services for all business operations.
- Data is persisted locally via Blazored LocalStorage repositories:
	- `LocalStorageRecipeRepository` (`recipes` key)
	- `LocalStorageGroupingRepository` (`groups` key)
	- `LocalStorageSettingsRepository` (`settings` key)
- Service boundary pattern:
	- Pages/components call services (`IRecipeService`, `IGroupingService`, etc.)
	- Services map between Models and ViewModels (`Mappers/RecipeMapper.cs`, `Mappers/SettingsMapper.cs`)
	- Repositories persist/retrieve raw models.

## Cross-Component Data Flow
- Recipe updates propagate through `IRecipeStateNotifier` (`Services/RecipeStateNotifier.cs`).
- Pages subscribe/unsubscribe in lifecycle methods and refresh on notification (see `Contents.razor` and `RecipePage.razor` implementing `IAsyncDisposable` + `CancellationTokenSource`).
- When adding write paths that affect recipes/imports, notify via `NotifyRecipesChangedAsync()` after successful persistence.

## Project Conventions (Important)
- **C# style**: Never use `var`; always use explicit types.
- Do not add comments to the code unless it is absolutely necessary to explain complex logic. Strive for self-explanatory code through clear naming and structure.
- Use null guards consistently (`ArgumentNullException.ThrowIfNull(...)` or `ThrowIfNull()` from `Core/GenericExtensions.cs`).
- Prefer returning `Result`/`Result<T>` (`Core/Result.cs`) from service/repository operations rather than throwing for expected failures.
- Keep mapping logic in mapper extensions, not pages/components.
- Grouped recipe content uses `GroupVM<T>` where ungrouped items have `Id == null` (see `GroupVM<T>.Empty()` and `Components/RecipeEditor/GroupedEditor.razor`).

## UI and Component Patterns
- Use Fluent UI components (`Microsoft.FluentUI.AspNetCore.Components`) throughout; match existing usage in `RecipeEditor.razor` and pages.
- Editing recipes is done in a dialog panel (`DialogService.ShowPanelAsync<RecipeEditor>` in `RecipePage.razor`).
- The app currently emphasizes simple, local-first UX (see `README.md` scope and MVP notes). Avoid adding server assumptions.

## Integration Points
- Import/export is handled in `Layout/Header.razor` using `IDataTransferService` + JS interop.
- JS helper lives in `wwwroot/js/fileHelper.js` (`downloadFile`) and is loaded by `wwwroot/index.html`.
- Wake lock integration is encapsulated in `Services/ScreenWakeLockService.cs` via `IJSRuntime`; keep JS access inside services when extending this area.

## Build/Test Workflow
- Build app: `dotnet build`
- Run app locally: `dotnet run`
- Run tests: `dotnet test Tests/Tests.csproj`
- Target framework is `net10.0` (`Reci.csproj`, `Tests/Tests.csproj`).

## Change Scope Guidance for Agents
- Prefer focused, minimal edits in existing service/repository/page structure.
- If adding new persisted data, define repository behavior first, then service mapping, then UI wiring.
- Preserve route + state-notifier refresh behavior when modifying recipe CRUD, import/export, or grouping logic.
