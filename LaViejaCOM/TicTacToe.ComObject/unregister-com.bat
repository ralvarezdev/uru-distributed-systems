@echo off
echo ================================================
echo Desregistrando TicTacToe COM Object
echo ================================================

:: Verificar si se ejecuta como administrador
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Ejecutando como administrador...
) else (
    echo ADVERTENCIA: Se recomienda ejecutar como administrador
    echo Presiona cualquier tecla para continuar...
    pause >nul
)

:: Buscar regasm.exe
echo.
echo 1. Buscando regasm.exe...

set REGASM_PATH=""
set FRAMEWORK_PATH="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"
set FRAMEWORK_PATH_32="C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe"

if exist %FRAMEWORK_PATH% (
    set REGASM_PATH=%FRAMEWORK_PATH%
    echo Encontrado regasm.exe (64-bit): %FRAMEWORK_PATH%
) else if exist %FRAMEWORK_PATH_32% (
    set REGASM_PATH=%FRAMEWORK_PATH_32%
    echo Encontrado regasm.exe (32-bit): %FRAMEWORK_PATH_32%
) else (
    echo ERROR: No se pudo encontrar regasm.exe
    echo Verifica que .NET Framework 4.0+ esté instalado
    pause
    exit /b 1
)

:: Desregistrar el COM object
echo.
echo 2. Desregistrando COM object...
set DLL_PATH="bin\Release\net48\TicTacToe.ComObject.dll"

if exist %DLL_PATH% (
    echo Desregistrando: %DLL_PATH%
    %REGASM_PATH% %DLL_PATH% /unregister
    
    if %errorLevel% == 0 (
        echo.
        echo ================================================
        echo ¡COM Object desregistrado exitosamente!
        echo ================================================
    ) else (
        echo ERROR: Falló la desregistración del COM object
    )
) else (
    echo ADVERTENCIA: No se encontró el archivo DLL: %DLL_PATH%
    echo Intentando desregistrar de todos modos...
    %REGASM_PATH% "TicTacToe.ComObject.dll" /unregister
)

echo.
echo Presiona cualquier tecla para continuar...
pause >nul