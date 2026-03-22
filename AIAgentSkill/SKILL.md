---
name: AIAgentSkill
description: This skill should be used when the user asks to "create a recipe", "add a recipe", "edit a recipe", "update a recipe", "delete a recipe", "list my recipes", "search recipes", "find a recipe", or mentions .reci files, recipe management, meal planning, or the Reci app. Use this skill whenever working with .reci recipe files on the local filesystem, even if the user doesn't explicitly mention the Reci format.
---

# Reci Recipe File (.reci) — AI Agent Skill

Reci is a privacy-first recipe manager. All recipe data lives on the user's device as individual `.reci` files — plain JSON with a specific schema. There is no server or database. Create, read, edit, delete, list, and search recipes by working with these files directly.

## Prerequisites

Before performing any operation, obtain the user's **recipe directory path** — the root folder containing their `.reci` files. All file operations are relative to this directory.

## File Format

Each `.reci` file is a UTF-8 JSON file following these rules:

- **Property names**: camelCase (e.g., `quantityAmount`, `furtherNotes`)
- **Indentation**: 2 spaces
- **Null handling**: Omit fields entirely when null — never write `"field": null`
- **Empty lists**: Write as `[]` — never omit (`ingredients`, `instructions`, `tags`)
- **Encoding**: Standard JSON; both literal characters (`&`) and Unicode escapes (`\u0026`) are valid

## Schema Reference

### Recipe (root object)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Recipe name. Must not be empty. |
| `description` | string | No | Brief description of the recipe. |
| `group` | string | No | Category name (e.g., `"Breakfast"`). Determines subdirectory — see Directory Placement section below. |
| `ingredients` | Ingredient[] | Yes | List of ingredients. Use `[]` if empty. |
| `instructions` | Instruction[] | Yes | List of steps. Use `[]` if empty. |
| `nutritionInfo` | NutritionInfo | No | Per-serving nutritional info. Omit entirely if unknown. |
| `source` | RecipeSource | No | Where the recipe came from. Omit entirely if not applicable. |
| `tags` | string[] | Yes | Search tags in kebab-case (e.g., `"meal-prep"`). Use `[]` if none. |
| `furtherNotes` | string | No | Servings, prep/cook time, storage tips, variations. |

Never include an `id` or `isNew` field. Identity is the (name, group) composite key.

### Ingredient

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Ingredient name (e.g., `"rolled oats"`). |
| `quantityAmount` | number | Yes | Numeric amount as decimal: `0.5`, `0.33` — not `"1/2"`. |
| `quantityUnit` | string | Yes | Unit: `"cup"`, `"tbsp"`, `"tsp"`, `"g"`, `"kg"`, `"ml"`, `"whole"`, `"pinch"`, `"cloves"`, `"to taste"`. |
| `group` | string | No | Section heading (e.g., `"Batter"`, `"Sauce"`). Omit if ungrouped. |

### Instruction

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `text` | string | Yes | The instruction step text. Do not add step numbers. |
| `group` | string | No | Section heading (e.g., `"Prep"`, `"Cook"`). Omit if ungrouped. |

Steps are ordered by array position.

### NutritionInfo

Omit the entire object if no values are known. All fields optional.

| Field | Type | Description |
|-------|------|-------------|
| `calories` | integer | Calories per serving. |
| `fat` | number | Grams of fat. |
| `carbohydrates` | number | Grams of carbohydrates. |
| `protein` | number | Grams of protein. |

### RecipeSource

Omit the entire object if there is no source. If present, `text` is required.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `text` | string | Yes | Source name (e.g., `"Budget Bytes"`). |
| `url` | string | No | URL to the original recipe. |

## File Naming

Convert the recipe name to a filename:

1. Remove characters: `< > : " / \ | ? *` and control characters (U+0000–U+001F)
2. Trim leading/trailing whitespace and trailing periods
3. If the result is empty, use `"Untitled"`
4. Truncate to 196 characters if longer (to leave room for `.reci`)
5. Append `.reci`

| Recipe name | Filename |
|-------------|----------|
| `Chicken Tikka Masala` | `Chicken Tikka Masala.reci` |
| `Mom's "Best" Soup` | `Mom's Best Soup.reci` |

## Directory Placement

The `group` field determines file location:

```
With group:     {RecipeDir}/{Group}/{SanitizedName}.reci
Without group:  {RecipeDir}/{SanitizedName}.reci
```

The `group` value in JSON must exactly match the subdirectory name. A recipe with `"group": "Snacks & Desserts"` must live in `Snacks & Desserts/`.

## Identity

Recipes are uniquely identified by **(name, group) composite key** with **case-insensitive** comparison. Two recipes cannot share the same name within the same group.

## Operations

### Create

1. Confirm the recipe name is not empty
2. Check no existing `.reci` file has the same (name, group) key (case-insensitive)
3. Sanitize the name to produce a filename
4. If grouped, create the subdirectory if it does not exist
5. Build JSON following the schema above
6. Write the file to the correct path

### Read

- **By name and group**: Look in `{RecipeDir}/{Group}/{SanitizedName}.reci`
- **By name only**: Scan `.reci` files in the directory tree, matching the `"name"` field

### Update

1. Read the existing recipe file
2. Apply changes
3. **If name or group changed**: Delete the old file and write to the new path
4. **If only content changed**: Overwrite the existing file
5. Ensure the `group` field still matches the subdirectory

### Delete

1. Locate and delete the recipe file
2. If the group subdirectory is now empty, optionally remove it

### List

1. Read `.reci` files in the root directory (ungrouped recipes)
2. For each subdirectory (skip those starting with `.`), read `.reci` files inside
3. Parse each file to extract name, group, description, and tags

### Search

Search across recipe file contents. Priority order (matching Reci's built-in search):

1. **Name** — substring match on `name`
2. **Group** — substring match on `group`
3. **Tag** — exact match on `tags` array items
4. **Description** — substring match on `description`

## Validation Checklist

Before writing a `.reci` file, verify:

- [ ] `name` is present and not empty
- [ ] `ingredients`, `instructions`, and `tags` are arrays (use `[]` if empty)
- [ ] Every ingredient has `name`, `quantityAmount` (number), and `quantityUnit` (string)
- [ ] Every instruction has `text`
- [ ] If `source` is present, it has a `text` field
- [ ] If `nutritionInfo` is present, at least one field is non-null (otherwise omit entirely)
- [ ] No `id` or `isNew` fields are present
- [ ] All property names are camelCase
- [ ] Null fields are omitted, not written as `null`
- [ ] The `group` value matches the subdirectory name exactly
- [ ] The filename contains no forbidden characters
- [ ] No existing recipe has the same (name, group) key (case-insensitive)

## Edge Cases

- **Special characters in group names**: `"Snacks & Desserts"`, `"Meal Prep - Chicken"` are valid. Keep `&`, `-`, spaces as-is in both JSON and directory name.
- **Decimal quantities**: Use `0.33` not `1/3`, `0.5` not `1/2`. The field is a JSON number.
- **"to taste" convention**: Use `"quantityUnit": "to taste"` with `"quantityAmount": 1`.
- **Countable items**: Use `"quantityUnit": "whole"` (e.g., 2 whole eggs).
- **Renaming**: Delete the old file, create a new one — the filename derives from the name.
- **Moving groups**: Delete from old group directory, write to new, update `group` in JSON.

## Additional Resources

### Examples

For complete recipe examples at three complexity levels (minimal, standard, and complex with ingredient/instruction groups and nutrition info), consult:

- **`references/examples.md`** — Three full JSON examples with file path guidance
