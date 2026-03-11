namespace Reci.Data.Models;

public record RecipeSource
{
    public required string Text { get; set; }

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

}
