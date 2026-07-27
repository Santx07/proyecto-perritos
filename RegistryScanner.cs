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
