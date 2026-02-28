# Reci - [Try it Out](https://reci.sebastianbogle.com)
Reci is a minimalist recipe management application designed to help you organize and access your favorite recipes with ease.

Try it out at [reci.sebastianbogle.com](https://reci.sebastianbogle.com) and use the '[seb.reci](https://github.com/sebbogle/reci/blob/dev/seb.reci)' file to get started!

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

You can use AI tools (like ChatGPT or Claude) to generate recipe data in the `.reci` format for easy import into the application.

### AI Prompt Template

Reci doesn't have any native AI features due to cost and complexity constraints, however recipes in a 'reci' formatted can be generated off platform and can then be imported into the app.

Copy and paste the following prompt into your AI assistant, replacing `[INSERT SPECIFIC RECIPE REQUEST HERE]` with your desired recipe request:

````markdown
You are a recipe data generator. Generate a JSON file in the .reci format with the following structure:

## Schema Requirements:

**ReciFile** (root object):
- `Version` (string, required): Use "1.0.0"
- `Settings` (object, optional): Can be null or empty object {}
- `Recipes` (array, optional): List of recipe objects
- `Groups` (array, optional): List of group objects for organizing recipes, ingredients, and instructions

**Recipe** object:
- `Id` (string, UUID format): Generate a unique GUID for each recipe
- `Name` (string, required): Recipe name
- `Description` (string, optional): Brief description
- `GroupId` (string, UUID format, optional): Reference to a Group Id if categorizing recipes
- `Ingredients` (array): List of ingredient objects
- `Instructions` (array): List of instruction objects
- `NutritionInfo` (object, optional): Nutrition information
- `Source` (object, optional): Recipe source information
- `Tags` (array of strings): Relevant tags (e.g., "dinner", "vegetarian", "quick")
- `FurtherNotes` (string, optional): Additional notes

**Ingredient** object:
- `Name` (string, required): Ingredient name
- `QuantityAmount` (decimal, required): Numeric quantity
- `QuantityUnit` (string, required): Unit of measurement (e.g., "cup", "tbsp", "g")
- `GroupId` (string, UUID format, optional): For grouping ingredients (e.g., "For the sauce")

**Instruction** object:
- `Text` (string, required): Step instruction
- `GroupId` (string, UUID format, optional): For grouping instructions (e.g., "Preparation", "Cooking")

**NutritionInfo** object (all optional):
- `Calories` (integer, optional): Calorie count
- `Fat` (decimal, optional): Fat in grams
- `Carbohydrates` (decimal, optional): Carbs in grams
- `Protein` (decimal, optional): Protein in grams

**RecipeSource** object:
- `Text` (string, required): Source name (e.g., "Grandma's cookbook", "AllRecipes")
- `Url` (string, optional): Source URL if available

**Group** object:
- `Id` (string, UUID format, required): Unique GUID
- `Name` (string, required): Group name
- `SortOrder` (integer, required): Display order (0-indexed)
- `GroupType` (string, required): One of "Recipe", "Ingredient", or "Instruction"

## Example Output:

```json
{
  "Version": "1.0.0",
  "Settings": {},
  "Recipes": [
    {
      "Id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "Name": "Classic Spaghetti Carbonara",
      "Description": "A creamy Italian pasta dish with bacon and eggs",
      "GroupId": "11111111-1111-1111-1111-111111111111",
      "Ingredients": [
        {
          "Name": "spaghetti",
          "QuantityAmount": 400,
          "QuantityUnit": "g",
          "GroupId": null
        },
        {
          "Name": "pancetta or bacon",
          "QuantityAmount": 200,
          "QuantityUnit": "g",
          "GroupId": null
        },
        {
          "Name": "eggs",
          "QuantityAmount": 4,
          "QuantityUnit": "whole",
          "GroupId": null
        },
        {
          "Name": "Parmesan cheese",
          "QuantityAmount": 100,
          "QuantityUnit": "g",
          "GroupId": null
        }
      ],
      "Instructions": [
        {
          "Text": "Cook spaghetti in salted boiling water until al dente",
          "GroupId": null
        },
        {
          "Text": "Fry pancetta until crispy",
          "GroupId": null
        },
        {
          "Text": "Beat eggs with grated Parmesan cheese",
          "GroupId": null
        },
        {
          "Text": "Drain pasta, mix with pancetta, remove from heat and stir in egg mixture",
          "GroupId": null
        }
      ],
      "NutritionInfo": {
        "Calories": 650,
        "Fat": 28.5,
        "Carbohydrates": 72.0,
        "Protein": 32.0
      },
      "Source": {
        "Text": "Traditional Italian Recipe",
        "Url": null
      },
      "Tags": ["pasta", "italian", "dinner", "comfort-food"],
      "FurtherNotes": "Reserve some pasta water to adjust consistency if needed"
    }
  ],
  "Groups": [
    {
      "Id": "11111111-1111-1111-1111-111111111111",
      "Name": "Italian Dishes",
      "SortOrder": 0,
      "GroupType": "Recipe"
    }
  ]
}
```

## User Request:
[INSERT SPECIFIC RECIPE REQUEST HERE]

Please generate a complete .reci file with the recipe(s) requested.
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
