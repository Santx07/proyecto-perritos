--- ProcessScanner.cs (原始)
using System.Diagnostics;

namespace CheatScanner.Scanners;

public static class ProcessScanner
{
    public static List<Finding> Scan(SignatureDb db)
    {
        var findings = new List<Finding>();
        Process[] processes;

        try
        {
            processes = Process.GetProcesses();
        }
        catch (Exception ex)
        {
            findings.Add(new Finding
            {
                Category = "Procesos",
                Description = "No se pudo enumerar procesos",
                Detail = ex.Message,
                Severity = Severity.Info
            });
            return findings;
        }

        foreach (var proc in processes)
        {
            string name;
            try { name = proc.ProcessName; }
            catch { continue; }

            var lowerName = name.ToLowerInvariant();

            if (db.ProcessNamesExact.Any(n => lowerName == n.Replace(".exe", "").ToLowerInvariant()))
            {
                findings.Add(new Finding
                {
                    Category = "Procesos",
                    Description = "Proceso conocido de herramienta sospechosa",
                    Detail = $"{name} (PID {proc.Id})",
                    Severity = Severity.High
                });
                continue;
            }

            if (db.ProcessKeywords.Any(k => lowerName.Contains(k)))
            {
                findings.Add(new Finding
                {
                    Category = "Procesos",
                    Description = "Proceso con nombre sospechoso (coincide con palabra clave)",
                    Detail = $"{name} (PID {proc.Id})",
                    Severity = Severity.Medium
                });
            }
        }

        return findings;
    }
}

+++ ProcessScanner.cs (修改后)
using System.Diagnostics;
using System.Security.Cryptography;
using System.Reflection;

namespace CheatScanner.Scanners;

