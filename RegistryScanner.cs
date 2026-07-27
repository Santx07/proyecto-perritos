--- RegistryScanner.cs (原始)
using Microsoft.Win32;

namespace CheatScanner.Scanners;

public static class RegistryScanner
{
    public static List<Finding> Scan(SignatureDb db)
    {
        var findings = new List<Finding>();

        ScanUninstallList(db, findings);
        ScanRunKeys(db, findings);

        return findings;
    }

    private static void ScanUninstallList(SignatureDb db, List<Finding> findings)
    {
        string[] uninstallPaths =
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (var path in uninstallPaths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);
                if (key == null) continue;

                foreach (var subName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subName);
                    var displayName = subKey?.GetValue("DisplayName") as string;
                    if (string.IsNullOrEmpty(displayName)) continue;

                    var lower = displayName.ToLowerInvariant();
                    if (db.RegistryUninstallKeywords.Any(k => lower.Contains(k)))
                    {
                        findings.Add(new Finding
                        {
                            Category = "Registro (Programas instalados)",
                            Description = "Programa instalado con nombre sospechoso",
                            Detail = displayName,
                            Severity = Severity.High
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                findings.Add(new Finding
                {
                    Category = "Registro",
                    Description = $"No se pudo leer {path}",
                    Detail = ex.Message,
                    Severity = Severity.Info
                });
            }
        }
    }

    private static void ScanRunKeys(SignatureDb db, List<Finding> findings)
    {
        foreach (var fullPath in db.RegistryRunKeys)
        {
            var parts = fullPath.Split('\\', 2);
            if (parts.Length != 2) continue;

            RegistryKey root = parts[0] == "HKCU" ? Registry.CurrentUser : Registry.LocalMachine;

            try
            {
                using var key = root.OpenSubKey(parts[1]);
                if (key == null) continue;

                foreach (var valueName in key.GetValueNames())
                {
                    var value = key.GetValue(valueName)?.ToString() ?? "";
                    var lower = value.ToLowerInvariant();

                    if (db.ProcessKeywords.Any(k => lower.Contains(k)) ||
                        db.FileFolderKeywords.Any(k => lower.Contains(k)))
                    {
                        findings.Add(new Finding
                        {
                            Category = "Registro (Inicio automático)",
                            Description = "Entrada de arranque automático sospechosa",
                            Detail = $"{valueName} -> {value}",
                            Severity = Severity.High
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                findings.Add(new Finding
                {
                    Category = "Registro",
                    Description = $"No se pudo leer {fullPath}",
                    Detail = ex.Message,
                    Severity = Severity.Info
                });
            }
        }
    }
}

+++ RegistryScanner.cs (修改后)
using Microsoft.Win32;

namespace CheatScanner.Scanners;

public static class RegistryScanner
{
    public static List<Finding> Scan(SignatureDb db)
    {
        var findings = new List<Finding>();

        ScanUninstallList(db, findings);
        ScanRunKeys(db, findings);
        ScanAbnormalRegistryKeys(findings);
        ScanKnownCheatLocations(findings);

        return findings;
    }

    private static void ScanUninstallList(SignatureDb db, List<Finding> findings)
    {
        string[] uninstallPaths =
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (var path in uninstallPaths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);
                if (key == null) continue;

                foreach (var subName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subName);
                    var displayName = subKey?.GetValue("DisplayName") as string;
                    if (string.IsNullOrEmpty(displayName)) continue;

                    var lower = displayName.ToLowerInvariant();
                    if (db.RegistryUninstallKeywords.Any(k => lower.Contains(k)))
                    {
                        findings.Add(new Finding
                        {
                            Category = "Registro (Programas instalados)",
                            Description = "Programa instalado con nombre sospechoso",
                            Detail = displayName,
                            Severity = Severity.High,
                            EvidenceType = "Registro"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                findings.Add(new Finding
                {
                    Category = "Registro",
                    Description = $"No se pudo leer {path}",
                    Detail = ex.Message,
                    Severity = Severity.Info,
                    EvidenceType = "Registro"
                });
            }
        }
    }

    private static void ScanRunKeys(SignatureDb db, List<Finding> findings)
    {
        foreach (var fullPath in db.RegistryRunKeys)
        {
            var parts = fullPath.Split('\\', 2);
            if (parts.Length != 2) continue;

            RegistryKey root = parts[0] == "HKCU" ? Registry.CurrentUser : Registry.LocalMachine;

            try
            {
                using var key = root.OpenSubKey(parts[1]);
                if (key == null) continue;

                foreach (var valueName in key.GetValueNames())
                {
                    var value = key.GetValue(valueName)?.ToString() ?? "";
                    var lower = value.ToLowerInvariant();

                    if (db.ProcessKeywords.Any(k => lower.Contains(k)) ||
                        db.FileFolderKeywords.Any(k => lower.Contains(k)))
                    {
                        findings.Add(new Finding
                        {
                            Category = "Registro (Inicio automático)",
                            Description = "Entrada de arranque automático sospechosa",
                            Detail = $"{valueName} -> {value}",
                            Severity = Severity.High,
                            EvidenceType = "Registro"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                findings.Add(new Finding
                {
                    Category = "Registro",
                    Description = $"No se pudo leer {fullPath}",
                    Detail = ex.Message,
                    Severity = Severity.Info,
                    EvidenceType = "Registro"
                });
            }
        }
    }

    private static void ScanAbnormalRegistryKeys(List<Finding> findings)
    {
        // Claves comúnmente modificadas por cheats/modificaciones
        string[] suspiciousPaths =
        {
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon",
            @"SYSTEM\CurrentControlSet\Services",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options"
        };

        foreach (var path in suspiciousPaths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);
                if (key == null) continue;

                // Verificar valores inusuales
                foreach (var valueName in key.GetValueNames())
                {
                    var value = key.GetValue(valueName)?.ToString() ?? "";

                    // Detectar modificaciones sospechosas
                    if (valueName.Equals("Shell", StringComparison.OrdinalIgnoreCase) &&
                        !value.Equals("explorer.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        findings.Add(new Finding
                        {
                            Category = "Registro (Configuración del Sistema)",
                            Description = "Shell de Windows modificado (posible persistencia)",
                            Detail = $"{valueName} = {value}",
                            Severity = Severity.High,
                            EvidenceType = "Registro"
                        });
                    }

                    if (valueName.Contains("Debugger", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value))
                    {
                        findings.Add(new Finding
                        {
                            Category = "Registro (Image File Execution Options)",
                            Description = "Debugger adjunto a ejecutable (técnica común de inyección)",
                            Detail = $"{valueName} = {value}",
                            Severity = Severity.High,
                            EvidenceType = "Registro"
                        });
                    }
                }

                // Verificar subclaves sospechosas en IFEo
                if (path.Contains("Image File Execution Options"))
                {
                    var subKeys = key.GetSubKeyNames();
                    if (subKeys.Length > 0)
                    {
                        findings.Add(new Finding
                        {
                            Category = "Registro (IFEO)",
                            Description = "Opciones de ejecución de archivos detectadas",
                            Detail = $"Subclaves encontradas: {string.Join(", ", subKeys.Take(5))}",
                            Severity = Severity.Medium,
                            EvidenceType = "Registro"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                findings.Add(new Finding
                {
                    Category = "Registro",
                    Description = $"Error al escanear {path}",
                    Detail = ex.Message,
                    Severity = Severity.Info,
                    EvidenceType = "Registro"
                });
            }
        }
    }

    private static void ScanKnownCheatLocations(List<Finding> findings)
    {
        // Rutas de registro donde los cheats suelen guardarse
        string[] cheatPaths =
        {
            @"SOFTWARE\EasyAntiCheat",
            @"SOFTWARE\BattlEye",
            @"SOFTWARE\Valve\Steam\Apps",
            @"SOFTWARE\Classes\VirtualStore\MACHINE\SOFTWARE"
        };

        foreach (var path in cheatPaths)
        {
            try
            {
                RegistryKey? key = null;
                if (path.StartsWith(@"SOFTWARE\Classes"))
                    key = Registry.CurrentUser.OpenSubKey(path);
                else
                    key = Registry.LocalMachine.OpenSubKey(path);

                if (key != null)
                {
                    // Solo reportamos que existen estas claves como informativo
                    findings.Add(new Finding
                    {
                        Category = "Registro (Anti-Cheat / Juegos)",
                        Description = $"Clave de anti-cheat o juego detectada",
                        Detail = path,
                        Severity = Severity.Info,
                        EvidenceType = "Registro"
                    });
                }
            }
            catch (Exception ex)
            {
                // Ignorar errores silenciosamente para estas rutas
            }
        }
    }
}
