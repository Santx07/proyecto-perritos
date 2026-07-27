using CheatScanner;
using CheatScanner.Scanners;
using CheatScanner.Report;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Escáner de rastros (uso personal) ===\n");

var signaturesPath = Path.Combine(AppContext.BaseDirectory, "signatures.json");
SignatureDb db;

try
{
    db = SignatureDb.Load(signaturesPath);
}
catch (Exception ex)
{
    Console.WriteLine($"Error cargando firmas: {ex.Message}");
    return;
}

var allFindings = new List<Finding>();

Console.WriteLine("[1/4] Escaneando procesos en ejecución...");
allFindings.AddRange(ProcessScanner.Scan(db));

Console.WriteLine("[2/4] Escaneando archivos y carpetas...");
allFindings.AddRange(FileScanner.Scan(db));

Console.WriteLine("[3/4] Escaneando registro de Windows...");
allFindings.AddRange(RegistryScanner.Scan(db));

Console.WriteLine("[4/4] Escaneando servicios...");
allFindings.AddRange(ServiceScanner.Scan(db));

Console.WriteLine($"\nEscaneo completado. Hallazgos: {allFindings.Count}");

var html = ReportGenerator.Generate(allFindings);
var outputPath = Path.Combine(AppContext.BaseDirectory, $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.html");
File.WriteAllText(outputPath, html);

Console.WriteLine($"Reporte guardado en: {outputPath}");
Console.WriteLine("\nPresiona una tecla para salir...");
Console.ReadKey();
