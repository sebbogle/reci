namespace Reci.Data.Repositories;

public class LocalStorageSettingsRepository(ILocalStorageService localStorage, ILogger<LocalStorageSettingsRepository> logger) : ISettingsRepository
{
    private readonly ILocalStorageService _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    private readonly ILogger<LocalStorageSettingsRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string _localStorageKey = "settings";

    public async Task<Settings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        Settings? settings = await _localStorage.GetItemAsync<Settings?>(_localStorageKey, cancellationToken);

        return settings ?? BuildDefaultSettings();
    }

    public async Task<Result> SaveSettingsAsync(Settings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            await _localStorage.SetItemAsync<Settings>(_localStorageKey, settings, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings to local storage.");
            
            return Result.Failure($"Failed to save settings to local storage: {ex.Message}");
        }        
    }

    private static Settings BuildDefaultSettings()
    {
        return new Settings {};
    }
}
