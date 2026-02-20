using Moq;
using Reci.Core;
using Reci.Data.Models;
using Reci.Data.Repositories.Interfaces;
using Reci.Services;
using Reci.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Tests;

public class DataTransferServiceTests
{
    private readonly Mock<IRecipeRepository> _mockRecipeRepository;
    private readonly Mock<IGroupingRepository> _mockGroupingRepository;
    private readonly Mock<ISettingsRepository> _mockSettingsRepository;
    private readonly Mock<IRecipeStateNotifier> _mockRecipeStateNotifier;
    private readonly Mock<ILogger<DataTransferService>> _mockLogger;
    private readonly DataTransferService _service;

    public DataTransferServiceTests()
    {
        _mockRecipeRepository = new Mock<IRecipeRepository>();
        _mockGroupingRepository = new Mock<IGroupingRepository>();
        _mockSettingsRepository = new Mock<ISettingsRepository>();
        _mockRecipeStateNotifier = new Mock<IRecipeStateNotifier>();
        _mockLogger = new Mock<ILogger<DataTransferService>>();

        _service = new DataTransferService(
            _mockRecipeRepository.Object,
            _mockGroupingRepository.Object,
            _mockSettingsRepository.Object,
            _mockRecipeStateNotifier.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithEmptyRecipeId_GeneratesNewGuid()
    {
        // Arrange
        Recipe recipe = new Recipe
        {
            Id = Guid.Empty,
            Name = "Test Recipe"
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe]
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.NotEqual(Guid.Empty, capturedRecipes[0].Id);
        Assert.Equal("Test Recipe", capturedRecipes[0].Name);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithValidRecipeId_PreservesGuid()
    {
        // Arrange
        Guid existingId = Guid.NewGuid();
        Recipe recipe = new Recipe
        {
            Id = existingId,
            Name = "Test Recipe"
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe]
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.Equal(existingId, capturedRecipes[0].Id);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithEmptyGroupId_GeneratesNewGuid()
    {
        // Arrange
        Group group = new Group
        {
            Id = Guid.Empty,
            Name = "Test Group",
            SortOrder = 1,
            GroupType = GroupType.Recipe
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Groups = [group]
        };

        List<Group>? capturedGroups = null;
        _mockGroupingRepository.Setup(r => r.SetGroups(It.IsAny<List<Group>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Group>, CancellationToken>((groups, _) => capturedGroups = groups)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedGroups);
        Assert.Single(capturedGroups);
        Assert.NotEqual(Guid.Empty, capturedGroups[0].Id);
        Assert.Equal("Test Group", capturedGroups[0].Name);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithValidGroupId_PreservesGuid()
    {
        // Arrange
        Guid existingId = Guid.NewGuid();
        Group group = new Group
        {
            Id = existingId,
            Name = "Test Group",
            SortOrder = 1,
            GroupType = GroupType.Recipe
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Groups = [group]
        };

        List<Group>? capturedGroups = null;
        _mockGroupingRepository.Setup(r => r.SetGroups(It.IsAny<List<Group>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Group>, CancellationToken>((groups, _) => capturedGroups = groups)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedGroups);
        Assert.Single(capturedGroups);
        Assert.Equal(existingId, capturedGroups[0].Id);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithEmptyRecipeGroupId_SetsToNull()
    {
        // Arrange
        Recipe recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            GroupId = Guid.Empty
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe]
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.Null(capturedRecipes[0].GroupId);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithInvalidRecipeGroupId_SetsToNull()
    {
        // Arrange
        Guid invalidGroupId = Guid.NewGuid();
        Recipe recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            GroupId = invalidGroupId
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe],
            Groups = [] // No groups defined
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        _mockGroupingRepository.Setup(r => r.SetGroups(It.IsAny<List<Group>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.Null(capturedRecipes[0].GroupId);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithValidRecipeGroupId_PreservesGroupId()
    {
        // Arrange
        Guid validGroupId = Guid.NewGuid();
        Group group = new Group
        {
            Id = validGroupId,
            Name = "Test Group",
            SortOrder = 1,
            GroupType = GroupType.Recipe
        };

        Recipe recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            GroupId = validGroupId
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe],
            Groups = [group]
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        _mockGroupingRepository.Setup(r => r.SetGroups(It.IsAny<List<Group>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.Equal(validGroupId, capturedRecipes[0].GroupId);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithEmptyIngredientGroupId_SetsToNull()
    {
        // Arrange
        Recipe recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            Ingredients =
            [
                new Ingredient
                {
                    Name = "Flour",
                    QuantityAmount = 2,
                    QuantityUnit = "cups",
                    GroupId = Guid.Empty
                }
            ]
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe]
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.Single(capturedRecipes[0].Ingredients);
        Assert.Null(capturedRecipes[0].Ingredients[0].GroupId);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithInvalidIngredientGroupId_SetsToNull()
    {
        // Arrange
        Guid invalidGroupId = Guid.NewGuid();
        Recipe recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            Ingredients =
            [
                new Ingredient
                {
                    Name = "Flour",
                    QuantityAmount = 2,
                    QuantityUnit = "cups",
                    GroupId = invalidGroupId
                }
            ]
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe],
            Groups = [] // No groups defined
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        _mockGroupingRepository.Setup(r => r.SetGroups(It.IsAny<List<Group>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.Single(capturedRecipes[0].Ingredients);
        Assert.Null(capturedRecipes[0].Ingredients[0].GroupId);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithEmptyInstructionGroupId_SetsToNull()
    {
        // Arrange
        Recipe recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            Instructions =
            [
                new Instruction
                {
                    Text = "Mix ingredients",
                    GroupId = Guid.Empty
                }
            ]
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe]
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.Single(capturedRecipes[0].Instructions);
        Assert.Null(capturedRecipes[0].Instructions[0].GroupId);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithInvalidInstructionGroupId_SetsToNull()
    {
        // Arrange
        Guid invalidGroupId = Guid.NewGuid();
        Recipe recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            Instructions =
            [
                new Instruction
                {
                    Text = "Mix ingredients",
                    GroupId = invalidGroupId
                }
            ]
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe],
            Groups = [] // No groups defined
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        _mockGroupingRepository.Setup(r => r.SetGroups(It.IsAny<List<Group>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);
        Assert.Single(capturedRecipes[0].Instructions);
        Assert.Null(capturedRecipes[0].Instructions[0].GroupId);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithCompleteRecipeAndEmptyGuids_NormalizesAllGuids()
    {
        // Arrange
        Recipe recipe = new Recipe
        {
            Id = Guid.Empty,
            Name = "Test Recipe",
            Description = "Test Description",
            GroupId = Guid.Empty,
            Ingredients =
            [
                new Ingredient
                {
                    Name = "Flour",
                    QuantityAmount = 2,
                    QuantityUnit = "cups",
                    GroupId = Guid.Empty
                },
                new Ingredient
                {
                    Name = "Sugar",
                    QuantityAmount = 1,
                    QuantityUnit = "cup",
                    GroupId = Guid.Empty
                }
            ],
            Instructions =
            [
                new Instruction
                {
                    Text = "Mix ingredients",
                    GroupId = Guid.Empty
                },
                new Instruction
                {
                    Text = "Bake at 350F",
                    GroupId = Guid.Empty
                }
            ],
            Tags = ["dessert", "baking"]
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe]
        };

        List<Recipe>? capturedRecipes = null;
        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRecipes);
        Assert.Single(capturedRecipes);

        Recipe importedRecipe = capturedRecipes[0];
        Assert.NotEqual(Guid.Empty, importedRecipe.Id);
        Assert.Null(importedRecipe.GroupId);
        Assert.Equal(2, importedRecipe.Ingredients.Count);
        Assert.All(importedRecipe.Ingredients, i => Assert.Null(i.GroupId));
        Assert.Equal(2, importedRecipe.Instructions.Count);
        Assert.All(importedRecipe.Instructions, i => Assert.Null(i.GroupId));
        Assert.Equal("Test Recipe", importedRecipe.Name);
        Assert.Equal("Test Description", importedRecipe.Description);
        Assert.Equal(2, importedRecipe.Tags.Count);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_WithMultipleRecipesAndGroups_ProcessesCorrectly()
    {
        // Arrange
        Guid validGroupId = Guid.NewGuid();

        Group validGroup = new Group
        {
            Id = validGroupId,
            Name = "Desserts",
            SortOrder = 1,
            GroupType = GroupType.Recipe
        };

        Group emptyIdGroup = new Group
        {
            Id = Guid.Empty,
            Name = "Appetizers",
            SortOrder = 2,
            GroupType = GroupType.Recipe
        };

        Recipe recipe1 = new Recipe
        {
            Id = Guid.Empty,
            Name = "Recipe 1",
            GroupId = validGroupId
        };

        Recipe recipe2 = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Recipe 2",
            GroupId = Guid.Empty
        };

        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0",
            Recipes = [recipe1, recipe2],
            Groups = [validGroup, emptyIdGroup]
        };

        List<Recipe>? capturedRecipes = null;
        List<Group>? capturedGroups = null;

        _mockRecipeRepository.Setup(r => r.SetRecipesAsync(It.IsAny<List<Recipe>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Recipe>, CancellationToken>((recipes, _) => capturedRecipes = recipes)
            .ReturnsAsync(Result.Success());

        _mockGroupingRepository.Setup(r => r.SetGroups(It.IsAny<List<Group>>(), It.IsAny<CancellationToken>()))
            .Callback<List<Group>, CancellationToken>((groups, _) => capturedGroups = groups)
            .ReturnsAsync(Result.Success());

        // Act
        Result result = await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(capturedGroups);
        Assert.Equal(2, capturedGroups.Count);
        Assert.Equal(validGroupId, capturedGroups[0].Id);
        Assert.NotEqual(Guid.Empty, capturedGroups[1].Id);

        Assert.NotNull(capturedRecipes);
        Assert.Equal(2, capturedRecipes.Count);
        Assert.NotEqual(Guid.Empty, capturedRecipes[0].Id);
        Assert.Equal(validGroupId, capturedRecipes[0].GroupId);
        Assert.NotEqual(Guid.Empty, capturedRecipes[1].Id);
        Assert.Null(capturedRecipes[1].GroupId);
    }

    [Fact]
    public async Task ImportReciDefinitionAsync_CallsRecipeStateNotifier()
    {
        // Arrange
        ReciFile reciFile = new ReciFile
        {
            Version = "1.0.0"
        };

        // Act
        await _service.ImportReciDefinitionAsync(reciFile);

        // Assert
        _mockRecipeStateNotifier.Verify(n => n.NotifyRecipesChangedAsync(), Times.Once);
    }
}
