--- ReportGenerator.cs (原始)
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

+++ ReportGenerator.cs (修改后)
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
        sb.AppendLine("<title>Reporte de Análisis de Seguridad | CheatScanner Pro</title>");
        sb.AppendLine("<style>");

        // Variables CSS
        sb.AppendLine(":root {");
        sb.AppendLine("--bg-primary: #0a0e1a; --bg-secondary: #111827; --bg-card: #1f2937; --bg-elevated: #374151;");
        sb.AppendLine("--text-primary: #f9fafb; --text-secondary: #9ca3af; --text-muted: #6b7280;");
        sb.AppendLine("--danger: #dc2626; --danger-light: #fee2e2; --danger-bg: rgba(220, 38, 38, 0.1);");
        sb.AppendLine("--warning: #f59e0b; --warning-light: #fef3c7; --warning-bg: rgba(245, 158, 11, 0.1);");
        sb.AppendLine("--success: #10b981; --success-light: #d1fae5; --success-bg: rgba(16, 185, 129, 0.1);");
        sb.AppendLine("--info: #3b82f6; --info-light: #dbeafe; --info-bg: rgba(59, 130, 246, 0.1);");
        sb.AppendLine("--border: #374151; --border-light: #4b5563;");
        sb.AppendLine("--shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.3), 0 2px 4px -1px rgba(0, 0, 0, 0.15);");
        sb.AppendLine("--shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.4), 0 4px 6px -2px rgba(0, 0, 0, 0.2);");
        sb.AppendLine("}");

        // Reset y base
        sb.AppendLine("* { box-sizing: border-box; margin: 0; padding: 0; }");
        sb.AppendLine("html { scroll-behavior: smooth; }");
        sb.AppendLine("body { font-family: 'Inter', 'Segoe UI', system-ui, -apple-system, sans-serif; background: var(--bg-primary); color: var(--text-primary); line-height: 1.6; min-height: 100vh; }");
        sb.AppendLine(".container { max-width: 1600px; margin: 0 auto; padding: 0 24px; }");

        // Header con gradiente animado
        sb.AppendLine("header { background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 50%, #1d4ed8 100%); padding: 48px 32px; border-radius: 16px; margin: 24px 0 32px; box-shadow: var(--shadow-lg); position: relative; overflow: hidden; }");
        sb.AppendLine("header::before { content: ''; position: absolute; top: -50%; right: -50%; width: 200%; height: 200%; background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%); animation: pulse 8s ease-in-out infinite; }");
        sb.AppendLine("@keyframes pulse { 0%, 100% { transform: scale(1); opacity: 0.5; } 50% { transform: scale(1.1); opacity: 0.8; } }");
        sb.AppendLine(".header-content { position: relative; z-index: 1; }");
        sb.AppendLine("h1 { font-size: 2.25rem; font-weight: 800; margin-bottom: 12px; color: white; letter-spacing: -0.025em; display: flex; align-items: center; gap: 12px; }");
        sb.AppendLine(".logo-icon { width: 40px; height: 40px; background: rgba(255,255,255,0.2); border-radius: 10px; display: flex; align-items: center; justify-content: center; backdrop-filter: blur(10px); }");
        sb.AppendLine(".meta { color: rgba(255,255,255,0.85); font-size: 0.95rem; display: flex; flex-wrap: wrap; gap: 16px; margin-top: 16px; }");
        sb.AppendLine(".meta-item { display: flex; align-items: center; gap: 6px; background: rgba(255,255,255,0.1); padding: 6px 12px; border-radius: 6px; backdrop-filter: blur(5px); }");

        // Grid de licencias
        sb.AppendLine(".section-title { font-size: 1.5rem; font-weight: 700; margin: 32px 0 20px; color: var(--text-primary); display: flex; align-items: center; gap: 10px; }");
        sb.AppendLine(".section-title svg { width: 28px; height: 28px; color: var(--info); }");
        sb.AppendLine(".licenses { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; margin-bottom: 32px; }");
        sb.AppendLine(".license-card { background: linear-gradient(145deg, var(--bg-secondary), var(--bg-card)); padding: 24px; border-radius: 12px; border: 1px solid var(--border); transition: all 0.3s ease; position: relative; overflow: hidden; }");
        sb.AppendLine(".license-card:hover { transform: translateY(-2px); box-shadow: var(--shadow-lg); border-color: var(--info); }");
        sb.AppendLine(".license-card::before { content: ''; position: absolute; top: 0; left: 0; right: 0; height: 3px; background: linear-gradient(90deg, var(--info), #60a5fa); opacity: 0; transition: opacity 0.3s; }");
        sb.AppendLine(".license-card:hover::before { opacity: 1; }");
        sb.AppendLine(".license-header { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; }");
        sb.AppendLine(".license-icon { width: 42px; height: 42px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; background: var(--info-bg); }");
        sb.AppendLine(".license-card h3 { font-size: 1.1rem; font-weight: 600; color: var(--text-primary); }");
        sb.AppendLine(".license-card p { color: var(--text-secondary); font-size: 0.9rem; word-break: break-all; line-height: 1.7; }");
        sb.AppendLine(".license-status { display: inline-block; padding: 4px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 600; margin-top: 12px; background: var(--success-bg); color: var(--success); }");
        sb.AppendLine(".license-status.warning { background: var(--warning-bg); color: var(--warning); }");
        sb.AppendLine(".license-status.error { background: var(--danger-bg); color: var(--danger); }");

        // Tarjetas de resumen con gráficos
        sb.AppendLine(".summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 20px; margin-bottom: 40px; }");
        sb.AppendLine(".stat-card { padding: 28px 24px; border-radius: 14px; text-align: center; position: relative; overflow: hidden; transition: all 0.3s ease; }");
        sb.AppendLine(".stat-card:hover { transform: translateY(-4px); box-shadow: var(--shadow-lg); }");
        sb.AppendLine(".stat-card.high { background: linear-gradient(135deg, #991b1b 0%, #dc2626 100%); }");
        sb.AppendLine(".stat-card.medium { background: linear-gradient(135deg, #92400e 0%, #f59e0b 100%); }");
        sb.AppendLine(".stat-card.low { background: linear-gradient(135deg, #065f46 0%, #10b981 100%); }");
        sb.AppendLine(".stat-card.info { background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%); }");
        sb.AppendLine(".stat-icon { width: 50px; height: 50px; margin: 0 auto 12px; background: rgba(255,255,255,0.2); border-radius: 12px; display: flex; align-items: center; justify-content: center; backdrop-filter: blur(10px); }");
        sb.AppendLine(".stat-icon svg { width: 28px; height: 28px; }");
        sb.AppendLine(".stat-number { font-size: 3rem; font-weight: 800; display: block; line-height: 1; text-shadow: 0 2px 4px rgba(0,0,0,0.2); }");
        sb.AppendLine(".stat-label { font-size: 0.95rem; font-weight: 500; opacity: 0.95; margin-top: 8px; display: block; }");
        sb.AppendLine(".stat-total { font-size: 0.8rem; opacity: 0.8; margin-top: 4px; }");

        // Barra de progreso de riesgo
        sb.AppendLine(".risk-bar-container { background: var(--bg-card); border-radius: 12px; padding: 24px; margin-bottom: 40px; border: 1px solid var(--border); }");
        sb.AppendLine(".risk-bar-title { font-size: 1.1rem; font-weight: 600; margin-bottom: 16px; color: var(--text-primary); }");
        sb.AppendLine(".risk-bar { display: flex; height: 12px; border-radius: 6px; overflow: hidden; background: var(--bg-elevated); }");
        sb.AppendLine(".risk-segment { height: 100%; transition: width 0.5s ease; }");
        sb.AppendLine(".risk-segment.high { background: var(--danger); }");
        sb.AppendLine(".risk-segment.medium { background: var(--warning); }");
        sb.AppendLine(".risk-segment.low { background: var(--success); }");
        sb.AppendLine(".risk-segment.info { background: var(--info); }");
        sb.AppendLine(".risk-legend { display: flex; justify-content: space-between; margin-top: 12px; font-size: 0.85rem; color: var(--text-secondary); }");
        sb.AppendLine(".risk-legend-item { display: flex; align-items: center; gap: 6px; }");
        sb.AppendLine(".risk-legend-dot { width: 10px; height: 10px; border-radius: 50%; }");

        // Secciones de riesgo
        sb.AppendLine(".risk-section { margin-bottom: 40px; animation: fadeIn 0.5s ease; }");
        sb.AppendLine("@keyframes fadeIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }");
        sb.AppendLine(".risk-header { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 20px; padding: 20px 24px; border-radius: 12px; border: 1px solid; }");
        sb.AppendLine(".risk-header.high { background: var(--danger-bg); border-color: var(--danger); }");
        sb.AppendLine(".risk-header.medium { background: var(--warning-bg); border-color: var(--warning); }");
        sb.AppendLine(".risk-header.low { background: var(--success-bg); border-color: var(--success); }");
        sb.AppendLine(".risk-title { display: flex; align-items: center; gap: 12px; font-size: 1.3rem; font-weight: 700; }");
        sb.AppendLine(".risk-icon { width: 32px; height: 32px; }");
        sb.AppendLine(".badge { padding: 6px 16px; border-radius: 20px; font-size: 0.9rem; font-weight: 700; display: inline-flex; align-items: center; gap: 6px; }");
        sb.AppendLine(".badge.high { background: var(--danger); color: white; }");
        sb.AppendLine(".badge.medium { background: var(--warning); color: #111827; }");
        sb.AppendLine(".badge.low { background: var(--success); color: white; }");
        sb.AppendLine(".badge.info { background: var(--info); color: white; }");

        // Tablas mejoradas
        sb.AppendLine(".table-container { background: var(--bg-secondary); border-radius: 12px; overflow: hidden; border: 1px solid var(--border); box-shadow: var(--shadow); }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
        sb.AppendLine("thead { background: linear-gradient(180deg, var(--bg-elevated), var(--bg-card)); }");
        sb.AppendLine("th { padding: 16px 20px; text-align: left; font-weight: 600; color: var(--text-secondary); text-transform: uppercase; font-size: 0.75rem; letter-spacing: 0.05em; border-bottom: 2px solid var(--border); }");
        sb.AppendLine("td { padding: 16px 20px; border-bottom: 1px solid var(--border); vertical-align: top; }");
        sb.AppendLine("tr:last-child td { border-bottom: none; }");
        sb.AppendLine("tr:hover { background: var(--bg-card); }");
        sb.AppendLine(".category-cell { font-weight: 600; color: var(--text-primary); }");
        sb.AppendLine(".description-cell { color: var(--text-secondary); }");
        sb.AppendLine(".detail-cell { font-family: 'JetBrains Mono', 'Consolas', 'Monaco', monospace; font-size: 0.85rem; word-break: break-all; color: var(--text-muted); background: var(--bg-primary); padding: 10px 12px; border-radius: 6px; border: 1px solid var(--border); }");
        sb.AppendLine(".evidence-badge { display: inline-flex; align-items: center; gap: 6px; padding: 6px 12px; border-radius: 6px; font-size: 0.8rem; font-weight: 600; background: var(--info-bg); color: var(--info); border: 1px solid rgba(59, 130, 246, 0.3); }");
        sb.AppendLine(".severity-indicator { width: 4px; height: 100%; position: absolute; left: 0; top: 0; border-radius: 4px 0 0 4px; }");
        sb.AppendLine(".severity-indicator.high { background: var(--danger); }");
        sb.AppendLine(".severity-indicator.medium { background: var(--warning); }");
        sb.AppendLine(".severity-indicator.low { background: var(--success); }");
        sb.AppendLine(".severity-indicator.info { background: var(--info); }");

        // Estado vacío
        sb.AppendLine(".empty-state { text-align: center; padding: 80px 24px; background: var(--bg-secondary); border-radius: 16px; border: 2px dashed var(--border); }");
        sb.AppendLine(".empty-icon { width: 80px; height: 80px; margin: 0 auto 24px; background: var(--success-bg); border-radius: 50%; display: flex; align-items: center; justify-content: center; }");
        sb.AppendLine(".empty-icon svg { width: 40px; height: 40px; color: var(--success); }");
        sb.AppendLine(".empty-state h3 { font-size: 1.5rem; font-weight: 700; margin-bottom: 12px; color: var(--text-primary); }");
        sb.AppendLine(".empty-state p { color: var(--text-secondary); max-width: 500px; margin: 0 auto; }");

        // Footer
        sb.AppendLine("footer { margin-top: 60px; padding: 32px; background: var(--bg-secondary); border-radius: 12px; border: 1px solid var(--border); text-align: center; }");
        sb.AppendLine(".footer-warning { background: var(--warning-bg); border: 1px solid var(--warning); border-radius: 8px; padding: 20px; margin-bottom: 20px; text-align: left; }");
        sb.AppendLine(".footer-warning strong { color: var(--warning); display: block; margin-bottom: 8px; font-size: 1rem; }");
        sb.AppendLine(".footer-warning p { color: var(--text-secondary); font-size: 0.9rem; line-height: 1.7; }");
        sb.AppendLine(".footer-meta { color: var(--text-muted); font-size: 0.85rem; }");
        sb.AppendLine(".footer-brand { margin-top: 16px; padding-top: 16px; border-top: 1px solid var(--border); color: var(--text-secondary); font-size: 0.85rem; display: flex; align-items: center; justify-content: center; gap: 8px; }");

        // Responsive
        sb.AppendLine("@media (max-width: 768px) {");
        sb.AppendLine("h1 { font-size: 1.75rem; }");
        sb.AppendLine(".licenses { grid-template-columns: 1fr; }");
        sb.AppendLine(".summary { grid-template-columns: repeat(2, 1fr); }");
        sb.AppendLine(".meta { flex-direction: column; gap: 8px; }");
        sb.AppendLine("table { font-size: 0.85rem; }");
        sb.AppendLine("th, td { padding: 12px; }");
        sb.AppendLine(".risk-header { flex-direction: column; align-items: flex-start; gap: 12px; }");
        sb.AppendLine("}");

        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<div class='container'>");

        // Header
        sb.AppendLine("<header>");
        sb.AppendLine("<div class='header-content'>");
        sb.AppendLine("<h1>");
        sb.AppendLine("<div class='logo-icon'><svg width='24' height='24' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z'/></svg></div>");
        sb.AppendLine("Reporte de Análisis de Seguridad");
        sb.AppendLine("</h1>");
        sb.AppendLine("<div class='meta'>");
        sb.AppendLine($"<span class='meta-item'><svg width='16' height='16' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z'/></svg>{now:dd/MM/yyyy HH:mm:ss}</span>");
        sb.AppendLine($"<span class='meta-item'><svg width='16' height='16' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z'/></svg>{System.Net.WebUtility.HtmlEncode(Environment.MachineName)}</span>");
        sb.AppendLine($"<span class='meta-item'><svg width='16' height='16' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z'/></svg>{System.Net.WebUtility.HtmlEncode(Environment.UserName)}</span>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</header>");

        // Licencias Section
        sb.AppendLine("<h2 class='section-title'><svg fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'/></svg>Información de Software y Licencias</h2>");
        sb.AppendLine("<div class='licenses'>");

        // Steam
        bool steamOk = !string.IsNullOrEmpty(steamInfo) && !steamInfo.Contains("No disponible") && !steamInfo.Contains("Error");
        sb.AppendLine("<div class='license-card'>");
        sb.AppendLine("<div class='license-header'>");
        sb.AppendLine("<div class='license-icon'>🎮</div>");
        sb.AppendLine("<h3>Steam</h3>");
        sb.AppendLine($"<span class='license-status {(steamOk ? "" : "error")}'>{(steamOk ? "Detectado" : "No disponible")}</span>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(steamInfo ?? "No disponible")}</p>");
        sb.AppendLine("</div>");

        // FiveM
        bool fiveMOk = !string.IsNullOrEmpty(fiveMInfo) && !fiveMInfo.Contains("No disponible") && !fiveMInfo.Contains("Error");
        sb.AppendLine("<div class='license-card'>");
        sb.AppendLine("<div class='license-header'>");
        sb.AppendLine("<div class='license-icon'>🚗</div>");
        sb.AppendLine("<h3>FiveM</h3>");
        sb.AppendLine($"<span class='license-status {(fiveMOk ? "" : "error")}'>{(fiveMOk ? "Detectado" : "No disponible")}</span>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(fiveMInfo ?? "No disponible")}</p>");
        sb.AppendLine("</div>");

        // Discord
        bool discordOk = !string.IsNullOrEmpty(discordInfo) && !discordInfo.Contains("No disponible") && !discordInfo.Contains("Error");
        sb.AppendLine("<div class='license-card'>");
        sb.AppendLine("<div class='license-header'>");
        sb.AppendLine("<div class='license-icon'>💬</div>");
        sb.AppendLine("<h3>Discord</h3>");
        sb.AppendLine($"<span class='license-status {(discordOk ? "" : "error")}'>{(discordOk ? "Detectado" : "No disponible")}</span>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(discordInfo ?? "No disponible")}</p>");
        sb.AppendLine("</div>");

        // Windows
        bool windowsOk = !string.IsNullOrEmpty(windowsLicense) && !windowsLicense.Contains("No disponible") && !windowsLicense.Contains("Error");
        sb.AppendLine("<div class='license-card'>");
        sb.AppendLine("<div class='license-header'>");
        sb.AppendLine("<div class='license-icon'>🪟</div>");
        sb.AppendLine("<h3>Licencia Windows</h3>");
        sb.AppendLine($"<span class='license-status {(windowsOk ? "" : "warning")}'>{(windowsOk ? "Verificada" : "No verificada")}</span>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(windowsLicense ?? "No disponible")}</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div>");

        // Cálculos de resumen
        var high = findings.Count(f => f.Severity == Severity.High);
        var med = findings.Count(f => f.Severity == Severity.Medium);
        var low = findings.Count(f => f.Severity == Severity.Low);
        var info = findings.Count(f => f.Severity == Severity.Info);
        var total = findings.Count;

        // Barra de riesgo
        if (total > 0)
        {
            var highPct = (high * 100.0) / total;
            var medPct = (med * 100.0) / total;
            var lowPct = (low * 100.0) / total;
            var infoPct = (info * 100.0) / total;

            sb.AppendLine("<div class='risk-bar-container'>");
            sb.AppendLine("<div class='risk-bar-title'>Distribución de Riesgos</div>");
            sb.AppendLine("<div class='risk-bar'>");
            sb.AppendLine($"<div class='risk-segment high' style='width: {highPct:F1}%'></div>");
            sb.AppendLine($"<div class='risk-segment medium' style='width: {medPct:F1}%'></div>");
            sb.AppendLine($"<div class='risk-segment low' style='width: {lowPct:F1}%'></div>");
            sb.AppendLine($"<div class='risk-segment info' style='width: {infoPct:F1}%'></div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='risk-legend'>");
            sb.AppendLine($"<span class='risk-legend-item'><span class='risk-legend-dot' style='background: var(--danger)'></span>Alto ({high})</span>");
            sb.AppendLine($"<span class='risk-legend-item'><span class='risk-legend-dot' style='background: var(--warning)'></span>Medio ({med})</span>");
            sb.AppendLine($"<span class='risk-legend-item'><span class='risk-legend-dot' style='background: var(--success)'></span>Bajo ({low})</span>");
            sb.AppendLine($"<span class='risk-legend-item'><span class='risk-legend-dot' style='background: var(--info)'></span>Informativo ({info})</span>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
        }

        // Summary Cards
        sb.AppendLine("<h2 class='section-title'><svg fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z'/></svg>Resumen de Hallazgos</h2>");
        sb.AppendLine("<div class='summary'>");

        sb.AppendLine($"<div class='stat-card high'>");
        sb.AppendLine("<div class='stat-icon'><svg fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z'/></svg></div>");
        sb.AppendLine($"<span class='stat-number'>{high}</span>");
        sb.AppendLine($"<span class='stat-label'>Riesgo Alto</span>");
        sb.AppendLine($"<span class='stat-total'>{(total > 0 ? (high * 100.0 / total).ToString("F1") : "0")}% del total</span>");
        sb.AppendLine("</div>");

        sb.AppendLine($"<div class='stat-card medium'>");
        sb.AppendLine("<div class='stat-icon'><svg fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg></div>");
        sb.AppendLine($"<span class='stat-number'>{med}</span>");
        sb.AppendLine($"<span class='stat-label'>Riesgo Medio</span>");
        sb.AppendLine($"<span class='stat-total'>{(total > 0 ? (med * 100.0 / total).ToString("F1") : "0")}% del total</span>");
        sb.AppendLine("</div>");

        sb.AppendLine($"<div class='stat-card low'>");
        sb.AppendLine("<div class='stat-icon'><svg fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg></div>");
        sb.AppendLine($"<span class='stat-number'>{low}</span>");
        sb.AppendLine($"<span class='stat-label'>Riesgo Bajo</span>");
        sb.AppendLine($"<span class='stat-total'>{(total > 0 ? (low * 100.0 / total).ToString("F1") : "0")}% del total</span>");
        sb.AppendLine("</div>");

        sb.AppendLine($"<div class='stat-card info'>");
        sb.AppendLine("<div class='stat-icon'><svg fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg></div>");
        sb.AppendLine($"<span class='stat-number'>{info}</span>");
        sb.AppendLine($"<span class='stat-label'>Informativos</span>");
        sb.AppendLine($"<span class='stat-total'>{(total > 0 ? (info * 100.0 / total).ToString("F1") : "0")}% del total</span>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div>");

        if (findings.Count == 0)
        {
            sb.AppendLine("<div class='empty-state'>");
            sb.AppendLine("<div class='empty-icon'><svg fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z'/></svg></div>");
            sb.AppendLine("<h3>✅ No se encontraron amenazas</h3>");
            sb.AppendLine("<p>Excelente. No se detectaron coincidencias con las firmas configuradas en este escaneo. Esto indica que tu sistema está limpio según las bases de datos actuales.</p>");
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
                sb.AppendLine("<div class='risk-title'>");
                sb.AppendLine("<svg class='risk-icon' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z'/></svg>");
                sb.AppendLine("Riesgo Alto - Requiere Atención Inmediata");
                sb.AppendLine("</div>");
                sb.AppendLine($"<span class='badge high'><svg width='16' height='16' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'/></svg>{highFindings.Count} hallazgos</span>");
                sb.AppendLine("</div>");
                sb.AppendLine("<div class='table-container'><table><thead><tr><th>Categoría</th><th>Descripción</th><th>Detalle</th><th>Tipo de Evidencia</th></tr></thead><tbody>");
                foreach (var f in highFindings)
                {
                    sb.AppendLine($"<tr style='position: relative;'><div class='severity-indicator high'></div>" +
                        $"<td class='category-cell'>{System.Net.WebUtility.HtmlEncode(f.Category)}</td>" +
                        $"<td class='description-cell'>{System.Net.WebUtility.HtmlEncode(f.Description)}</td>" +
                        $"<td><div class='detail-cell'>{System.Net.WebUtility.HtmlEncode(f.Detail)}</div></td>" +
                        $"<td><span class='evidence-badge'><svg width='14' height='14' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'/></svg>{System.Net.WebUtility.HtmlEncode(f.EvidenceType)}</span></td></tr>");
                }
                sb.AppendLine("</tbody></table></div></div>");
            }

            // Medium Risk Section
            var medFindings = findings.Where(f => f.Severity == Severity.Medium).ToList();
            if (medFindings.Any())
            {
                sb.AppendLine("<div class='risk-section'>");
                sb.AppendLine("<div class='risk-header medium'>");
                sb.AppendLine("<div class='risk-title'>");
                sb.AppendLine("<svg class='risk-icon' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg>");
                sb.AppendLine("Riesgo Medio - Verificación Recomendada");
                sb.AppendLine("</div>");
                sb.AppendLine($"<span class='badge medium'><svg width='16' height='16' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg>{medFindings.Count} hallazgos</span>");
                sb.AppendLine("</div>");
                sb.AppendLine("<div class='table-container'><table><thead><tr><th>Categoría</th><th>Descripción</th><th>Detalle</th><th>Tipo de Evidencia</th></tr></thead><tbody>");
                foreach (var f in medFindings.OrderByDescending(f => f.Category))
                {
                    sb.AppendLine($"<tr style='position: relative;'><div class='severity-indicator medium'></div>" +
                        $"<td class='category-cell'>{System.Net.WebUtility.HtmlEncode(f.Category)}</td>" +
                        $"<td class='description-cell'>{System.Net.WebUtility.HtmlEncode(f.Description)}</td>" +
                        $"<td><div class='detail-cell'>{System.Net.WebUtility.HtmlEncode(f.Detail)}</div></td>" +
                        $"<td><span class='evidence-badge'><svg width='14' height='14' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'/></svg>{System.Net.WebUtility.HtmlEncode(f.EvidenceType)}</span></td></tr>");
                }
                sb.AppendLine("</tbody></table></div></div>");
            }

            // Low Risk & Info Section
            var lowFindings = findings.Where(f => f.Severity == Severity.Low || f.Severity == Severity.Info).ToList();
            if (lowFindings.Any())
            {
                sb.AppendLine("<div class='risk-section'>");
                sb.AppendLine("<div class='risk-header low'>");
                sb.AppendLine("<div class='risk-title'>");
                sb.AppendLine("<svg class='risk-icon' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg>");
                sb.AppendLine("Informativos / Riesgo Bajo");
                sb.AppendLine("</div>");
                sb.AppendLine($"<span class='badge low'><svg width='16' height='16' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 13l4 4L19 7'/></svg>{lowFindings.Count} hallazgos</span>");
                sb.AppendLine("</div>");
                sb.AppendLine("<div class='table-container'><table><thead><tr><th>Categoría</th><th>Descripción</th><th>Detalle</th><th>Tipo de Evidencia</th></tr></thead><tbody>");
                foreach (var f in lowFindings.OrderByDescending(f => f.Category))
                {
                    sb.AppendLine($"<tr style='position: relative;'><div class='severity-indicator {(f.Severity == Severity.Info ? "info" : "low")}'></div>" +
                        $"<td class='category-cell'>{System.Net.WebUtility.HtmlEncode(f.Category)}</td>" +
                        $"<td class='description-cell'>{System.Net.WebUtility.HtmlEncode(f.Description)}</td>" +
                        $"<td><div class='detail-cell'>{System.Net.WebUtility.HtmlEncode(f.Detail)}</div></td>" +
                        $"<td><span class='evidence-badge'><svg width='14' height='14' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'/></svg>{System.Net.WebUtility.HtmlEncode(f.EvidenceType)}</span></td></tr>");
                }
                sb.AppendLine("</tbody></table></div></div>");
            }
        }

        sb.AppendLine("<footer>");
        sb.AppendLine("<div class='footer-warning'>");
        sb.AppendLine("<strong>⚠️ Nota Importante sobre este Reporte</strong>");
        sb.AppendLine("<p>Este análisis se basa en coincidencias de nombres de procesos, patrones de comportamiento, hashes conocidos y análisis de registro de Windows. Es posible que existan <strong>falsos positivos</strong> (elementos marcados como sospechosos que son legítimos) y <strong>falsos negativos</strong> (amenazas que no fueron detectadas). Los hallazgos clasificados como \"Riesgo Alto\" deben ser verificados manualmente por un técnico especializado antes de tomar cualquier acción. Esta herramienta está diseñada para uso personal e informativo y <strong>no constituye un veredicto definitivo</strong> sobre la presencia o ausencia de software malicioso en el sistema.</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div class='footer-meta'>");
        sb.AppendLine($"<p>ID del Reporte: {Guid.NewGuid():N}</p>");
        sb.AppendLine($"<p>Motor de escaneo: CheatScanner Pro v2.0 | Timestamp: {now:O}</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div class='footer-brand'>");
        sb.AppendLine("<svg width='18' height='18' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z'/></svg>");
        sb.AppendLine($"CheatScanner Pro - Análisis de Seguridad Local en {Environment.MachineName}");
        sb.AppendLine("</div>");
        sb.AppendLine("</footer>");

        sb.AppendLine("</div></body></html>");

        return sb.ToString();
    }
}
