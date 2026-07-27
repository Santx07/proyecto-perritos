using System.Text;

namespace CheatScanner.Report;

public static class ReportGenerator
{
    public static string Generate(List<Finding> findings)
    {
        var sb = new StringBuilder();
        var now = DateTime.Now;

        sb.AppendLine("<html><head><meta charset='utf-8'><title>Reporte de Escaneo</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;background:#111;color:#eee;padding:24px}");
        sb.AppendLine("h1{color:#4fc3f7} h2{color:#81d4fa;margin-top:32px}");
        sb.AppendLine("table{border-collapse:collapse;width:100%;margin-top:8px}");
        sb.AppendLine("td,th{border:1px solid #333;padding:8px;text-align:left;font-size:13px}");
        sb.AppendLine("th{background:#222}");
        sb.AppendLine(".High{color:#ff5252;font-weight:bold} .Medium{color:#ffb74d} .Low{color:#aed581} .Info{color:#90a4ae}");
        sb.AppendLine(".summary{background:#1b1b1b;padding:16px;border-radius:8px;margin-bottom:16px}");
        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<h1>Reporte de Escaneo Local</h1>");
        sb.AppendLine($"<p>Generado: {now:yyyy-MM-dd HH:mm:ss} | Equipo: {Environment.MachineName} | Usuario: {Environment.UserName}</p>");

        var high = findings.Count(f => f.Severity == Severity.High);
        var med = findings.Count(f => f.Severity == Severity.Medium);
        var low = findings.Count(f => f.Severity == Severity.Low);
        var info = findings.Count(f => f.Severity == Severity.Info);

        sb.AppendLine("<div class='summary'>");
        sb.AppendLine($"<b>Resumen:</b> {findings.Count} hallazgos totales &nbsp;|&nbsp; " +
                      $"<span class='High'>{high} altos</span> &nbsp;|&nbsp; " +
                      $"<span class='Medium'>{med} medios</span> &nbsp;|&nbsp; " +
                      $"<span class='Low'>{low} bajos</span> &nbsp;|&nbsp; " +
                      $"<span class='Info'>{info} informativos</span>");
        sb.AppendLine("</div>");

        if (findings.Count == 0)
        {
            sb.AppendLine("<p>No se encontraron coincidencias con las firmas configuradas.</p>");
        }
        else
        {
            foreach (var group in findings.GroupBy(f => f.Category))
            {
                sb.AppendLine($"<h2>{System.Net.WebUtility.HtmlEncode(group.Key)}</h2>");
                sb.AppendLine("<table><tr><th>Severidad</th><th>Descripción</th><th>Detalle</th></tr>");
                foreach (var f in group.OrderByDescending(f => f.Severity))
                {
                    sb.AppendLine("<tr>" +
                        $"<td class='{f.Severity}'>{f.Severity}</td>" +
                        $"<td>{System.Net.WebUtility.HtmlEncode(f.Description)}</td>" +
                        $"<td>{System.Net.WebUtility.HtmlEncode(f.Detail)}</td>" +
                        "</tr>");
                }
                sb.AppendLine("</table>");
            }
        }

        sb.AppendLine("<p style='margin-top:32px;color:#666;font-size:12px'>Nota: este reporte se basa en coincidencias de nombres y patrones. " +
                       "Puede haber falsos positivos y falsos negativos. No es un veredicto definitivo.</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}
