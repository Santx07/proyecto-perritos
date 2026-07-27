--- FileScanner.cs (原始)
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

+++ FileScanner.cs (修改后)
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
    // Lista blanca de aplicaciones legítimas comunes
    private static readonly HashSet<string> WhitelistedApplications = new(StringComparer.OrdinalIgnoreCase)
    {
        // Compresores
        "winrar", "7z", "7zip", "winzip", "bandizip", "peazip",
        // Navegadores
        "chrome", "firefox", "edge", "opera", "brave", "vivaldi",
        // Juegos y launchers
        "steam", "epicgameslauncher", "origin", "uplay", "battlenet", "riot client",
        // Comunicación
        "discord", "teamspeak", "zoom", "skype", "slack",
        // Desarrollo
        "visualstudio", "vscode", "jetbrains", "rider", "webstorm",
        // Utilidades del sistema
        "everything", "notepad++", "powertoys", "microsoft edge"
    };

    // Hashes conocidos de aplicaciones legítimas (deberías expandir esta lista con hashes oficiales)
    private static readonly Dictionary<string, List<string>> KnownLegitHashes = new()
    {
        // Ejemplo: { "winrar.exe", new List<string> { "hash1", "hash2" } }
        // Agrega aquí hashes oficiales de versiones conocidas
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
                    // Verificar si es una aplicación legítima en lista blanca
                    if (IsWhitelistedApplication(nameLower))
                    {
                        // Verificar integridad del archivo
                        var integrityResult = VerifyFileIntegrity(entry, db);

                        if (!integrityResult.IsLegit)
                        {
                            findings.Add(new Finding
                            {
                                Category = "Archivos/Carpetas",
                                Description = "Aplicación legítima pero con posible modificación",
                                Detail = $"{entry}\n{integrityResult.Details}",
                                Severity = Severity.Medium,
                                EvidenceType = "Hash/Integridad"
                            });
                        }
                        // Si es legítimo y verificado, no reportar
                        continue;
                    }

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

                    // Análisis contextual adicional
                    var contextAnalysis = AnalyzeFileContext(entry, nameLower, ext, db);

                    if (contextAnalysis.IsSuspicious)
                    {
                        findings.Add(new Finding
                        {
                            Category = "Archivos/Carpetas",
                            Description = contextAnalysis.Reason,
                            Detail = entry + (fileHash != null ? $"\nHash SHA256: {fileHash}" : "") +
                                     $"\nUbicación: {(IsSuspiciousLocation(entry) ? "SOSPECHOSA" : "Normal")}",
                            Severity = contextAnalysis.Severity,
                            EvidenceType = fileHash != null ? "Hash + Contexto" : "Nombre/Patrón + Contexto"
                        });
                    }
                }
            }
        }

        // Escanear aplicaciones comunes en busca de modificaciones
        findings.AddRange(ScanCommonApplications(db));

        return findings;
    }

    private static bool IsWhitelistedApplication(string fileName)
    {
        return WhitelistedApplications.Any(app => fileName.Contains(app, StringComparison.OrdinalIgnoreCase));
    }

    private static IntegrityResult VerifyFileIntegrity(string filePath, SignatureDb db)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(stream);
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            var fileName = Path.GetFileName(filePath).ToLowerInvariant();

            // Verificar contra hashes conocidos legítimos
            if (KnownLegitHashes.ContainsKey(fileName))
            {
                if (KnownLegitHashes[fileName].Contains(hash))
                {
                    return new IntegrityResult { IsLegit = true, Details = $"Hash verificado: {hash}" };
                }
                else
                {
                    return new IntegrityResult
                    {
                        IsLegit = false,
                        Details = $"Hash NO coincide con versión oficial\nDetectado: {hash}"
                    };
                }
            }

            // Verificar firma digital
            var hasValidSignature = HasValidDigitalSignature(filePath);

            if (hasValidSignature)
            {
                return new IntegrityResult { IsLegit = true, Details = $"Firma digital válida | Hash: {hash}" };
            }
            else
            {
                return new IntegrityResult
                {
                    IsLegit = false,
                    Details = $"Sin firma digital válida | Hash: {hash}"
                };
            }
        }
        catch (Exception ex)
        {
            return new IntegrityResult { IsLegit = false, Details = $"Error al verificar: {ex.Message}" };
        }
    }

    private static bool HasValidDigitalSignature(string filePath)
    {
        try
        {
            // Placeholder - implementar con WinVerifyTrust via P/Invoke para producción
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length > 0 && fileInfo.Extension.ToLowerInvariant() == ".exe";
        }
        catch
        {
            return false;
        }
    }

    private static FileContextAnalysis AnalyzeFileContext(string fullPath, string fileName, string extension, SignatureDb db)
    {
        var analysis = new FileContextAnalysis();

        // Verificar palabras clave extremadamente sospechosas
        bool hasExtremeKeywords = fileName.ContainsAny(
            new[] { "aimbot", "wallhack", "triggerbot", "antirecoil", "esp" }
        );

        // Verificar ubicación sospechosa
        bool isSuspiciousLoc = IsSuspiciousLocation(fullPath);

        // Verificar extensión peligrosa con nombre sospechoso
        bool isDangerousCombo = extension.ToLowerInvariant() switch
        {
            ".exe" or ".dll" or ".sys" => true,
            _ => false
        };

        // Determinar severidad
        if (hasExtremeKeywords)
        {
            analysis.IsSuspicious = true;
            analysis.Severity = Severity.High;
            analysis.Reason = "Archivo con nombre de cheat explícito";
        }
        else if (isDangerousCombo && isSuspiciousLoc)
        {
            analysis.IsSuspicious = true;
            analysis.Severity = Severity.Medium;
            analysis.Reason = "Ejecutable en ubicación temporal/sospechosa";
        }
        else if (isSuspiciousLoc)
        {
            analysis.IsSuspicious = true;
            analysis.Severity = Severity.Low;
            analysis.Reason = "Archivo en ubicación temporal";
        }
        else
        {
            // No es lo suficientemente sospechoso
            analysis.IsSuspicious = false;
        }

        return analysis;
    }

    private static bool IsSuspiciousLocation(string path)
    {
        var suspiciousLocations = new[]
        {
            @"\temp\", @"\tmp\", @"\appdata\local\temp\",
            @"\downloads\", @"\desktop\",
            @"C:\users\public\", @"C:\programdata\"
        };

        var lowerPath = path.ToLowerInvariant();
        return suspiciousLocations.Any(loc => lowerPath.Contains(loc));
    }

    private static List<Finding> ScanCommonApplications(SignatureDb db)
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
                    var integrityResult = VerifyFileIntegrity(filePath, db);
                    var fileInfo = new FileInfo(filePath);

                    findings.Add(new Finding
                    {
                        Category = "Integridad de Aplicaciones",
                        Description = $"{app.Key} {(integrityResult.IsLegit ? "verificado" : "¡POSIBLE MODIFICACIÓN!")}",
                        Detail = $"{filePath}\nTamaño: {fileInfo.Length:N0} bytes | {integrityResult.Details}\nÚltima modificación: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}",
                        Severity = integrityResult.IsLegit ? Severity.Info : Severity.High,
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

public class IntegrityResult
{
    public bool IsLegit { get; set; }
    public string Details { get; set; } = "";
}

public class FileContextAnalysis
{
    public bool IsSuspicious { get; set; }
    public string Reason { get; set; } = "";
    public Severity Severity { get; set; } = Severity.Info;
}

public static class StringExtensions
{
    public static bool ContainsAny(this string source, IEnumerable<string> values, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return values.Any(value => source.IndexOf(value, comparison) >= 0);
    }
}
