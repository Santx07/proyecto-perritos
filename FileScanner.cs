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
