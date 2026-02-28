namespace Tests.Fixtures;

[CollectionDefinition(Name)]
public class AppCollection : ICollectionFixture<AppFixture>
{
    public const string Name = "App";
}
