using System.Diagnostics;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UnrealRepacker;


class PakTool
{
    Config config;

    public PakTool(Config config)
    {
        this.config = config;
    }

    public void UnpackPakFile(string unrealPakExePath, PakInfo pakInfo)
    {
        string extractPath = Path.Combine(config.ExtractDirectory, pakInfo.pakName);
        Directory.CreateDirectory(extractPath);

        string command = $"\"{unrealPakExePath}\" \"{pakInfo.path}\" -Extract \"{extractPath}\"";
        var process = Process.Start(command);
        process.WaitForExit();
    }


    private IEnumerable<string> SearchDependenciesRecursive(string rootDirectory, IEnumerable<string> imports, string modName)
    {
        HashSet<string> totalDependencyPaths = new(imports.Select(x => ImportToPath(rootDirectory, x)));
        Stack<string> pathsToProcess = new(totalDependencyPaths);

        while (pathsToProcess.Count > 0)
        {
            var uassetPath = pathsToProcess.Pop();
            UAsset uasset = new(uassetPath, EngineVersion.VER_UE4_26);

            // filter imports that start with modname
            // convert imports to uasset paths
            var importPaths = GetUassetImports(uasset)
                .Where(x => x.StartsWith($"/{modName}/"))
                .Select(x => ImportToPath(rootDirectory, x));
            
            foreach (var importPath in importPaths)
            {
                if (totalDependencyPaths.Add(importPath))
                {
                    pathsToProcess.Push(importPath);
                }
            }
        }

        return totalDependencyPaths;
    }

    public void ExtractPaks(ModInfo modInfo)
    {
        // clean extract directory and unpack all paks
        if (Directory.Exists(config.ExtractDirectory))
            Directory.Delete(config.ExtractDirectory, true);

        foreach (var pakInfo in modInfo.paks)
        {
            UnpackPakFile(config.UnrealPak, pakInfo);

            // rename extracted/pakname/content to extracted/pakname/modname
            string originalContentPath = Path.Combine(config.ExtractDirectory, pakInfo.pakName, "Content");
            string newModPath = Path.Combine(config.ExtractDirectory, pakInfo.pakName, modInfo.modName);
            Directory.Move(originalContentPath, newModPath);
        }
    }

    public void PackUassetFiles(IEnumerable<string> imports, ModInfo modInfo, bool removeMetadata)
    {
        // for every mod, pack every selected uassets
        foreach (var pakInfo in modInfo.paks)
        {
            PackUassetFiles(modInfo, pakInfo, imports, removeMetadata);
        }
    }

    private void PackUassetFiles(ModInfo modInfo, PakInfo pakInfo, IEnumerable<string> imports, bool removeMetadata)
    {
        Debug.WriteLine($"-------------- {pakInfo.pakName} ---------------");
        string currentExtractedPakDir = Path.Combine(config.ExtractDirectory, pakInfo.pakName);
        var uassetFiles = SearchDependenciesRecursive(currentExtractedPakDir, imports, modInfo.modName);
        var depsWithoutExtension = uassetFiles.Select(x => Path.ChangeExtension(x, null));

        // Remove all files and directories in the extracted pak directory except for the ones we're interested in
        DirectoryInfo di = new DirectoryInfo(currentExtractedPakDir);
        foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories))
        {
            // check for if file without extension not is in the deps list - remove it
            var currentFileWithoutExtension = Path.ChangeExtension(file.FullName, null);
            if (depsWithoutExtension.Contains(currentFileWithoutExtension))
            {
                continue;
            }

            var fileRelativePath = Path.GetRelativePath(currentExtractedPakDir, file.FullName);
            if (!removeMetadata && fileRelativePath.StartsWith("Metadata\\"))
            {
                continue;
            }

            if (fileRelativePath == "AssetRegistry.bin")
            {
                continue;
            }

            // file.Delete();
            File.Delete(@"\\?\" + file.FullName);
        }

        // remove empty directories
        foreach (DirectoryInfo dir in di.GetDirectories("*", SearchOption.AllDirectories))
        {
            var dirPath = @"\\?\" + dir.FullName;
            if (Directory.Exists(dirPath)
                && Directory.GetFiles(dirPath ,"*", SearchOption.AllDirectories).Length == 0)
            {
                Directory.Delete(dirPath, true);
            }
        }

        // rename directory extracted/pakname/modname to extracted/pakname/Content
        string modDirectoryPath = Path.Combine(currentExtractedPakDir, modInfo.modName);
        string contentDirectoryPath = Path.Combine(currentExtractedPakDir, "Content");
        Directory.Move(modDirectoryPath, contentDirectoryPath);

        // create ResponseFile
        string responseFilePath = Path.Combine(config.ExtractDirectory, "ResponseFile.txt");
        string responseFileContent = $"\"{Path.Combine(currentExtractedPakDir, "*.*")}\" \"../../../Mordhau/Mods/{modInfo.modName}/*.*\" -compress";
        File.WriteAllText(responseFilePath, responseFileContent);

        // build command for execution
        string command = $"\"{config.UnrealPak}\" \"{Path.Combine(Directory.GetCurrentDirectory(), "packed", pakInfo.pakName)}.pak\" \"-Create={responseFilePath}\"";

        // run process and wait for it to complete
        var process = Process.Start(command);
        process.WaitForExit();
    }

    private static IEnumerable<string> GetUassetImports(UAsset uasset)
    {
        foreach (var import in uasset.Imports)
        {
            if (import.ClassName.ToString() == "Package")
            {
                yield return import.ObjectName.ToString();
            }
        }
    }
    private string ImportToPath(string rootDirectory, string import)
    {
        var parts = import.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Prepend(rootDirectory);
        return Path.Combine(parts.ToArray()) + ".uasset";
    }

    public IEnumerable<string> GetExtractedUassets(string pakname)
    {
        string extractPath = Path.Combine(config.ExtractDirectory, pakname);

        return Directory.
            EnumerateFiles(extractPath, "*.uasset", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(extractPath, x))
            .Select(x => Path.ChangeExtension(x, null));
    }
}