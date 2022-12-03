using Newtonsoft.Json;

namespace FortnitePaths.Settings;

public class SettingsManager
{

    public static string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FortnitePaths", "UserSettings.json");

    public static UserSettings LoadSettings()
    {
        try
        {
            return JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(SettingsPath));
        }
        catch
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FortnitePaths"));
            return new UserSettings();
        }
    }

    public static void SaveSettings(UserSettings user)
    {
       File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(user, Formatting.Indented));
    }
}