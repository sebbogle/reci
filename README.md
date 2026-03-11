# Reci - [Try it Out](https://reci.sebastianbogle.com)
Reci is a minimalist recipe management application designed to help you organize and access your favorite recipes with ease.

Try it out at [reci.sebastianbogle.com](https://reci.sebastianbogle.com) and use the sample [`Recipes/`](https://github.com/sebbogle/reci/tree/dev/Recipes) folder to get started!

## Current State
The project is still in early development.

## What Reci Aims to Be
- A simple and intuitive interface for storing and retrieving recipes, prioritising easy access to the information while cooking.
- Privacy-focused, with all data stored locally on the user's device.

## What Reci Isn't
- A full-featured cooking app with meal planning and grocery lists.
- A platform for recipe discovery or sharing.

## MVP Feature Targets
- [x] View recipes.
- [x] Add, edit, and delete recipes.
- [x] Basic grouping functionality.
- [ ] Search functionality.
- [ ] Stable and reliable.

## Generating `.reci` Files with AI

You can use AI tools (like ChatGPT or Claude) to generate recipe data in the `.reci` format for easy import into the application. Each `.reci` file contains a single recipe as a JSON object.

### AI Prompt Template

Reci doesn't have any native AI features due to cost and complexity constraints, however recipes in a 'reci' format can be generated off platform and can then be imported into the app.

Copy and paste the following prompt into your AI assistant, replacing `[INSERT SPECIFIC RECIPE REQUEST HERE]` with your desired recipe request:

````markdown
You are a recipe data generator. Generate one or more JSON files in the .reci format. Each .reci file contains a single recipe as the root JSON object (no wrapper).

## Schema Requirements:

**Recipe** (root object — one per `.reci` file):
- `id` (string, UUID format): Generate a unique GUID for each recipe
- `name` (string, required): Recipe name
- `description` (string, optional): Brief description
- `group` (string, optional): Category name for grouping recipes (e.g., "Meal Prep", "Quick Weeknight")
- `ingredients` (array): List of ingredient objects
- `instructions` (array): List of instruction objects
- `nutritionInfo` (object, optional): Nutrition information
- `source` (object, optional): Recipe source information
- `tags` (array of strings): Relevant tags (e.g., "dinner", "vegetarian", "quick")
- `furtherNotes` (string, optional): Additional notes

**Ingredient** object:
- `name` (string, required): Ingredient name
- `quantityAmount` (decimal, required): Numeric quantity
- `quantityUnit` (string, required): Unit of measurement (e.g., "cup", "tbsp", "g")
- `group` (string, optional): For grouping ingredients (e.g., "For the sauce", "Base Components")

**Instruction** object:
- `text` (string, required): Step instruction
- `group` (string, optional): For grouping instructions (e.g., "Preparation", "Cooking")

**NutritionInfo** object (all optional):
- `calories` (integer, optional): Calorie count
- `fat` (decimal, optional): Fat in grams
- `carbohydrates` (decimal, optional): Carbs in grams
- `protein` (decimal, optional): Protein in grams

**RecipeSource** object:
- `text` (string, required): Source name (e.g., "Grandma's cookbook", "AllRecipes")
- `url` (string, optional): Source URL if available


## Example Output:

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "Classic Spaghetti Carbonara",
  "description": "A creamy Italian pasta dish with bacon and eggs",
  "group": "Italian Dishes",
  "ingredients": [
    {
      "name": "spaghetti",
      "quantityAmount": 400,
      "quantityUnit": "g"
    },
    {
      "name": "pancetta or bacon",
      "quantityAmount": 200,
      "quantityUnit": "g"
    },
    {
      "name": "eggs",
      "quantityAmount": 4,
      "quantityUnit": "whole"
    },
    {
      "name": "Parmesan cheese",
      "quantityAmount": 100,
      "quantityUnit": "g"
    }
  ],
  "instructions": [
    {
      "text": "Cook spaghetti in salted boiling water until al dente"
    },
    {
      "text": "Fry pancetta until crispy"
    },
    {
      "text": "Beat eggs with grated Parmesan cheese"
    },
    {
      "text": "Drain pasta, mix with pancetta, remove from heat and stir in egg mixture"
    }
  ],
  "nutritionInfo": {
    "calories": 650,
    "fat": 28.5,
    "carbohydrates": 72.0,
    "protein": 32.0
  },
  "source": {
    "text": "Traditional Italian Recipe"
  },
  "tags": ["pasta", "italian", "dinner", "comfort-food"],
  "furtherNotes": "Reserve some pasta water to adjust consistency if needed"
}
```

## File Naming:
Name each file as: `{RecipeName}_{first8charsOfGuid}.reci`
For example: `Classic Spaghetti Carbonara_a1b2c3d4.reci`

If generating multiple recipes with the same group, place them in a folder named after the group.

## User Request:
[INSERT SPECIFIC RECIPE REQUEST HERE]

Please generate the recipe(s) as `.reci` files.
````

### Usage Examples

- **Single Recipe:** 'Generate a chocolate chip cookie recipe with nutritional information'

- **Multiple Recipes:** 'Generate 3 healthy breakfast recipes'

- **Themed Collection:** 'Generate 5 vegetarian dinner recipes and organize them in a "Meatless Mondays" group'

- **From Source:** 'Generate the recipe(s) from "url.com" or "C:\File" '

## Technology
### Dotnet Blazor WASM
Reci is built using Dotnet Blazor WebAssembly (WASM), allowing it to run entirely in the browser without the need for server-side components. This ensures that all user data remains local and private.
This choice of technology also enables cross-platform compatibility, making Reci accessible on various devices and operating systems.

### Fluent UI
The user interface of Reci is designed using Fluent UI, a modern design system developed by Microsoft.
Fluent UI provides a consistent and visually appealing experience across different platforms and devices.

## Testing
Reci uses Playwright-based integration tests that launch the real app and drive a browser.

### Prerequisites
Install Playwright browsers (one-time after building the test project):
```
dotnet build Tests/Tests.csproj
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install
```

### Running Tests
```
dotnet test Tests/Tests.csproj
```

### Updating Golden Screenshots
When UI changes are intentional, update the baseline images:
```powershell
$env:UPDATE_SNAPSHOTS="true"; dotnet test Tests/Tests.csproj
```
Review the updated PNGs in `Tests/Resources/Golden Screenshots/` before committing.

On failure, the actual screenshot is saved to `Tests/bin/.../Temp Test Screenshots/` for comparison.

## Code Formatting
The project uses `dotnet format` with an `.editorconfig` at the repository root to enforce consistent C# code style.

### Running Locally
Check for violations without modifying files:
```
dotnet format Reci.slnx --verify-no-changes
```

Auto-fix formatting issues:
```
dotnet format Reci.slnx
```

### CI Enforcement
A GitHub Actions workflow (`.github/workflows/format-check.yml`) runs `dotnet format --verify-no-changes` on every push and PR to `dev`. The build will fail if formatting drifts.

### Limitations
- `dotnet format` does **not** support `.razor` files. Razor formatting relies on IDE settings only.
- `var` usage is flagged as a **warning** (not a build-breaking error) to allow gradual migration to explicit types.
