using System.Diagnostics;

namespace UnrealRepacker;

public class Repacker(Config config)
{
    private Config config = config;

    public List<ModInfo> Mods { get; } = [];
    public ModInfo MainMod { get; set; }

    private readonly List<PakInfo> allPaks = [];
    public IEnumerable<PakInfo> Paks
    {
        get
        {
            var pakTypes = MainMod.paks.Select(x => x.PakType);
            return allPaks.Where(x => pakTypes.Contains(x.PakType));
        }
    }

    public void AddPak(PakInfo pak)
    {
        var mod = Mods.FirstOrDefault(m => m.modName == pak.ModName);

        if (mod == null)
        {
            mod = new ModInfo(pak.ModName);
            Mods.Add(mod);
        }

        mod.paks.Add(pak);
        allPaks.Add(pak);

        MainMod ??= mod;
    }

    public IEnumerable<string> GetMainModImports()
    {
        // if server pak is not found, use client
        var mainPak = MainMod.paks.FirstOrDefault(p => p.PakType == PakType.WindowsServer);
        mainPak ??= MainMod.paks.FirstOrDefault(p => p.PakType == PakType.LinuxServer);
        mainPak ??= MainMod.paks.First(p => p.PakType == PakType.WindowsClient);

        string searchRoot = Path.Combine(config.ExtractDirectory, mainPak.PakType.ToString());
        string searchPath = Path.Combine(searchRoot, MainMod.modName);

        return Directory
            .EnumerateFiles(searchPath, "*.uasset", SearchOption.AllDirectories)
            .Select(p => AssetHelper.PathToImport(searchRoot, p));
    }


    public void Pack(IEnumerable<string> imports, string newModName)
    {
        SafeRemoveDirectory(config.TempDirectory);

        foreach (var pak in MainMod.paks)
        {
            Pack(imports, pak, newModName);
        }
    }

    private void Pack(IEnumerable<string> imports, PakInfo targetPak, string newModName)
    {
        var directory = Path.Combine(config.ExtractDirectory, targetPak.PakType.ToString());
        var uassetDependencies = AssetHelper.SearchDependencies(imports, directory);

        // copy files from extract directory to temp directory
        foreach (var sourceUasset in uassetDependencies)
        {
            if (!File.Exists(sourceUasset)) continue;

            var relativePath = Path.GetRelativePath(directory, sourceUasset);

            var parts = relativePath.Split(Path.DirectorySeparatorChar).ToList();
            var dependencyModName = parts[0];

            // remove <modname>/Content
            parts.RemoveRange(0, 2);

            // if dependency mod is not the main mod
            // change <path>
            // to     __IMPORTS__/<modname>/<path>
            if (dependencyModName != MainMod.modName)
            {
                parts.InsertRange(0, ["__IMPORTS__", dependencyModName]);
            }

            // all assets should be in Content
            parts.Insert(0, "Content");

            // get new abs path
            var newPathRelative = string.Join(Path.DirectorySeparatorChar, parts);
            string destUasset = Path.Combine(config.TempDirectory, targetPak.PakType.ToString(), newPathRelative);

            // change imports and save
            var additionalMods = Mods.Where(m => m != MainMod).Select(m => m.modName).ToArray();
            AssetHelper.ChangeImportsAndSave(sourceUasset, destUasset, MainMod.modName, additionalMods, newModName);
        }

        CreatePak(targetPak, newModName);
    }

    private void CreatePak(PakInfo targetPak, string newModName)
    {
        // create dummy AssetRegistry.bin in temp directory
        string assetRegistryPath = Path.Combine(config.TempDirectory,
                                                targetPak.PakType.ToString(),
                                                "AssetRegistry.bin");
        File.Create(assetRegistryPath).Close();

        // create ResponseFile
        string responseFilePath = Path.Combine(config.TempDirectory, "ResponseFile.txt");
        string responseFileContent = $"\"{Path.Combine(config.TempDirectory, targetPak.PakType.ToString(), "*.*")}\" \"../../../Mordhau/Mods/{newModName}/*.*\" -compress";
        File.WriteAllText(responseFilePath, responseFileContent);

        // build command for execution
        var pakName = newModName + targetPak.PakType.ToString() + ".pak";
        var pakPath = Path.Combine(config.PackedDirectory, newModName, pakName);

        string command = $"\"{config.UnrealPak}\" \"{pakPath}\" \"-Create={responseFilePath}\"";

        // run process and wait for it to complete
        var process = Process.Start(command);
        process.WaitForExit();
    }


    public IEnumerable<PakInfo> SearchPaksForSameMod(string pathToPak)
    {
        string dir = Path.GetDirectoryName(pathToPak);

        PakInfo targetPak = new PakInfo(pathToPak);
        List<PakInfo> foundPaks = [targetPak];

        foreach (string file in Directory.EnumerateFiles(dir))
        {
            // skip original file
            if (file == pathToPak) continue;

            // get all paks with same mod name
            try
            {
                PakInfo pakInfo = new PakInfo(file);
                if (pakInfo.ModName == targetPak.ModName)
                {
                    foundPaks.Add(pakInfo);
                }
            }
            catch (Exception) { }
        }

        return foundPaks;
    }


    public void ExtractAllPaks()
    {
        SafeRemoveDirectory(config.ExtractDirectory);

        foreach (var pakInfo in Paks)
        {
            string extractPath = Path.Combine(config.ExtractDirectory, pakInfo.PakType.ToString(), pakInfo.ModName);
            Directory.CreateDirectory(extractPath);

            string command = $"\"{config.UnrealPak}\" \"{pakInfo.Path}\" -Extract \"{extractPath}\"";
            var process = Process.Start(command);
            process.WaitForExit();
        }
    }

    public void Clean()
    {
        foreach (var dir in new [] { config.TempDirectory, config.ExtractDirectory })
        {
            SafeRemoveDirectory(dir);
        }

        allPaks.Clear();
        Mods.Clear();
        MainMod = null;
    }

    private static void SafeRemoveDirectory(string dir)
    {
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }
}