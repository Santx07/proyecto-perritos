# Escáner de rastros (uso personal)

Herramienta en C# (.NET 8) que escanea tu propia PC en busca de indicios de
software de trampas/bypass: procesos en ejecución, archivos y carpetas,
entradas del registro de Windows y servicios. Genera un reporte HTML.

⚠️ Está pensada para uso personal (auditar tu propio equipo), usa coincidencias
de nombre/patrón (no firmas binarias reales), por lo que **puede haber falsos
positivos y falsos negativos**. No es un veredicto definitivo de nada.

## Requisitos

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado

## Cómo compilar y correr

1. Copia toda la carpeta `CheatScanner` a tu PC Windows.
2. Abre una terminal (PowerShell o CMD) dentro de la carpeta.
3. Ejecuta:

   ```
   dotnet restore
   dotnet run
   ```

   O para generar un .exe standalone:

   ```
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

   El ejecutable quedará en `bin\Release\net8.0\win-x64\publish\CheatScanner.exe`.

4. El programa imprimirá el progreso en consola y al final generará un
   archivo `reporte_YYYYMMDD_HHMMSS.html` en la misma carpeta del ejecutable.
   Ábrelo con tu navegador.

## Personalizar las firmas (`signatures.json`)

Este archivo controla **todo** lo que el escáner busca. Puedes editarlo sin
recompilar:

- `processKeywords` / `processNamesExact`: nombres de procesos.
- `fileFolderKeywords` / `suspiciousExtensions` / `scanDirectories`: qué
  carpetas revisa y qué nombres/extensiones marca.
- `registryUninstallKeywords` / `registryRunKeys`: programas instalados y
  entradas de arranque automático.
- `serviceKeywords`: servicios de Windows.

La lista incluida es solo un punto de partida genérico — te recomiendo
ampliarla con lo que tú sepas que es relevante para tu caso.

## Notas sobre permisos

Algunas partes del registro (`HKLM`) y ciertos servicios pueden requerir
ejecutar la terminal **como Administrador** para leerse completamente. Si ves
mensajes de error de "acceso denegado" en el reporte (categoría "Info"), es
por eso.

## Ideas para extender esto más adelante

- Verificar firma digital de drivers cargados (`.sys` en `System32\drivers`)
  usando `X509Certificate.CreateFromSignedFile`, y marcar los no firmados.
- Comparar hashes SHA-256 de archivos contra una lista propia conocida, en
  vez de solo nombres.
- Exportar el reporte también en JSON para procesarlo con otra herramienta.
- Programarlo como tarea programada de Windows para correr periódicamente.
