namespace Tests.Unit.Services;

public class FilePathServiceTests
{
    private readonly FilePathService _service = new();

    #region SanitizeForFileSystem

    [Fact]
    public void Sanitize_ValidName_ReturnsSameName()
    {
        Assert.Equal("My Recipe", _service.SanitizeForFileSystem("My Recipe"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Sanitize_NullOrWhitespace_ReturnsUntitled(string? input)
    {
        Assert.Equal("Untitled", _service.SanitizeForFileSystem(input!));
    }

    [Theory]
    [InlineData("<Recipe>", "Recipe")]
    [InlineData("My:Recipe", "MyRecipe")]
    [InlineData("A/B\\C", "ABC")]
    [InlineData("Test|File", "TestFile")]
    [InlineData("What?", "What")]
    [InlineData("Star*Recipe", "StarRecipe")]
    [InlineData("Quote\"Name", "QuoteName")]
    public void Sanitize_InvalidChars_Removed(string input, string expected)
    {
        Assert.Equal(expected, _service.SanitizeForFileSystem(input));
    }

    [Fact]
    public void Sanitize_ControlCharacters_Removed()
    {
        Assert.Equal("Test", _service.SanitizeForFileSystem("Te\x01s\x1Ft"));
    }

    [Fact]
    public void Sanitize_TrailingDots_Trimmed()
    {
        Assert.Equal("Recipe", _service.SanitizeForFileSystem("Recipe..."));
    }

    [Fact]
    public void Sanitize_LeadingTrailingSpaces_Trimmed()
    {
        Assert.Equal("Recipe", _service.SanitizeForFileSystem("  Recipe  "));
    }

    [Fact]
    public void Sanitize_AllInvalidChars_ReturnsUntitled()
    {
        Assert.Equal("Untitled", _service.SanitizeForFileSystem("<>:\"/\\|?*"));
    }

    [Fact]
    public void Sanitize_MixedValidInvalid_KeepsValid()
    {
        Assert.Equal("Hello World", _service.SanitizeForFileSystem("Hello <World>"));
    }

    #endregion

    #region ToFileName

    [Fact]
    public void ToFileName_NormalName_AddsReciExtension()
    {
        Assert.Equal("Pancakes.reci", _service.ToFileName("Pancakes"));
    }

    [Fact]
    public void ToFileName_LongName_Truncates()
    {
        string longName = new('A', 250);
        string result = _service.ToFileName(longName);
        Assert.EndsWith(".reci", result);
        Assert.True(result.Length <= 200);
    }

    [Fact]
    public void ToFileName_TruncatedName_DoesNotEndInSpace()
    {
        // Create a name where truncation would leave a trailing space
        string name = new string('A', 195) + " BBB";
        string result = _service.ToFileName(name);
        string baseName = result[..^5]; // remove .reci
        Assert.False(baseName.EndsWith(' '));
    }

    [Fact]
    public void ToFileName_SpecialChars_Sanitized()
    {
        Assert.Equal("My Recipe.reci", _service.ToFileName("My <Recipe>"));
    }

    #endregion

    #region ValidateFileName

    [Fact]
    public void ValidateFileName_Valid_ReturnsNull()
    {
        Assert.Null(_service.ValidateFileName("Classic Pancakes"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateFileName_NullOrEmpty_ReturnsError(string? name)
    {
        Assert.NotNull(_service.ValidateFileName(name));
    }

    [Fact]
    public void ValidateFileName_TrailingSpace_ReturnsError()
    {
        Assert.NotNull(_service.ValidateFileName("Recipe "));
    }

    [Fact]
    public void ValidateFileName_LeadingSpace_ReturnsError()
    {
        Assert.NotNull(_service.ValidateFileName(" Recipe"));
    }

    [Fact]
    public void ValidateFileName_TrailingDot_ReturnsError()
    {
        Assert.NotNull(_service.ValidateFileName("Recipe."));
    }

    [Fact]
    public void ValidateFileName_InvalidChars_ReturnsError()
    {
        Assert.NotNull(_service.ValidateFileName("My<Recipe>"));
    }

    [Fact]
    public void ValidateFileName_TooLong_ReturnsError()
    {
        string longName = new('A', 197); // > 196 (200 - ".reci".length)
        Assert.NotNull(_service.ValidateFileName(longName));
    }

    [Fact]
    public void ValidateFileName_ExactlyAtMax_ReturnsNull()
    {
        string maxName = new('A', 196);
        Assert.Null(_service.ValidateFileName(maxName));
    }

    #endregion

    #region ValidateGroupName

    [Fact]
    public void ValidateGroupName_Valid_ReturnsNull()
    {
        Assert.Null(_service.ValidateGroupName("Breakfast"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidateGroupName_NullOrEmpty_ReturnsNull(string? group)
    {
        Assert.Null(_service.ValidateGroupName(group));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public void ValidateGroupName_DotOrDotDot_ReturnsError(string group)
    {
        Assert.NotNull(_service.ValidateGroupName(group));
    }

    [Fact]
    public void ValidateGroupName_InvalidChars_ReturnsError()
    {
        Assert.NotNull(_service.ValidateGroupName("Dinner/Lunch"));
    }

    [Fact]
    public void ValidateGroupName_TooLong_ReturnsError()
    {
        string longGroup = new('A', 201);
        Assert.NotNull(_service.ValidateGroupName(longGroup));
    }

    [Fact]
    public void ValidateGroupName_TrailingSpace_ReturnsError()
    {
        Assert.NotNull(_service.ValidateGroupName("Breakfast "));
    }

    [Fact]
    public void ValidateGroupName_TrailingDot_ReturnsError()
    {
        Assert.NotNull(_service.ValidateGroupName("Breakfast."));
    }

    #endregion

    #region ValidateRecipePath

    [Fact]
    public void ValidateRecipePath_Valid_ReturnsNull()
    {
        Assert.Null(_service.ValidateRecipePath("Pancakes", "Breakfast"));
    }

    [Fact]
    public void ValidateRecipePath_InvalidName_ReturnsError()
    {
        Assert.NotNull(_service.ValidateRecipePath("", "Breakfast"));
    }

    [Fact]
    public void ValidateRecipePath_InvalidGroup_ReturnsError()
    {
        Assert.NotNull(_service.ValidateRecipePath("Pancakes", ".."));
    }

    [Fact]
    public void ValidateRecipePath_CombinedTooLong_ReturnsError()
    {
        string name = new('A', 196);
        string group = new('B', 40);
        // Combined: 196 + 5 (.reci) + 40 + 1 (/) = 242 > 240
        Assert.NotNull(_service.ValidateRecipePath(name, group));
    }

    [Fact]
    public void ValidateRecipePath_NoGroup_Valid()
    {
        Assert.Null(_service.ValidateRecipePath("Pancakes", null));
    }

    #endregion
}
