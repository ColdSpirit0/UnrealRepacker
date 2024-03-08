namespace UnrealRepacker;

public class ModInfo(string modname)
{
    public string modName = modname;
    public List<PakInfo> paks = [];

    public override string ToString() => modName;
}