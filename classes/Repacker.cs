using System.Diagnostics;
using System.Text.RegularExpressions;
using UAssetAPI;

namespace UnrealRepacker;

public class Repacker(Config config) : IRepacker
{
    private List<PakInfo> paks = [];
    public IEnumerable<PakInfo> Paks { get => paks; }

    private Config config = config;

    public void AddPak(PakInfo pak) => paks.Add(pak);

    public PakInfo[] GetMainModPaks()
    {
        // currently main mod is the first added
        var firstPak = Paks.First();

        // search paks with same mod name
        return Paks.Where(pak => pak.modName == firstPak.modName).ToArray();
    }

    public IEnumerable<string> GetImports()
    {
        string searchPath = Path.Combine(config.ExtractDirectory, PakNetworkType.Server.ToString());

        return Directory
            .EnumerateFiles(searchPath, "*.uasset", SearchOption.AllDirectories)
            .Select(p => AssetHelper.PathToImport(searchPath, p));
    }

    public void Pack(IEnumerable<string> imports)
    {
        // clean temp directory
        if (Directory.Exists(config.TempDirectory))
        {
            Directory.Delete(config.TempDirectory, true);
        }

        var mainPaks = GetMainModPaks();
        foreach (var pak in mainPaks)
        {
            Pack(imports, pak);
        }
    }

    private void Pack(IEnumerable<string> imports, PakInfo targetPak)
    {
        var newModName = "MyMod";
        var directory = Path.Combine(config.ExtractDirectory, targetPak.networkType.ToString());
        var uassetDependencies = AssetHelper.SearchDependencies(imports, directory);
        var filesToPack = GetFilesToPack(targetPak, uassetDependencies);

        // copy files from extract directory to temp directory
        foreach (var file in filesToPack)
        {
            // remove first directory (mod name) from relative path
            // needed to merge files from different mods
            var relativePath = Path.GetRelativePath(directory, file);
            var destRelativePath = string.Join("\\", relativePath.Split(Path.DirectorySeparatorChar).Skip(1));

            // copy file
            string destFileName = Path.Combine(config.TempDirectory, targetPak.networkType.ToString(), destRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destFileName));


            // File.Copy(file, destFileName);

            if (Path.GetExtension(file) == ".uasset")
            {
                string[] additionalPackages = paks.Select(x => x.modName).Distinct().Where(x => x != targetPak.modName).ToArray();
                AssetHelper.ChangeImports(file, destFileName, targetPak.modName, additionalPackages, newModName);
            }
        }

        // create dummy AssetRegistry.bin in temp directory
        File.Create(Path.Combine(config.TempDirectory, targetPak.networkType.ToString(), "AssetRegistry.bin"));

        // create ResponseFile
        // TODO: check for packing without asset registry, just from Content/*
        string responseFilePath = Path.Combine(config.TempDirectory, "ResponseFile.txt");
        string responseFileContent = $"\"{Path.Combine(config.TempDirectory, targetPak.networkType.ToString(), "*.*")}\" \"../../../Mordhau/Mods/{newModName}/*.*\" -compress";
        File.WriteAllText(responseFilePath, responseFileContent);

        // build command for execution
        // var pakPath = Path.Combine(config.PackedDirectory, targetPak.pakName) + ".pak";
        // FIXME
        var pakPath = Path.Combine(config.PackedDirectory, newModName + "Windows" + targetPak.networkType.ToString()) + ".pak";
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
                if (pakInfo.modName == targetPak.modName)
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
            string extractPath = Path.Combine(config.ExtractDirectory, pakInfo.networkType.ToString(), pakInfo.modName);
            Directory.CreateDirectory(extractPath);

            string command = $"\"{config.UnrealPak}\" \"{pakInfo.path}\" -Extract \"{extractPath}\"";
            var process = Process.Start(command);
            process.WaitForExit();
        }
    }

    public void Clean()
    {
        // remove extracted paks, clean Paks
        Directory.Delete(config.ExtractDirectory, true);
        paks = [];
    }
}