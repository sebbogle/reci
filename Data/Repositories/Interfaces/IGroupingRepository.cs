using Reci.Data.Models;

namespace Reci.Data.Repositories.Interfaces;

public interface IGroupingRepository
{
    Task<List<Group>> GetGroupsAsync(CancellationToken cancellationToken = default);

    Task<Result> AddGroupAsync(Group group, CancellationToken cancellationToken = default);

    Task<Result> UpdateGroup(Group group, CancellationToken cancellationToken = default);

    Task<Result> DeleteGroup(Guid id, CancellationToken cancellationToken = default);

    Task<Result> SetGroups(List<Group> groups, CancellationToken cancellationToken = default);



    //Task<List<RecipeGroup>> GetRecipeGroupsAsync(CancellationToken cancellationToken = default);
        
    //Task<Result> AddRecipeGroupAsync(RecipeGroup group, CancellationToken cancellationToken = default);
    
    //Task<Result> UpdateRecipeGroup(RecipeGroup group, CancellationToken cancellationToken = default);

    //Task<Result> DeleteRecipeGroup(Guid id, CancellationToken cancellationToken = default);

    //Task<Result> SetRecipeGroups(List<RecipeGroup> groups, CancellationToken cancellationToken = default);

    //Task<List<IngredientGroup>> GetIngredientGroupsAsync(CancellationToken cancellationToken = default);

    //Task<List<InstructionGroup>> GetInstructionsGroupsAsync(CancellationToken cancellationToken = default);


    //Task<Result> AddIngredientGroupAsync(RecipeGroup group, CancellationToken cancellationToken = default);

    //Task<Result> AddInstructionGroupAsync(RecipeGroup group, CancellationToken cancellationToken = default);
}
