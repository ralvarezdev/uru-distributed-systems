@echo off
echo ================================================
echo Registrando TicTacToe COM Object
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

:: Construir el proyecto primero
echo.
echo 1. Construyendo el proyecto...
dotnet build --configuration Release

if %errorLevel% neq 0 (
    echo ERROR: Fallo la construccion del proyecto
    pause
    exit /b 1
)

:: Buscar regasm.exe
echo.
echo 2. Buscando regasm.exe...

set REGASM_PATH=
set FRAMEWORK_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe
set FRAMEWORK_PATH_32=C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe

if exist "%FRAMEWORK_PATH%" (
    set REGASM_PATH=%FRAMEWORK_PATH%
    echo Encontrado regasm.exe 64-bit: %FRAMEWORK_PATH%
) else if exist "%FRAMEWORK_PATH_32%" (
    set REGASM_PATH=%FRAMEWORK_PATH_32%
    echo Encontrado regasm.exe 32-bit: %FRAMEWORK_PATH_32%
) else (
    echo ERROR: No se pudo encontrar regasm.exe
    echo Verifica que .NET Framework 4.0+ este instalado
    pause
    exit /b 1
)

:: Registrar el COM object
echo.
echo 3. Registrando COM object...
set DLL_PATH=bin\Release\net48\TicTacToe.ComObject.dll

if exist "%DLL_PATH%" (
    echo Registrando: %DLL_PATH%
    "%REGASM_PATH%" "%DLL_PATH%" /tlb /codebase
    
    if %errorLevel% == 0 (
        echo.
        echo ================================================
        echo COM Object registrado exitosamente!
        echo ================================================
        echo.
        echo Para desregistrar, ejecuta:
        echo "%REGASM_PATH%" "%DLL_PATH%" /unregister
        echo.
    ) else (
        echo ERROR: Fallo el registro del COM object
    )
) else (
    echo ERROR: No se encontro el archivo DLL: %DLL_PATH%
    echo Verifica que la construccion haya sido exitosa
)

echo.
echo Presiona cualquier tecla para continuar...
pause >nul