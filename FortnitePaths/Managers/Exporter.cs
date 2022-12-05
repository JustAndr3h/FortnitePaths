using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using CUE4Parse.MappingsProvider;
using Newtonsoft.Json;
using FortnitePaths.Rest;
using FortnitePaths.Rest.Models;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Exports.Sound;
using CUE4Parse_Conversion.Sounds;
using CUE4Parse_Conversion.Textures;
using CUE4Parse.UE4.Objects.UObject;

namespace FortnitePaths.Managers;

public class Exporter
{
    private DefaultFileProvider provider;
    private string[] types = { "AthenaBackpackItemDefinition", "AthenaCharacterItemDefinition", "AthenaDanceItemDefinition", "AthenaItemWrapDefinition", "AthenaLoadingScreenItemDefinition", "AthenaPickaxeItemDefinition", "AthenaSprayItemDefinition", "FortVariantTokenType" };

    public void InitializeProvider(string mappingsPath, string filesPath, string language)
    {
        Console.Write("Initializing provider.. ");
        provider = new(filesPath, SearchOption.AllDirectories, true, new VersionContainer(EGame.GAME_UE5_2)); // now fortnite use UnrealEngine 2 version (536870912)
        provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mappingsPath);
        provider.Initialize();
        LoadAesKeys();

        switch (language)
        {
            case "en":
                provider.LoadLocalization(ELanguage.English);
                break;
            case "it":
                provider.LoadLocalization(ELanguage.Italian);
                break;
            case "fr":
                provider.LoadLocalization(ELanguage.French);
                break;
            case "es":
                provider.LoadLocalization(ELanguage.Spanish);
                break;
            case "de":
                provider.LoadLocalization(ELanguage.German);
                break;
            case "ar":
                provider.LoadLocalization(ELanguage.Arabic);
                break;
            default:
                provider.LoadLocalization(ELanguage.English);
                break;
        }

        Console.WriteLine("Initialized!");
        Console.Write("Insert the path you want to extract: ");
        var path = Console.ReadLine();
        Extractor(path);
        Console.WriteLine("Finish");
        Console.ReadKey();
    }

    public void LoadAesKeys()
    {
        var AesKeys = Requests.GetAesKeys();
        provider.SubmitKey(new FGuid(), new FAesKey(AesKeys.mainKey));
        foreach (DynamicKey key in AesKeys.dynamicKeys) 
        {
            provider.SubmitKey(new FGuid(key.guid), new FAesKey(key.key));
        }
    }

    public void Extractor(string extractionPath)
    {
        try
        {
            var exports = provider.LoadObjectExports(extractionPath.Replace(".uasset", ""));
            var t = Path.Combine(DirectoryManager.dataDir, extractionPath.Split("/").Last().Replace(".uasset", ".json"));
            File.WriteAllText(t, JsonConvert.SerializeObject(exports, Formatting.Indented));

            Console.WriteLine($"\nSaved export data as {t}");

            foreach (var x in exports)
            {
                CheckExport(x);
                CheckType(x, extractionPath.Replace(".uasset", ""));
                break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"\nNo file found for {extractionPath}");
        }
    }

    public void CheckType(UObject file, string path) // some parts from intheshade solitude
    {
        if (types.Contains(file.ExportType) && provider.TryLoadObject(path, out var c))
        {
            if (c.TryGetValue(out UTexture2D texture, "LargePreviewImage"))
            {
                SaveTexture(texture);
            }

            if (c.TryGetValue(out UTexture2D ndTexture, "SmallPreviewImage"))
            {
                SaveTexture(ndTexture);
            }

            if (c.TryGetValue(out FPackageIndex hero, "HeroDefinition") &&
                hero.TryLoad(out var herofn) &&
                herofn is not null &&
                herofn.TryGetValue(out UTexture2D heroDefIcon, "LargePreviewImage"))
            {
                SaveTexture(heroDefIcon);
            }

            if (file.ExportType == "AthenaPickaxeItemDefinition" &&
                c.TryGetValue(out FPackageIndex pickaxe, "WeaponDefinition") &&
                pickaxe.TryLoad(out var pickaxeWid) &&
                pickaxeWid is not null &&
                pickaxeWid.TryGetValue(out UTexture2D pickaxeIcon, "LargePreviewImage"))
            {
                SaveTexture(pickaxeIcon);
            }
        }
    }

    public bool CheckExport(UObject uobject) // https://github.com/4sval/FModel/blob/snooper-tk/FModel/ViewModels/CUE4ParseViewModel.cs#L762
    {
        switch (uobject)
        {
            case UTexture2D texture:
                return SaveTexture(texture);

            case USoundWave audio:
                audio.Decode(true, out var format, out var audioData);
                if (audioData == null || string.IsNullOrEmpty(format) || audio.Owner == null)
                    return false;
                return saveAudio(Path.Combine(DirectoryManager.audiosDir, audio.Name), format, audioData);

            default:
                return true;
        }
    }

    public bool SaveTexture(UTexture2D texture)
    {
        try
        {
            var savePath = Path.Combine(DirectoryManager.texturesDir, texture.Name + ".png");
            var decoded = texture.Decode();
            var encoded = decoded.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            var file = File.Create(savePath);
            encoded.AsStream().CopyTo(file);
            Console.WriteLine($"\nSaved texture {texture.Name} in textures folder.");
            return true;
        }
        catch (Exception e)
        {
#if DEBUG
            if (e.ToString().StartsWith("System.IO.IOException:")) return false; // let's remove this useless exception
            Console.WriteLine(e);
#endif
            Console.WriteLine($"\nError while saving texture for {texture.Name}");
            return false;
        }
    }

    public bool saveAudio(string filePath, string ext, byte[] data)
    {
        try
        {
            var stream = new FileStream(filePath + "." + ext, FileMode.Create, FileAccess.Write); // https://github.com/4sval/FModel/blob/snooper-tk/FModel/ViewModels/CUE4ParseViewModel.cs#L845
            var writer = new BinaryWriter(stream);
            writer.Write(data);
            writer.Flush();
            Console.WriteLine($"\nSaved audio {filePath.Split('/').Last()} in audios folder.");
            return true;
        }
        catch (Exception e)
        {
#if DEBUG
            Console.WriteLine(e);
#endif
            Console.WriteLine($"\nError while saving texture for {filePath.Split('/').Last()}");
            return false;
        }
    }
}
