namespace Reci.Data.Models;

public class RecipeSource
{
    public string? Text { get; set; }

    public string? Url { get; set; }
}

public static class RecipeSourceExtensions
{
    public static bool IsEmpty(this RecipeSource? source)
    {
        if (source is null)
        {
            return true;
        }
        return string.IsNullOrWhiteSpace(source.Text)
            && string.IsNullOrWhiteSpace(source.Url);
    }

    public static bool IsEqualTo(this RecipeSource? first, RecipeSource? second)
    {
        if (first is null && second is null)
        {
            return true;
        }
        if (first is null || second is null)
        {
            return false;
        }
        return first.Text == second.Text
            && first.Url == second.Url;
    }
}
