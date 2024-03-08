using System.Text.RegularExpressions;

namespace UnrealRepacker;

public class PakInfo
{
    public string Path { get; private set; }
    public string PakName { get; private set; }
    public string ModName { get; private set; }
    public PakNetworkType NetworkType { get; private set; }
    public PakOsType PakOsType { get; private set; }


    public PakInfo(string path)
    {
        Path = path;
        PakName = System.IO.Path.GetFileNameWithoutExtension(path);

        string pattern = $@".+\{System.IO.Path.DirectorySeparatorChar}(.+?)(Linux|Windows)(Server|Client)\.pak";
        Match match = Regex.Match(path, pattern);

        if (match.Success)
        {
            ModName = match.Groups[1].Value;
            PakOsType = Enum.Parse<PakOsType>(match.Groups[2].Value);
            NetworkType = Enum.Parse<PakNetworkType>(match.Groups[3].Value);
            return;
        }

        throw new Exception("File doesn't match pattern <ModName><OS><Network>.pak");
    }
}
