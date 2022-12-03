using Spectre.Console;
using FortnitePaths.Settings;
using FortnitePaths.Rest;
using FortnitePaths.Managers;

namespace FortnitePaths;

public class Program
{
    public static UserSettings USettings;

    public static void Main()
    {
        Console.Title = "FortnitePaths";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Welcome in FortnitePaths");
        Console.ForegroundColor = ConsoleColor.White;

        USettings = SettingsManager.LoadSettings();

        if (USettings.GamePath is null)
        {
            Console.WriteLine("No game files path selected!");
            Console.Write("Insert your game files directory here: ");
            USettings.GamePath = Console.ReadLine();
        }

        if (USettings.Language is null)
        {
            var lang = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select your [11]language[/]")
                    .PageSize(10)
                    .MoreChoicesText("[7]Use the arrows on the keyboard to select.[/]")
                    .AddChoices(new[]
                        {
                            "en",
                            "it",
                            "es",
                            "fr",
                            "de"
                        }
                    )
            );
            USettings.Language = lang;
        }

        StartAll();

        SettingsManager.SaveSettings(USettings);
    }

    public static void StartAll()
    {
        var mappingResponse = Requests.TryGetMappings().FirstOrDefault();

        if (mappingResponse.IsValid)
        {
            if (!File.Exists(Path.Combine(DirectoryManager.mappingsDir, mappingResponse.fileName)))
            {
                Requests.downloadMappings(mappingResponse.url, mappingResponse.fileName);
            }
        }
        else
        {
            Console.WriteLine("Invalid mappings.");
            Console.ReadKey();
        }

        var all = new Exporter();

        all.InitializeProvider(Path.Combine(DirectoryManager.mappingsDir, mappingResponse.fileName), USettings.GamePath, USettings.Language);
    }
}