--- ReportGenerator.cs (原始)
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

+++ ReportGenerator.cs (修改后)
using System.Text;

namespace CheatScanner.Report;

public static class ReportGenerator
{
    public static string Generate(List<Finding> findings, string? steamInfo, string? fiveMInfo, string? discordInfo, string? windowsLicense)
    {
        var sb = new StringBuilder();
        var now = DateTime.Now;

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='es'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.AppendLine("<title>Reporte de Escaneo - CheatScanner</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(":root { --bg-primary: #0f172a; --bg-secondary: #1e293b; --bg-card: #334155; --text-primary: #f1f5f9; --text-secondary: #cbd5e1; --danger: #ef4444; --warning: #f59e0b; --success: #10b981; --info: #3b82f6; --border: #475569; }");
        sb.AppendLine("* { box-sizing: border-box; margin: 0; padding: 0; }");
        sb.AppendLine("body { font-family: 'Segoe UI', system-ui, -apple-system, sans-serif; background: var(--bg-primary); color: var(--text-primary); padding: 24px; line-height: 1.6; }");
        sb.AppendLine(".container { max-width: 1400px; margin: 0 auto; }");
        sb.AppendLine("header { background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%); padding: 32px; border-radius: 12px; margin-bottom: 24px; box-shadow: 0 4px 6px rgba(0,0,0,0.3); }");
        sb.AppendLine("h1 { font-size: 2rem; margin-bottom: 8px; color: white; }");
        sb.AppendLine(".meta { color: rgba(255,255,255,0.8); font-size: 0.9rem; }");

        // Licencias Section
        sb.AppendLine(".licenses { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 16px; margin-bottom: 24px; }");
        sb.AppendLine(".license-card { background: var(--bg-secondary); padding: 20px; border-radius: 8px; border: 1px solid var(--border); }");
        sb.AppendLine(".license-card h3 { color: var(--info); margin-bottom: 12px; font-size: 1.1rem; display: flex; align-items: center; gap: 8px; }");
        sb.AppendLine(".license-card p { color: var(--text-secondary); font-size: 0.95rem; word-break: break-all; }");

        // Summary Cards
        sb.AppendLine(".summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; margin-bottom: 24px; }");
        sb.AppendLine(".stat-card { padding: 20px; border-radius: 8px; text-align: center; }");
        sb.AppendLine(".stat-card.high { background: linear-gradient(135deg, #7f1d1d 0%, #ef4444 100%); }");
        sb.AppendLine(".stat-card.medium { background: linear-gradient(135deg, #78350f 0%, #f59e0b 100%); }");
        sb.AppendLine(".stat-card.low { background: linear-gradient(135deg, #065f46 0%, #10b981 100%); }");
        sb.AppendLine(".stat-card.info { background: linear-gradient(135deg, #1e3a5f 0%, #3b82f6 100%); }");
        sb.AppendLine(".stat-number { font-size: 2.5rem; font-weight: bold; display: block; }");
        sb.AppendLine(".stat-label { font-size: 0.9rem; opacity: 0.9; }");

        // Findings by Risk Level
        sb.AppendLine(".risk-section { margin-bottom: 32px; }");
        sb.AppendLine(".risk-header { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; padding: 12px 16px; border-radius: 8px; }");
        sb.AppendLine(".risk-header.high { background: rgba(239, 68, 68, 0.15); border-left: 4px solid var(--danger); }");
        sb.AppendLine(".risk-header.medium { background: rgba(245, 158, 11, 0.15); border-left: 4px solid var(--warning); }");
        sb.AppendLine(".risk-header.low { background: rgba(16, 185, 129, 0.15); border-left: 4px solid var(--success); }");
        sb.AppendLine(".risk-header h2 { font-size: 1.4rem; }");
        sb.AppendLine(".badge { padding: 4px 12px; border-radius: 20px; font-size: 0.85rem; font-weight: 600; }");
        sb.AppendLine(".badge.high { background: var(--danger); color: white; }");
        sb.AppendLine(".badge.medium { background: var(--warning); color: black; }");
        sb.AppendLine(".badge.low { background: var(--success); color: white; }");
        sb.AppendLine(".badge.info { background: var(--info); color: white; }");

        // Tables
        sb.AppendLine("table { width: 100%; border-collapse: collapse; background: var(--bg-secondary); border-radius: 8px; overflow: hidden; }");
        sb.AppendLine("thead { background: var(--bg-card); }");
        sb.AppendLine("th, td { padding: 14px 16px; text-align: left; border-bottom: 1px solid var(--border); }");
        sb.AppendLine("th { font-weight: 600; color: var(--text-secondary); text-transform: uppercase; font-size: 0.8rem; letter-spacing: 0.05em; }");
        sb.AppendLine("tr:last-child td { border-bottom: none; }");
        sb.AppendLine("tr:hover { background: var(--bg-card); }");
        sb.AppendLine(".detail-cell { font-family: 'Consolas', 'Monaco', monospace; font-size: 0.85rem; word-break: break-all; color: var(--text-secondary); }");
        sb.AppendLine(".evidence-type { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 0.75rem; background: var(--bg-card); color: var(--info); }");

        // Empty state
        sb.AppendLine(".empty-state { text-align: center; padding: 60px 20px; color: var(--text-secondary); }");
        sb.AppendLine(".empty-state svg { width: 64px; height: 64px; margin-bottom: 16px; opacity: 0.5; }");

        // Footer
        sb.AppendLine("footer { margin-top: 40px; padding-top: 24px; border-top: 1px solid var(--border); color: var(--text-secondary); font-size: 0.85rem; text-align: center; }");
        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<div class='container'>");

        // Header
        sb.AppendLine("<header>");
        sb.AppendLine("<h1>🔍 Reporte de Escaneo Local</h1>");
        sb.AppendLine($"<p class='meta'>Generado: {now:yyyy-MM-dd HH:mm:ss} | Equipo: {Environment.MachineName} | Usuario: {Environment.UserName}</p>");
        sb.AppendLine("</header>");

        // Licenses Section
        sb.AppendLine("<h2 style='margin-bottom: 16px; color: var(--info);'>📋 Información de Software y Licencias</h2>");
        sb.AppendLine("<div class='licenses'>");

        sb.AppendLine("<div class='license-card'>");
        sb.AppendLine("<h3>🎮 Steam</h3>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(steamInfo ?? "No disponible")}</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class='license-card'>");
        sb.AppendLine("<h3>🚗 FiveM</h3>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(fiveMInfo ?? "No disponible")}</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class='license-card'>");
        sb.AppendLine("<h3>💬 Discord</h3>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(discordInfo ?? "No disponible")}</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class='license-card'>");
        sb.AppendLine("<h3>🪟 Windows</h3>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(windowsLicense ?? "No disponible")}</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div>");

        // Summary
        var high = findings.Count(f => f.Severity == Severity.High);
        var med = findings.Count(f => f.Severity == Severity.Medium);
        var low = findings.Count(f => f.Severity == Severity.Low);
        var info = findings.Count(f => f.Severity == Severity.Info);

        sb.AppendLine("<h2 style='margin-bottom: 16px;'>📊 Resumen de Hallazgos</h2>");
        sb.AppendLine("<div class='summary'>");

        sb.AppendLine($"<div class='stat-card high'><span class='stat-number'>{high}</span><span class='stat-label'>Riesgo Alto</span></div>");
        sb.AppendLine($"<div class='stat-card medium'><span class='stat-number'>{med}</span><span class='stat-label'>Riesgo Medio</span></div>");
        sb.AppendLine($"<div class='stat-card low'><span class='stat-number'>{low}</span><span class='stat-label'>Riesgo Bajo</span></div>");
        sb.AppendLine($"<div class='stat-card info'><span class='stat-number'>{info}</span><span class='stat-label'>Informativos</span></div>");

        sb.AppendLine("</div>");

        if (findings.Count == 0)
        {
            sb.AppendLine("<div class='empty-state'>");
            sb.AppendLine("<svg fill='currentColor' viewBox='0 0 20 20'><path d='M10 2a6 6 0 00-6 6v3.586l-.707.707A1 1 0 004 14h12a1 1 0 00.707-1.707L16 11.586V8a6 6 0 00-6-6zM10 18a3 3 0 01-3-3h6a3 3 0 01-3 3z'/></svg>");
            sb.AppendLine("<h3>No se encontraron amenazas</h3>");
            sb.AppendLine("<p>No se detectaron coincidencias con las firmas configuradas en este escaneo.</p>");
            sb.AppendLine("</div>");
        }
        else
        {
            // High Risk Section
            var highFindings = findings.Where(f => f.Severity == Severity.High).ToList();
            if (highFindings.Any())
            {
                sb.AppendLine("<div class='risk-section'>");
                sb.AppendLine("<div class='risk-header high'>");
                sb.AppendLine("<h2>⚠️ Riesgo Alto</h2>");
                sb.AppendLine($"<span class='badge high'>{highFindings.Count} hallazgos</span>");
                sb.AppendLine("</div>");
                sb.AppendLine("<table><thead><tr><th>Categoría</th><th>Descripción</th><th>Detalle</th><th>Evidencia</th></tr></thead><tbody>");
                foreach (var f in highFindings)
                {
                    sb.AppendLine($"<tr><td>{System.Net.WebUtility.HtmlEncode(f.Category)}</td>" +
                        $"<td>{System.Net.WebUtility.HtmlEncode(f.Description)}</td>" +
                        $"<td class='detail-cell'>{System.Net.WebUtility.HtmlEncode(f.Detail)}</td>" +
                        $"<td><span class='evidence-type'>{System.Net.WebUtility.HtmlEncode(f.EvidenceType)}</span></td></tr>");
                }
                sb.AppendLine("</tbody></table></div>");
            }

            // Medium Risk Section
            var medFindings = findings.Where(f => f.Severity == Severity.Medium).ToList();
            if (medFindings.Any())
            {
                sb.AppendLine("<div class='risk-section'>");
                sb.AppendLine("<div class='risk-header medium'>");
                sb.AppendLine("<h2>⚡ Riesgo Medio</h2>");
                sb.AppendLine($"<span class='badge medium'>{medFindings.Count} hallazgos</span>");
                sb.AppendLine("</div>");
                sb.AppendLine("<table><thead><tr><th>Categoría</th><th>Descripción</th><th>Detalle</th><th>Evidencia</th></tr></thead><tbody>");
                foreach (var f in medFindings.OrderByDescending(f => f.Category))
                {
                    sb.AppendLine($"<tr><td>{System.Net.WebUtility.HtmlEncode(f.Category)}</td>" +
                        $"<td>{System.Net.WebUtility.HtmlEncode(f.Description)}</td>" +
                        $"<td class='detail-cell'>{System.Net.WebUtility.HtmlEncode(f.Detail)}</td>" +
                        $"<td><span class='evidence-type'>{System.Net.WebUtility.HtmlEncode(f.EvidenceType)}</span></td></tr>");
                }
                sb.AppendLine("</tbody></table></div>");
            }

            // Low Risk & Info Section
            var lowFindings = findings.Where(f => f.Severity == Severity.Low || f.Severity == Severity.Info).ToList();
            if (lowFindings.Any())
            {
                sb.AppendLine("<div class='risk-section'>");
                sb.AppendLine("<div class='risk-header low'>");
                sb.AppendLine("<h2>ℹ️ Informativos / Riesgo Bajo</h2>");
                sb.AppendLine($"<span class='badge low'>{lowFindings.Count} hallazgos</span>");
                sb.AppendLine("</div>");
                sb.AppendLine("<table><thead><tr><th>Categoría</th><th>Descripción</th><th>Detalle</th><th>Evidencia</th></tr></thead><tbody>");
                foreach (var f in lowFindings.OrderByDescending(f => f.Category))
                {
                    sb.AppendLine($"<tr><td>{System.Net.WebUtility.HtmlEncode(f.Category)}</td>" +
                        $"<td>{System.Net.WebUtility.HtmlEncode(f.Description)}</td>" +
                        $"<td class='detail-cell'>{System.Net.WebUtility.HtmlEncode(f.Detail)}</td>" +
                        $"<td><span class='evidence-type'>{System.Net.WebUtility.HtmlEncode(f.EvidenceType)}</span></td></tr>");
                }
                sb.AppendLine("</tbody></table></div>");
            }
        }

        sb.AppendLine("<footer>");
        sb.AppendLine("<p><strong>Nota importante:</strong> Este reporte se basa en coincidencias de nombres, patrones, hashes y análisis de registro. " +
                       "Puede haber falsos positivos y falsos negativos. Los hallazgos de \"Riesgo Alto\" requieren verificación manual. " +
                       "Esta herramienta es para uso personal y no constituye un veredicto definitivo sobre la presencia de software malicioso.</p>");
        sb.AppendLine($"<p style='margin-top: 12px;'>CheatScanner v1.0 | Escaneo realizado en {Environment.MachineName}</p>");
        sb.AppendLine("</footer>");

        sb.AppendLine("</div></body></html>");

        return sb.ToString();
    }
}
