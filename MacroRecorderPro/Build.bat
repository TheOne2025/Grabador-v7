@echo off
echo 🔨 Compilando Macro Recorder Pro...
dotnet restore
dotnet build --configuration Release
if %ERRORLEVEL% == 0 (
    echo ✅ Compilación exitosa
    echo 📁 Archivos en: bin\Release\net6.0-windows\
) else (
    echo ❌ Error en la compilación
)
pause
