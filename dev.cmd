@echo off

if "%~1"=="" goto :help
if /I "%~1"=="help" goto :help
if /I "%~1"=="-h" goto :help
if /I "%~1"=="/h" goto :help
if /I "%~1"=="/?" goto :help

powershell -ExecutionPolicy Bypass -File "%~dp0scripts\stack.ps1" %*
exit /b %errorlevel%

:help
echo Usage: dev.cmd ^<action^>
echo Actions: up ^| down ^| reset ^| status ^| logs ^| test ^| test-e2e ^| test-all ^| ci
echo.
echo Local CI (pre-push): .\scripts\ci-local.ps1
exit /b 0
