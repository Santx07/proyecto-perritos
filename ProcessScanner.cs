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
