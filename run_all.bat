@echo off
setlocal

echo ===================================================
echo    UniversityManagement - Starting Services
echo ===================================================

:: ===================================================
:: Kill process using required ports
:: ===================================================

call :KillPort 5156
call :KillPort 5187
call :KillPort 5124
call :KillPort 5221
call :KillPort 5281

echo.
echo Waiting 2 seconds...
timeout /t 2 /nobreak >nul

:: ===================================================
:: Start Services
:: ===================================================

echo Starting UserService...
start "UserService - Port 5156" cmd /k dotnet run --project UserService

echo Starting SubjectService...
start "SubjectService - Port 5187" cmd /k dotnet run --project SubjectService

echo Starting ClassService...
start "ClassService - Port 5124" cmd /k dotnet run --project ClassService

echo Starting ScoreService...
start "ScoreService - Port 5221" cmd /k dotnet run --project ScoreService

echo Starting FrontendMVC...
start "FrontendMVC - Port 5281" cmd /k dotnet run --project FrontendMVC

echo.
echo ===================================================
echo   All services have been launched!
echo   Frontend: http://localhost:5281
echo ===================================================
pause
exit /b

:: ===================================================
:: Function: Kill process using a port
:: ===================================================
:KillPort
set PORT=%1

echo Checking port %PORT%...

for /f "tokens=5" %%a in ('netstat -ano ^| findstr :%PORT%') do (
    echo Port %PORT% is in use. Killing PID %%a...
    taskkill /PID %%a /F >nul 2>&1
)

exit /b