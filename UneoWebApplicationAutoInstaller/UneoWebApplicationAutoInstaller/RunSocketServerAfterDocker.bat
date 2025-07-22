@echo off
setlocal

REM Set current directory to the location of this script
set "currentDir=%~dp0"

REM Wait until the Docker backend process is running
:waitDocker
tasklist /fi "imagename eq com.docker.backend.exe" | find /i "com.docker.backend.exe" >nul
if errorlevel 1 (
    echo [INFO] Docker backend not running yet. Waiting 5 seconds...
    timeout /t 5 >nul
    goto waitDocker
)

REM Wait 15 seconds before launching app
timeout /t 15 >nul

REM Start your application once Docker is running
echo [INFO] Docker backend is running. Launching app...
start "" "%currentDir%UMonitorSocketServer\publish\UMonitorSocketServer.exe"
