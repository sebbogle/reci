# Recipe Examples

Three complete `.reci` file examples at different complexity levels.

## Minimal Recipe

A recipe with only the required fields and no optional metadata:

```json
{
  "name": "Scrambled Eggs",
  "ingredients": [
    {
      "name": "eggs",
      "quantityAmount": 3,
      "quantityUnit": "whole"
    },
    {
      "name": "butter",
      "quantityAmount": 1,
      "quantityUnit": "tbsp"
    },
    {
      "name": "salt and pepper",
      "quantityAmount": 1,
      "quantityUnit": "to taste"
    }
  ],
  "instructions": [
    {
      "text": "Crack eggs into a bowl and whisk until uniform"
    },
    {
      "text": "Melt butter in a non-stick pan over medium-low heat"
    },
    {
      "text": "Pour in eggs and stir gently with a spatula until just set"
    }
  ],
  "tags": []
}
```

**File path**: `{RecipeDir}/Scrambled Eggs.reci`

No `group` field — file lives in the root recipe directory. No `description`, `source`, `nutritionInfo`, or `furtherNotes` — all omitted because they are null.

---

## Standard Recipe

A typical recipe using all common fields but no ingredient/instruction groups:

```json
{
  "name": "Overnight Oats",
  "description": "Creamy no-cook oats with chia seeds and honey. Prep the night before for grab-and-go breakfasts.",
  "group": "Breakfast",
  "ingredients": [
    {
      "name": "rolled oats",
      "quantityAmount": 2.5,
      "quantityUnit": "cups"
    },
    {
      "name": "milk of choice",
      "quantityAmount": 2.5,
      "quantityUnit": "cups"
    },
    {
      "name": "Greek yogurt",
      "quantityAmount": 0.75,
      "quantityUnit": "cup"
    },
    {
      "name": "chia seeds",
      "quantityAmount": 2.5,
      "quantityUnit": "tbsp"
    },
    {
      "name": "honey",
      "quantityAmount": 2.5,
      "quantityUnit": "tbsp"
    },
    {
      "name": "vanilla extract",
      "quantityAmount": 1,
      "quantityUnit": "tsp"
    }
  ],
  "instructions": [
    {
      "text": "Combine oats, milk, yogurt, chia seeds, honey, and vanilla in a large bowl"
    },
    {
      "text": "Divide among 5 jars, cover, and refrigerate at least 4 hours or overnight"
    },
    {
      "text": "Stir before serving and add a splash of milk if too thick"
    }
  ],
  "source": {
    "text": "Love and Lemons",
    "url": "https://www.loveandlemons.com/overnight-oats-recipe/"
  },
  "tags": [
    "breakfast",
    "no-cook",
    "meal-prep",
    "healthy"
  ],
  "furtherNotes": "Makes 5 servings. Prep time: 10 minutes. Keeps refrigerated up to 5 days. Try variations: PB & banana, mango coconut, or apple cinnamon."
}
```

**File path**: `{RecipeDir}/Breakfast/Overnight Oats.reci`

Has a `group` of `"Breakfast"` — file lives in the `Breakfast/` subdirectory. Includes `source` with URL, `tags`, and `furtherNotes`. No `nutritionInfo` (omitted, not null).

---

## Complex Recipe (Groups + Nutrition)

A recipe with ingredient/instruction grouping and nutritional information:

```json
{
  "name": "Chicken Tikka Masala",
  "description": "Tender marinated chicken in a rich, creamy spiced tomato sauce.",
  "group": "Meal Prep - Chicken",
  "ingredients": [
    {
      "name": "boneless chicken thighs, cubed",
      "quantityAmount": 1,
      "quantityUnit": "kg",
      "group": "Marinated Chicken"
    },
    {
      "name": "plain yogurt",
      "quantityAmount": 0.5,
      "quantityUnit": "cup",
      "group": "Marinated Chicken"
    },
    {
      "name": "garam masala",
      "quantityAmount": 2,
      "quantityUnit": "tsp",
      "group": "Marinated Chicken"
    },
    {
      "name": "vegetable oil",
      "quantityAmount": 2,
      "quantityUnit": "tbsp",
      "group": "Sauce"
    },
    {
      "name": "yellow onion, diced",
      "quantityAmount": 1,
      "quantityUnit": "whole",
      "group": "Sauce"
    },
    {
      "name": "crushed tomatoes",
      "quantityAmount": 400,
      "quantityUnit": "g",
      "group": "Sauce"
    },
    {
      "name": "heavy cream",
      "quantityAmount": 0.75,
      "quantityUnit": "cup",
      "group": "Sauce"
    }
  ],
  "instructions": [
    {
      "text": "Combine chicken with yogurt and spices. Marinate at least 30 minutes",
      "group": "Marinate & Cook Chicken"
    },
    {
      "text": "Cook marinated chicken in batches over high heat until charred. Set aside",
      "group": "Marinate & Cook Chicken"
    },
    {
      "text": "Saute onion in oil until softened, add garlic and ginger for 1 minute",
      "group": "Make Sauce"
    },
    {
      "text": "Add tomatoes and simmer 15 minutes until thickened",
      "group": "Make Sauce"
    },
    {
      "text": "Stir in cream, return chicken to sauce, and simmer 5-8 minutes",
      "group": "Make Sauce"
    }
  ],
  "nutritionInfo": {
    "calories": 485,
    "fat": 22.5,
    "carbohydrates": 18,
    "protein": 52
  },
  "source": {
    "text": "RecipeTin Eats",
    "url": "https://www.recipetineats.com/chicken-tikka-masala/"
  },
  "tags": [
    "chicken",
    "indian",
    "curry",
    "meal-prep",
    "dinner"
  ],
  "furtherNotes": "Makes 5 servings. Prep: 15 min + marinating. Cook: 35 min. Refrigerate up to 4 days, freezes well for 3 months."
}
```

**File path**: `{RecipeDir}/Meal Prep - Chicken/Chicken Tikka Masala.reci`

Demonstrates:
- **Ingredient groups**: `"Marinated Chicken"` and `"Sauce"` — displayed as sections in the Reci app
- **Instruction groups**: `"Marinate & Cook Chicken"` and `"Make Sauce"` — displayed as sections
- **`nutritionInfo`** with all four fields populated
- **Group with special characters**: `"Meal Prep - Chicken"` as both the JSON field and subdirectory name
