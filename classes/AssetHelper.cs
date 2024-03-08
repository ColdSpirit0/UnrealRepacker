using UAssetAPI;

using UAssetAPI.UnrealTypes;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UnrealRepacker;

public static class AssetHelper
{
    /// <summary>
    /// returns absolute path
    /// </summary>
    public static string ImportToPath(string pakDirectory, string import)
    {
        return Path.Combine(pakDirectory, ImportToPath(import));
    }

    /// <summary>
    /// returns relative path
    /// </summary>
    public static string ImportToPath(string import)
    {
        var importParts = import.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        importParts.Insert(1, "Content");

        return Path.Combine(importParts.ToArray()) + ".uasset";
    }

    public static string PathToImport(string pakDirectory, string path)
    {
        string relativePath = Path.GetRelativePath(pakDirectory, path);
        return PathToImport(relativePath);
    }

    public static string PathToImport(string relativePath)
    {
        // remove extension
        // remove Content part
        // replace / with \
        relativePath = Path.ChangeExtension(relativePath, null);
        var splitted = relativePath.Split(Path.DirectorySeparatorChar).ToList();
        splitted.RemoveAt(1);
        return string.Join("/", splitted);
    }

    public static IEnumerable<string> SearchDependencies(IEnumerable<string> targetImports, string pakDirectory)
    {
        HashSet<string> totalDependencyPaths = new(targetImports.Select(x => ImportToPath(pakDirectory, x)));
        Stack<string> pathsToProcess = new(totalDependencyPaths);

        while (pathsToProcess.Count > 0)
        {
            var uassetPath = pathsToProcess.Pop();
            if (!Path.Exists(uassetPath)) continue;

            UAsset uasset = new(uassetPath, EngineVersion.VER_UE4_26);

            // filter imports that start with modname
            // convert imports to uasset paths
            var importPaths = GetUassetImports(uasset)
                // .Where(x => x.StartsWith($"/{modName}/"))
                .Select(x => ImportToPath(pakDirectory, x));
            
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

    public static void ChangeImportsAndSave(string srcUassetPath, string dstUassetPath, string mainPackage, string[] additionalPackages, string newPackageName)
    {
        UAsset uasset = new(srcUassetPath, EngineVersion.VER_UE4_26);
        var names = uasset.GetNameMapIndexList();
        foreach (var name in names)
        {
            // replace mod name if the main package
            if (name.Value.StartsWith($"/{mainPackage}/"))
            {
                name.Value = Regex.Replace(name.Value, $"^/{mainPackage}/", $"/{newPackageName}/");
                continue;
            }

            // put additional packages in __imports__
            foreach (var additionalPackage in additionalPackages)
            {
                if (name.Value.StartsWith($"/{additionalPackage}/"))
                {
                    name.Value = Regex.Replace(name.Value, $"^/{additionalPackage}/", $"/{newPackageName}/__IMPORTS__/{additionalPackage}/");
                }
            }
        }
        Directory.CreateDirectory(Path.GetDirectoryName(dstUassetPath));
        uasset.Write(dstUassetPath);
    }

    private static IEnumerable<string> GetUassetImports(UAsset uasset)
    {
        return from import in uasset.Imports
               where import.ClassName.ToString() == "Package"
               select import.ObjectName.ToString();
    }
}