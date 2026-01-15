using Reci.ViewModels;
using Reci.Data.Models;

namespace Reci.Mappers;

public static class SettingsMapper
{
    public static SettingsVM ToViewModel(this Settings settings)
    {
        return new SettingsVM {};
    }

    public static Settings ToModel(this SettingsVM settingsVM)
    {
        return new Settings {};
    }
}
