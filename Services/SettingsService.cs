using Reci.Data.Models;
using Reci.Data.Repositories.Interfaces;
using Reci.Services.Interfaces;
using Reci.ViewModels;
using Reci.Mappers;

namespace Reci.Services;

public class SettingsService(ISettingsRepository settingsRepository, ILogger<SettingsService> logger) : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
    private readonly ILogger<SettingsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private SettingsVM? _cachedsettingsVM = null;

    public async Task<SettingsVM> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedsettingsVM is not null)
        {
            _logger.LogDebug("Returning cached settings");
            return _cachedsettingsVM;
        }

        _logger.LogDebug("Loading settings from repository");
        Settings settings = await _settingsRepository.GetSettingsAsync(cancellationToken);
        SettingsVM settingsVM = settings.ToViewModel();
        
        _cachedsettingsVM = settingsVM;

        return settingsVM;
    }

    public async Task<Result> SaveSettingsAsync(SettingsVM settingsVM, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settingsVM);

        _logger.LogDebug("Saving settings");
        Settings settings = settingsVM.ToModel();
        Result result = await _settingsRepository.SaveSettingsAsync(settings, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully saved settings");
            ClearCache();
        }
        else
        {
            _logger.LogWarning("Failed to save settings: {Error}", result.ErrorMessage);
        }

        return result;
    }
    
    public void ClearCache()
    {
        _logger.LogDebug("Settings cache cleared");
        _cachedsettingsVM = null;
    }
}
