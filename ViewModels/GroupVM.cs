namespace Reci.ViewModels;

public class GroupVM<T> : List<T>
{
    public required Guid? Id { get; set; }

    public string? Name { get; set; }

    public int SortOrder { get; set; }


    public static GroupVM<T> Empty() => new() 
    {
        Id = null ,
        Name = null,
        SortOrder = 0
    };
}


