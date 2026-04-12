@echo off
setlocal EnableDelayedExpansion

set "SRC=%~1"
set "OUT=%~2"

if "%SRC%"=="" (
  echo [build-current] 缺少源文件参数
  exit /b 1
)
if "%OUT%"=="" (
  echo [build-current] 缺少输出文件参数
  exit /b 1
)

for %%I in ("%OUT%") do set "OUTDIR=%%~dpI"
if not exist "%OUTDIR%" mkdir "%OUTDIR%"

for %%I in ("%SRC%") do set "SRCDIR=%%~dpI"
set "MODE=single"
if exist "%SRCDIR%main.cpp" set "MODE=project"

set "SOURCES="
if /I "%MODE%"=="project" (
  for %%f in ("%SRCDIR%*.cpp") do set "SOURCES=!SOURCES! "%%~ff""
  set "PROJECT_DIR=%SRCDIR%"
  if "!PROJECT_DIR:~-1!"=="\" set "PROJECT_DIR=!PROJECT_DIR:~0,-1!"
  for %%D in ("!PROJECT_DIR!") do set "PROJECT_NAME=%%~nD"
  if "!PROJECT_NAME!"=="" set "PROJECT_NAME=app"
  set "OUT=!OUTDIR!!PROJECT_NAME!.exe"
) else (
  set "SOURCES="%SRC%""
)

where g++ >nul 2>nul
if %errorlevel%==0 (
  if /I "%MODE%"=="project" (
    echo [build-current] 使用 g++ 编译（目录项目）
  ) else (
    echo [build-current] 使用 g++ 编译（单文件）
  )
  g++ -g -std=c++17 -Wall !SOURCES! -o "%OUT%"
  exit /b %errorlevel%
)

set "VSWHERE=C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
  echo [build-current] 未检测到 g++，且找不到 vswhere.exe
  exit /b 1
)

set "CLPATH="
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualCpp.Tools.HostX64.TargetX64 -find VC\Tools\MSVC\**\bin\Hostx64\x64\cl.exe`) do (
  set "CLPATH=%%i"
  goto :found_cl
)

echo [build-current] 未检测到 g++，且未定位到 cl.exe
exit /b 1

:found_cl
for %%D in ("%CLPATH%") do set "CLDIR=%%~dpD"
call "%CLDIR%..\..\..\..\..\..\Auxiliary\Build\vcvars64.bat" >nul 2>nul
if errorlevel 1 (
  echo [build-current] vcvars64.bat 初始化失败
  exit /b 1
)

if /I "%MODE%"=="project" (
  echo [build-current] 使用 MSVC cl.exe 编译（目录项目）
) else (
  echo [build-current] 使用 MSVC cl.exe 编译（单文件）
)
cl.exe /nologo /Zi /EHsc /std:c++17 /utf-8 !SOURCES! /Fe:"%OUT%"
exit /b %errorlevel%