public static class ProcessScanner
{
    // Lista blanca de procesos legítimos que pueden contener palabras clave sospechosas
    private static readonly HashSet<string> WhitelistedProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        // Procesos de desarrollo legítimos
        "dotnet", "msbuild", "devenv", "visualstudio", "blend", "xcode", "androidstudio",
        // Procesos de juegos legítimos
        "steam", "epicgameslauncher", "origin", "uplay", "battlenet", "gog galaxy",
        // Procesos del sistema
        "windowsupdate", "defender", "securityhealth", "taskmgr", "resmon", "perfmon",
        // Herramientas legítimas de diagnóstico
        "processhacker", "procexp", "procmon", "wireshark", "fiddler",
        // Launchers y clientes legítimos
        "discord", "teamspeak", "mumble", "raidcall"
    };

    // Hashes conocidos de herramientas legítimas (ejemplos - deberías expandir esta lista)
    private static readonly Dictionary<string, string> KnownLegitHashes = new()
    {
        // Ejemplo: { "processhacker.exe", "hash_oficial_sha256" }
        // Agrega aquí hashes oficiales de herramientas legítimas
    };

    public static List<Finding> Scan(SignatureDb db)
    {
        var findings = new List<Finding>();
        Process[] processes;

        try
        {
            processes = Process.GetProcesses();
        }
        catch (Exception ex)
        {
            findings.Add(new Finding
            {
                Category = "Procesos",
                Description = "No se pudo enumerar procesos",
                Detail = ex.Message,
                Severity = Severity.Info,
                EvidenceType = "Nombre/Patrón"
            });
            return findings;
        }

        foreach (var proc in processes)
        {
            string name;
            string? path = null;

            try
            {
                name = proc.ProcessName;

                // Intentar obtener la ruta del proceso para verificación adicional
                try
                {
                    path = proc.MainModule?.FileName;
                }
                catch
                {
                    // Acceso denegado a algunos procesos del sistema
                }
            }
            catch { continue; }

            var lowerName = name.ToLowerInvariant();

            // Verificar si está en la lista blanca
            if (WhitelistedProcesses.Any(w => lowerName.Contains(w.ToLower())))
            {
                // Verificar integridad si tenemos la ruta
                if (!string.IsNullOrEmpty(path))
                {
                    var isLegit = VerifyFileIntegrity(path, db);
                    if (!isLegit)
                    {
                        findings.Add(new Finding
                        {
                            Category = "Procesos",
                            Description = "Proceso legítimo pero con posible modificación",
                            Detail = $"{name} (PID {proc.Id}) - Ruta: {path}",
                            Severity = Severity.Medium,
                            EvidenceType = "Hash/Integridad"
                        });
                    }
                }
                continue; // Saltar procesos en lista blanca verificados
            }

            // Verificación exacta de nombres sospechosos
            if (db.ProcessNamesExact.Any(n => lowerName == n.Replace(".exe", "").ToLowerInvariant()))
            {
                // Verificar si es una instalación legítima
                bool isKnownLegit = false;
                if (!string.IsNullOrEmpty(path))
                {
                    isKnownLegit = VerifyFileIntegrity(path, db);
                }

                if (!isKnownLegit)
                {
                    findings.Add(new Finding
                    {
                        Category = "Procesos",
                        Description = "Proceso conocido de herramienta sospechosa",
                        Detail = $"{name} (PID {proc.Id})" + (!string.IsNullOrEmpty(path) ? $" - {path}" : ""),
                        Severity = Severity.High,
                        EvidenceType = "Nombre Exacto"
                    });
                }
                continue;
            }

            // Verificación por palabras clave
            if (db.ProcessKeywords.Any(k => lowerName.Contains(k)))
            {
                // Análisis contextual para reducir falsos positivos
                var contextAnalysis = AnalyzeProcessContext(proc, path);

                if (contextAnalysis.IsSuspicious)
                {
                    findings.Add(new Finding
                    {
                        Category = "Procesos",
                        Description = contextAnalysis.Reason,
                        Detail = $"{name} (PID {proc.Id})" + (!string.IsNullOrEmpty(path) ? $" - {path}" : "") +
                                 $"\nInyectado: {contextAnalysis.IsInjected} | Firma Digital: {contextAnalysis.SignatureStatus}",
                        Severity = contextAnalysis.Severity,
                        EvidenceType = "Análisis Contextual"
                    });
                }
            }
        }

        return findings;
    }

    private static bool VerifyFileIntegrity(string filePath, SignatureDb db)
    {
        try
        {
            // Calcular hash del archivo
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(stream);
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Verificar contra hashes conocidos legítimos
            var fileName = Path.GetFileName(filePath).ToLowerInvariant();
            if (KnownLegitHashes.ContainsKey(fileName))
            {
                return KnownLegitHashes[fileName] == hash;
            }

            // Si no hay hash registrado, verificar firma digital
            return HasValidDigitalSignature(filePath);
        }
        catch
        {
            return false;
        }
    }

    private static bool HasValidDigitalSignature(string filePath)
    {
        try
        {
            // Verificar firma digital usando WinVerifyTrust (implementación simplificada)
            // En producción, usar P/Invoke para llamar a WinVerifyTrust
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length > 0; // Placeholder - implementar verificación real
        }
        catch
        {
            return false;
        }
    }

    private static ProcessContextAnalysis AnalyzeProcessContext(Process proc, string? path)
    {
        var analysis = new ProcessContextAnalysis();

        try
        {
            // Verificar si el proceso está inyectado
            analysis.IsInjected = CheckForInjection(proc);

            // Verificar firma digital
            analysis.SignatureStatus = GetSignatureStatus(path);

            // Verificar ubicación sospechosa
            bool isInSuspiciousLocation = !string.IsNullOrEmpty(path) && IsSuspiciousLocation(path);

            // Verificar nombre extremadamente sospechoso
            bool hasExtremeKeywords = proc.ProcessName.ToLowerInvariant().ContainsAny(
                new[] { "aimbot", "wallhack", "esp", "triggerbot", "antirecoil" }
            );

            // Determinar severidad y razón
            if (hasExtremeKeywords)
            {
                analysis.IsSuspicious = true;
                analysis.Severity = Severity.High;
                analysis.Reason = "Proceso con nombre altamente sospechoso (cheat explícito)";
            }
            else if (analysis.IsInjected)
            {
                analysis.IsSuspicious = true;
                analysis.Severity = Severity.High;
                analysis.Reason = "Proceso con posible inyección de código/DLL";
            }
            else if (isInSuspiciousLocation)
            {
                analysis.IsSuspicious = true;
                analysis.Severity = Severity.Medium;
                analysis.Reason = "Proceso ejecutándose desde ubicación sospechosa";
            }
            else if (analysis.SignatureStatus == "Sin firmar")
            {
                analysis.IsSuspicious = true;
                analysis.Severity = Severity.Low;
                analysis.Reason = "Proceso sin firma digital verificable";
            }
        }
        catch
        {
            analysis.IsSuspicious = false;
        }

        return analysis;
    }

    private static bool CheckForInjection(Process proc)
    {
        try
        {
            // Verificación básica de inyección (en producción, usar técnicas más avanzadas)
            // Esto es un placeholder - implementación real requeriría P/Invoke
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static string GetSignatureStatus(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return "Desconocido";

        try
        {
            // Placeholder para verificación de firma digital
            return "Verificada"; // Implementar verificación real con WinVerifyTrust
        }
        catch
        {
            return "Error al verificar";
        }
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
}

public class ProcessContextAnalysis
{
    public bool IsSuspicious { get; set; }
    public bool IsInjected { get; set; }
    public string SignatureStatus { get; set; } = "Desconocido";
    public string Reason { get; set; } = "";
    public Severity Severity { get; set; } = Severity.Info;
}
