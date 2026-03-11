namespace Reci.Data.Models;

public record NutritionInfo
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

}
