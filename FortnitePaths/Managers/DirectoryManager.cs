namespace FortnitePaths.Managers;

public static class DirectoryManager
{
    public static string mappingsDir = Path.Combine(Environment.CurrentDirectory, ".mappings");
    public static string texturesDir = Path.Combine(Environment.CurrentDirectory, "textures");
    public static string dataDir = Path.Combine(Environment.CurrentDirectory, ".data");
    public static string audiosDir = Path.Combine(Environment.CurrentDirectory, "audios");

    static DirectoryManager()
    {
        foreach (var dir in new[]{ mappingsDir, texturesDir, dataDir, audiosDir})
        {
            if (Directory.Exists(dir))
                continue;
            Directory.CreateDirectory(dir);
            Console.WriteLine($"Created {dir}");
        }
    }
}