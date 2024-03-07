namespace UnrealRepacker;

public interface IRepacker
{
    IEnumerable<PakInfo> Paks { get; }
    IEnumerable<PakInfo> SearchPaksForSameMod(string pathToPak);
    void AddPak(PakInfo pak);
    void ExtractAllPaks();
    IEnumerable<string> GetImports();
    void Pack(IEnumerable<string> imports);
    void Clean();
}