using System.ServiceProcess;

namespace CheatScanner.Scanners;

public static class ServiceScanner
{
    public static List<Finding> Scan(SignatureDb db)
    {
        var findings = new List<Finding>();

        ServiceController[] services;
        try
        {
            services = ServiceController.GetServices();
        }
        catch (Exception ex)
        {
            findings.Add(new Finding
            {
                Category = "Servicios",
                Description = "No se pudo enumerar servicios",
                Detail = ex.Message,
                Severity = Severity.Info
            });
            return findings;
        }

        foreach (var svc in services)
        {
            var lower = (svc.ServiceName + " " + svc.DisplayName).ToLowerInvariant();

            if (db.ServiceKeywords.Any(k => lower.Contains(k)))
            {
                findings.Add(new Finding
                {
                    Category = "Servicios",
                    Description = "Servicio con nombre sospechoso",
                    Detail = $"{svc.ServiceName} ({svc.DisplayName}) - Estado: {svc.Status}",
                    Severity = Severity.Medium
                });
            }
        }

        return findings;
    }
}
