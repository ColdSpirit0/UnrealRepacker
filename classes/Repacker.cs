using System.Diagnostics;
using System.Net.Security;
using System.Text.RegularExpressions;
using UAssetAPI;
using UAssetAPI.UnrealTypes.EngineEnums;

namespace UnrealRepacker;

public class Repacker(Config config)
{
    private Config config = config;


    public List<ModInfo> Mods { get; } = [];
    public ModInfo? MainMod { get; set; }

    public List<PakInfo> Paks { get; } = [];


    public void AddPak(PakInfo pak)
    {
        var mod = Mods.FirstOrDefault(m => m.modName == pak.ModName);

        if (mod == null)
        {
            mod = new ModInfo(pak.ModName);
            Mods.Add(mod);
        }

        mod.paks.Add(pak);
        Paks.Add(pak);

        MainMod ??= mod;
    }

    public IEnumerable<string> GetMainModImports()
    {
        string searchRoot = Path.Combine(config.ExtractDirectory, PakNetworkType.Server.ToString());
        string searchPath = Path.Combine(searchRoot, MainMod!.modName);

        return Directory
            .EnumerateFiles(searchPath, "*.uasset", SearchOption.AllDirectories)
            .Select(p => AssetHelper.PathToImport(searchRoot, p));
    }


    public void Pack(IEnumerable<string> imports, string newModName)
    {
        // clean temp directory
        if (Directory.Exists(config.TempDirectory))
        {
            Directory.Delete(config.TempDirectory, true);
        }

        foreach (var pak in MainMod!.paks)
        {
            Pack(imports, pak, newModName);
        }
    }

    private void Pack(IEnumerable<string> imports, PakInfo targetPak, string newModName)
    {
        var directory = Path.Combine(config.ExtractDirectory, targetPak.NetworkType.ToString());
        var uassetDependencies = AssetHelper.SearchDependencies(imports, directory);
        // var filesToPack = GetFilesToPack(targetPak, uassetDependencies);

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
            if (dependencyModName != MainMod!.modName)
            {
                parts.InsertRange(0, ["__IMPORTS__", dependencyModName]);
            }

            // all assets should be in Content
            parts.Insert(0, "Content");

            // get new abs path
            var newPathRelative = string.Join(Path.DirectorySeparatorChar, parts);
            string destUasset = Path.Combine(config.TempDirectory, targetPak.NetworkType.ToString(), newPathRelative);

            // change imports and save
            var additionalMods = Mods.Where(m => m != MainMod).Select(m => m.modName).ToArray();
            AssetHelper.ChangeImportsAndSave(sourceUasset, destUasset, MainMod!.modName, additionalMods, newModName);
        }

        CreatePak(targetPak, newModName);
    }

    private void CreatePak(PakInfo targetPak, string newModName)
    {
        // create dummy AssetRegistry.bin in temp directory
        File.Create(Path.Combine(config.TempDirectory, targetPak.NetworkType.ToString(), "AssetRegistry.bin"));

        // create ResponseFile
        // TODO: check for packing without asset registry, just from Content/*
        string responseFilePath = Path.Combine(config.TempDirectory, "ResponseFile.txt");
        string responseFileContent = $"\"{Path.Combine(config.TempDirectory, targetPak.NetworkType.ToString(), "*.*")}\" \"../../../Mordhau/Mods/{newModName}/*.*\" -compress";
        File.WriteAllText(responseFilePath, responseFileContent);

        // build command for execution
        // var pakPath = Path.Combine(config.PackedDirectory, targetPak.pakName) + ".pak";
        // FIXME: use better pak path
        var pakPath = Path.Combine(config.PackedDirectory, newModName + "Windows" + targetPak.NetworkType.ToString()) + ".pak";
        string command = $"\"{config.UnrealPak}\" \"{pakPath}\" \"-Create={responseFilePath}\"";

        // run process and wait for it to complete
        var process = Process.Start(command);
        process.WaitForExit();
    }

    private IEnumerable<string> GetFilesToPack(PakInfo pakInfo, IEnumerable<string> uassetDependencies)
    {
        // XXX: possibly add AssetRegistry.bin and Metadata/

        foreach (string dependency in uassetDependencies)
        {
            if (!File.Exists(dependency))
            {
                Debug.WriteLine($"Dependency file not found: \"{dependency}\"");
                continue;
            }

            foreach (var ext in new[] { ".uasset", ".uexp", ".ubulk" })
            {
                var currentExtDependency = Path.ChangeExtension(dependency, ext);
                if (File.Exists(currentExtDependency))
                {
                    yield return currentExtDependency;
                }
            }
        }
    }

    public IEnumerable<PakInfo> SearchPaksForSameMod(string pathToPak)
    {
        string? dir = Path.GetDirectoryName(pathToPak);
        if (dir == null) throw new Exception("Directory not found");

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
        // clean extract directory and unpack all paks
        if (Directory.Exists(config.ExtractDirectory))
        {
            Directory.Delete(config.ExtractDirectory, true);
        }

        // extract
        foreach (var pakInfo in Paks)
        {
            string extractPath = Path.Combine(config.ExtractDirectory, pakInfo.NetworkType.ToString(), pakInfo.ModName);
            Directory.CreateDirectory(extractPath);

            string command = $"\"{config.UnrealPak}\" \"{pakInfo.Path}\" -Extract \"{extractPath}\"";
            var process = Process.Start(command);
            process.WaitForExit();
        }
    }

    public void Clean()
    {
        throw new NotImplementedException();
        // remove extracted paks, clean Paks
        // Directory.Delete(config.ExtractDirectory, true);
        // Paks.Clear();
    }
}