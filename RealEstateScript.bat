@echo off
echo ============================================
echo   Deploying to GitHub Pages
echo ============================================
echo.

REM Step 1: Ensure we're on master branch
echo [1/5] Checking out master branch...
git checkout master
if errorlevel 1 (
    echo Error: Failed to checkout master branch
    pause
    exit /b 1
)
echo.

REM Step 2: Start MVC app in background
echo [2/5] Starting MVC app...
cd SCRANKRealEstate
start /B dotnet run
echo Waiting for app to start (15 seconds)...
timeout /t 15 /nobreak >nul
cd ..
echo.

REM Step 3: Generate static site
echo [3/5] Generating static site...
cd SCRANKRealEstate.StaticGen
dotnet run
if errorlevel 1 (
    echo Error: Failed to generate static site
    taskkill /F /IM dotnet.exe >nul 2>&1
    cd ..
    pause
    exit /b 1
)
cd ..
echo.

REM Step 4: Stop MVC app
echo [4/5] Stopping MVC app...
taskkill /F /IM dotnet.exe >nul 2>&1
echo.

REM Step 5: Deploy to GitHub Pages
echo [5/5] Deploying to GitHub Pages...
cd SCRANKRealEstate\wwwroot-static

REM Clean up any existing git
if exist .git (
    rmdir /s /q .git
)

REM Initialize and deploy
git init
git add .
git commit -m "Deploy static site - %date% %time%"
git remote add origin https://github.com/chan0206/SCRANKRealEstate.git
git push -f origin master:gh-pages

if errorlevel 1 (
    echo Error: Failed to push to GitHub
    cd ..\..
    pause
    exit /b 1
)

cd ..\..
echo.
echo ============================================
echo   Deployment Complete!
echo ============================================
echo.
echo Your site will be live at:
echo https://chan0206.github.io/SCRANKRealEstate/
echo.
echo Note: It may take 1-2 minutes to update.
echo ============================================
pause