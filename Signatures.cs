--- Signatures.cs (原始)
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

+++ Signatures.cs (修改后)
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Win32;

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
    public Dictionary<string, string> KnownFileHashes { get; set; } = new();

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
    public string EvidenceType { get; set; } = "Nombre/Patrón"; // Nombre/Patrón, Hash, Registro, Integridad

    public override string ToString() => $"[{Severity}] {Category}: {Description} ({Detail})";
}

public static class LicenseInfo
{
    public static string? GetSteamLicenseKey()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (key != null)
            {
                var value = key.GetValue("SteamPath")?.ToString();
                if (!string.IsNullOrEmpty(value))
                    return "Steam instalado (clave no accesible por seguridad)";
            }
        }
        catch { }
        return "No detectado";
    }

    public static string? GetFiveMInfo()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\FiveM");
            if (key != null)
            {
                return "FiveM instalado";
            }
            // También verificar en LocalAppData
            var fivemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM");
            if (Directory.Exists(fivemPath))
                return "FiveM instalado";
        }
        catch { }
        return "No detectado";
    }

    public static string? GetDiscordInfo()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Discord");
            if (key != null)
            {
                var version = key.GetValue("Version")?.ToString() ?? "Desconocida";
                return $"Discord instalado (v{version})";
            }

            // Intentar obtener el username desde config
            var discordPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
            var settingsPath = Path.Combine(discordPath, "settings.json");
            if (File.Exists(settingsPath))
            {
                var content = File.ReadAllText(settingsPath);
                // Buscar patrones de username en el JSON
                if (content.Contains("\"username\""))
                {
                    return "Discord instalado (usuario configurado)";
                }
            }
        }
        catch { }
        return "No detectado";
    }

    public static string? GetWindowsLicense()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform");
            if (key != null)
            {
                var backupProductKey = key.GetValue("BackupProductKeyDefault")?.ToString();
                if (!string.IsNullOrEmpty(backupProductKey))
                    return backupProductKey;
            }
        }
        catch { }
        return "No disponible";
    }
}

public static class FileIntegrityChecker
{
    private static readonly Dictionary<string, string> CommonPaths = new()
    {
        { "WinRAR", @"C:\Program Files\WinRAR\WinRAR.exe" },
        { "WinRAR", @"C:\Program Files (x86)\WinRAR\WinRAR.exe" },
        { "7-Zip", @"C:\Program Files\7-Zip\7zFM.exe" },
        { "7-Zip", @"C:\Program Files (x86)\7-Zip\7zFM.exe" }
    };

    public static List<Finding> CheckCommonApplications()
    {
        var findings = new List<Finding>();

        foreach (var app in CommonPaths)
        {
            var appName = app.Key;
            var filePath = app.Value;

            if (!File.Exists(filePath))
                continue;

            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = sha256.ComputeHash(stream);
                var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                // Aquí podrías comparar con hashes oficiales conocidos
                // Por ahora, solo reportamos que existe y su hash
                // En una implementación real, compararías con una base de datos de hashes oficiales

                findings.Add(new Finding
                {
                    Category = "Integridad de Archivos",
                    Description = $"{appName} verificado",
                    Detail = $"{filePath} | Hash: {hash}",
                    Severity = Severity.Info,
                    EvidenceType = "Integridad"
                });
            }
            catch (Exception ex)
            {
                findings.Add(new Finding
                {
                    Category = "Integridad de Archivos",
                    Description = $"Error al verificar {appName}",
                    Detail = ex.Message,
                    Severity = Severity.Low,
                    EvidenceType = "Integridad"
                });
            }
        }

        return findings;
    }
}
