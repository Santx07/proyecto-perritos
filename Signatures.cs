using System.Text.Json;

namespace CheatScanner;

public class SignatureDb
{
    public List<string> ProcessKeywords { get; set; } = new();
    public List<string> ProcessNamesExact { get; set; } = new();
    public List<string> FileFolderKeywords { get; set; } = new();
    public List<string> SuspiciousExtensions { get; set; } = new();
    public List<string> ScanDirectories { get; set; } = new();
    public List<string> RegistryUninstallKeywords { get; set; } = new();
    public List<string> RegistryRunKeys { get; set; } = new();
    public List<string> ServiceKeywords { get; set; } = new();

    public static SignatureDb Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"No se encontró el archivo de firmas: {path}");

        var json = File.ReadAllText(path);
        var db = JsonSerializer.Deserialize<SignatureDb>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return db ?? new SignatureDb();
    }
}

public enum Severity { Info, Low, Medium, High }

public class Finding
{
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string Detail { get; set; } = "";
    public Severity Severity { get; set; } = Severity.Info;

    public override string ToString() => $"[{Severity}] {Category}: {Description} ({Detail})";
}
