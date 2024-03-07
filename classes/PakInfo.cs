using System.Text.RegularExpressions;

namespace UnrealRepacker;

public struct PakInfo
{
    public string path;
    public string pakName;
    public string modName;
    public PakNetworkType networkType;
    public PakOsType pakOsType;

    public PakInfo(string path)
    {
        this.path = path;
        pakName = Path.GetFileNameWithoutExtension(path);

        string pattern = $@".+\{Path.DirectorySeparatorChar}(.+?)(Linux|Windows)(Server|Client)\.pak";
        Match match = Regex.Match(path, pattern);

        if (match.Success)
        {
            modName = match.Groups[1].Value;
            pakOsType = Enum.Parse<PakOsType>(match.Groups[2].Value);
            networkType = Enum.Parse<PakNetworkType>(match.Groups[3].Value);
            return;
        }

        throw new Exception("File doesn't match pattern <ModName><OS><Network>.pak");
    }
}
