namespace Reci.Data.Models;

public class NutritionInfo
{
    public int? Calories { get; set; }
    
    public decimal? Fat { get; set; }
    
    public decimal? Carbohydrates { get; set; }

    public decimal? Protein { get; set; }
}

public static class NutritionInfoExtensions
{
    public static bool IsEmpty(this NutritionInfo? info)
    {
        if (info is null)
        {
            return true;
        }
        return info.Calories is null
            && info.Fat is null
            && info.Carbohydrates is null
            && info.Protein is null;
    }

    public static bool IsEqualTo(this NutritionInfo? first, NutritionInfo? second)
    {
        if (first is null && second is null)
        {
            return true;
        }
        if (first is null || second is null)
        {
            return false;
        }
        return first.Calories == second.Calories
            && first.Fat == second.Fat
            && first.Carbohydrates == second.Carbohydrates
            && first.Protein == second.Protein;
    }
}