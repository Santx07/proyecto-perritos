--- FileScanner.cs (原始)
namespace CheatScanner.Scanners;

public static class FileScanner
{
    public static List<Finding> Scan(SignatureDb db)
    {
        var findings = new List<Finding>();

        foreach (var rawDir in db.ScanDirectories)
        {
            var dir = Environment.ExpandEnvironmentVariables(rawDir);
            if (!Directory.Exists(dir)) continue;

            IEnumerable<string> entries;
            try
            {
                // Solo primer nivel + un nivel de subcarpetas para no tardar horas
                entries = Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                findings.Add(new Finding
                {
                    Category = "Archivos",
                    Description = $"No se pudo escanear carpeta {dir}",
                    Detail = ex.Message,
                    Severity = Severity.Info
                });
                continue;
            }

            foreach (var entry in entries)
            {
                var nameLower = Path.GetFileName(entry).ToLowerInvariant();
                var ext = Path.GetExtension(entry).ToLowerInvariant();

                bool nameMatches = db.FileFolderKeywords.Any(k => nameLower.Contains(k));
                bool extMatches = db.SuspiciousExtensions.Contains(ext);

                if (nameMatches)
                {
                    findings.Add(new Finding
                    {
                        Category = "Archivos/Carpetas",
                        Description = "Nombre coincide con palabra clave sospechosa",
                        Detail = entry,
                        Severity = extMatches ? Severity.High : Severity.Medium
                    });
                }
            }
        }

        return findings;
    }
}

+++ FileScanner.cs (修改后)
using System.Security.Cryptography;

namespace CheatScanner.Scanners;

public static class FileScanner
{
    // Hashes conocidos de aplicaciones comunes (ejemplo - deberías agregar más)
    private static readonly Dictionary<string, string> KnownHashes = new()
    {
        // Agrega aquí hashes oficiales de WinRAR, 7-Zip, etc.
        // Ejemplo: { "winrar.exe", "hash_oficial_sha256" }
    };

    public static List<Finding> Scan(SignatureDb db)
    {
        var findings = new List<Finding>();

        foreach (var rawDir in db.ScanDirectories)
        {
            var dir = Environment.ExpandEnvironmentVariables(rawDir);
            if (!Directory.Exists(dir)) continue;

            IEnumerable<string> entries;
            try
            {
                // Solo primer nivel + un nivel de subcarpetas para no tardar horas
                entries = Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                findings.Add(new Finding
                {
                    Category = "Archivos",
                    Description = $"No se pudo escanear carpeta {dir}",
                    Detail = ex.Message,
                    Severity = Severity.Info,
                    EvidenceType = "Nombre/Patrón"
                });
                continue;
            }

            foreach (var entry in entries)
            {
                var nameLower = Path.GetFileName(entry).ToLowerInvariant();
                var ext = Path.GetExtension(entry).ToLowerInvariant();

                bool nameMatches = db.FileFolderKeywords.Any(k => nameLower.Contains(k));
                bool extMatches = db.SuspiciousExtensions.Contains(ext);

                if (nameMatches)
                {
                    // Calcular hash para archivos ejecutables
                    string? fileHash = null;
                    if (ext == ".exe" || ext == ".dll")
                    {
                        try
                        {
                            fileHash = CalculateSHA256(entry);
                        }
                        catch { }
                    }

                    findings.Add(new Finding
                    {
                        Category = "Archivos/Carpetas",
                        Description = "Nombre coincide con palabra clave sospechosa",
                        Detail = entry + (fileHash != null ? $" | Hash: {fileHash}" : ""),
                        Severity = extMatches ? Severity.High : Severity.Medium,
                        EvidenceType = fileHash != null ? "Hash" : "Nombre/Patrón"
                    });
                }
            }
        }

        // Escanear aplicaciones comunes en busca de modificaciones
        findings.AddRange(ScanCommonApplications());

        return findings;
    }

    private static List<Finding> ScanCommonApplications()
    {
        var findings = new List<Finding>();

        // Aplicaciones comunes que suelen ser modificadas
        var commonApps = new Dictionary<string, string[]>
        {
            { "WinRAR", new[] {
                @"C:\Program Files\WinRAR\WinRAR.exe",
                @"C:\Program Files (x86)\WinRAR\WinRAR.exe"
            }},
            { "7-Zip", new[] {
                @"C:\Program Files\7-Zip\7zFM.exe",
                @"C:\Program Files (x86)\7-Zip\7zFM.exe"
            }},
            { "Steam", new[] {
                @"C:\Program Files (x86)\Steam\steam.exe"
            }}
        };

        foreach (var app in commonApps)
        {
            foreach (var filePath in app.Value)
            {
                if (!File.Exists(filePath))
                    continue;

                try
                {
                    var hash = CalculateSHA256(filePath);
                    var fileInfo = new FileInfo(filePath);

                    // Verificar si el hash coincide con alguno conocido (si está en la base de datos)
                    bool isModified = false;
                    if (KnownHashes.ContainsKey(Path.GetFileName(filePath).ToLowerInvariant()))
                    {
                        if (KnownHashes[Path.GetFileName(filePath).ToLowerInvariant()] != hash)
                        {
                            isModified = true;
                        }
                    }

                    findings.Add(new Finding
                    {
                        Category = "Integridad de Aplicaciones",
                        Description = $"{app.Key} verificado{(isModified ? " - ¡POSIBLE MODIFICACIÓN!" : "")}",
                        Detail = $"{filePath}\nTamaño: {fileInfo.Length:N0} bytes | Hash SHA256: {hash}\nÚltima modificación: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}",
                        Severity = isModified ? Severity.High : Severity.Info,
                        EvidenceType = "Integridad"
                    });
                }
                catch (Exception ex)
                {
                    findings.Add(new Finding
                    {
                        Category = "Integridad de Aplicaciones",
                        Description = $"Error al verificar {app.Key}",
                        Detail = $"{filePath}: {ex.Message}",
                        Severity = Severity.Low,
                        EvidenceType = "Integridad"
                    });
                }
            }
        }

        return findings;
    }

    private static string CalculateSHA256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
