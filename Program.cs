WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Logging
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Serialization
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
});

// Dependencies
builder.Services.AddFluentUIComponents();

// Infrastructure
builder.Services.AddScoped<IFileSystemAccessService, FileSystemAccessService>();
builder.Services.AddScoped<IConnectionStateService, ConnectionStateService>();

// Data
builder.Services.AddScoped<IRecipeRepository, FileSystemRecipeRepository>();

// Services
builder.Services.AddScoped<IScreenWakeLockService, ScreenWakeLockService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IRecipeExportService, RecipeExportService>();
builder.Services.AddScoped<IRecipeStateNotifier, RecipeStateNotifier>();

await builder.Build().RunAsync();
