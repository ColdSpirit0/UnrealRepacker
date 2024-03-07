
using System.Runtime.InteropServices;

public class Config
{
    Dictionary<string, string> options = [];
    public string UnrealPak { get => options["UnrealPak"]; }
    public string WorkingDirectory { get; }
    public string PackedDirectory { get { return Path.Combine(WorkingDirectory, "packed"); } }
    public string ExtractDirectory { get { return Path.Combine(WorkingDirectory, "extracted"); } }
    public string TempDirectory { get { return Path.Combine(WorkingDirectory, "temp"); } }


    public Config(string workingDirectory)
    {
        WorkingDirectory = workingDirectory;
    }

    public void Load(string filename)
    {
        var lines = File.ReadAllLines(filename);

        foreach (var line in lines)
        {
            string[] parts = line.Split("=");
            string key = parts[0].Trim();
            string value = parts[1].Trim();

            options.Add(key, value);
        }
    }
}