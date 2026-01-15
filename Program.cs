using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Reci;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Dependencies
builder.Services.AddFluentUIComponents();
builder.Services.AddBlazoredLocalStorage();

// Data
builder.Services.AddScoped<ISettingsRepository, LocalStorageSettingsRepository>();
builder.Services.AddScoped<IGroupingRepository, LocalStorageGroupingRepository>();
builder.Services.AddScoped<IRecipeRepository, LocalStorageRecipeRepository>();

// Services
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IScreenWakeLockService, ScreenWakeLockService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IGroupingService, GroupingService>();
builder.Services.AddScoped<IDataTransferService, DataTransferService>();

await builder.Build().RunAsync();
