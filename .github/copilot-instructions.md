# Project Guidelines

## Architecture

Blazor WebAssembly (.NET 10) client-side recipe manager using Fluent UI. All data is stored locally on the user's device via the File System Access API — there is no backend server.

**Layered structure:**
- `Data/Models/` — Record types (`Recipe`, `Ingredient`, `Instruction`, etc.)
- `Data/Repositories/` — `FileSystemRecipeRepository` with in-memory cache + semaphore locking
- `Services/` — Business logic (`RecipeService`, `RecipeExportService`, `ConnectionStateService`, etc.)
- `Components/` — Reusable Razor components (display + editor)
- `Pages/` — Routable pages (`Contents.razor` at `/`, `RecipePage.razor` at `/recipe/{slug}`)
- `Core/` — Utilities (`Result<T>`, extension methods)
- `Layout/` — App shell (`MainLayout`, `NavMenu`, `Header`)

**Key patterns:**
- All services are registered as **Scoped** in `Program.cs` and coded to interfaces in `Services/Interfaces/`
- Each recipe is stored as an individual `.reci` JSON file; files are organized in subdirectories by recipe group
- Sample recipes live in the `Recipes/` folder at the repository root, mirroring the on-device folder structure
- Cross-component state sync via `IRecipeStateNotifier` (pub-sub)
- Error handling uses `Result<T>` (railway-oriented) — prefer `Result.Success()`/`Result.Failure()` over exceptions for expected failures
- JS interop via `wwwroot/js/fileSystemHelper.js` bridged through `FileSystemAccessService`

## Code Style

- C# nullable reference types enabled; implicit usings enabled
- Models use **records** for immutability (`Ingredient`, `Instruction`, `RecipeSummary`)
- PascalCase for C#, camelCase for JSON serialization, kebab-case for CSS classes
- Component-scoped CSS via `.razor.css` files; global styles in `wwwroot/css/`
- Extension methods for validation (`.IsEmpty()`, `.IsEqualTo()`) and null checks (`.ThrowIfNull()`)
- `IGroupable` interface for items that support grouping (ingredients, instructions)
- Async throughout with `CancellationToken` support
- Components implement `IAsyncDisposable` to unsubscribe from notifiers

## Build

```bash
# Run the app (dev server at http://localhost:5265)
dotnet run

# Build release
dotnet build -c Release
```

## Conventions

- File names are sanitized via `IFilePathService` / `FilePathService` with cross-platform validation
- JSON config: camelCase property names, null values omitted, indented formatting
- UI components use Microsoft Fluent UI (`FluentButton`, `FluentDialog`, etc.)
- Global usings are declared in `Reci.csproj` `<Using>` items — no need for repeated `using` statements in most files
