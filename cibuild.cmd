@setlocal enabledelayedexpansion

REM Parse Arguments.

set RoslynRoot=%~dp0
set BuildConfiguration=Debug
:ParseArguments
if "%1" == "" goto :DoneParsing
if /I "%1" == "/?" call :Usage && exit /b 1
if /I "%1" == "/debug" set BuildConfiguration=Debug&&shift&& goto :ParseArguments
if /I "%1" == "/release" set BuildConfiguration=Release&&shift&& goto :ParseArguments
if /I "%1" == "/test32" set Test64=false&&shift&& goto :ParseArguments
if /I "%1" == "/test64" set Test64=true&&shift&& goto :ParseArguments
call :Usage && exit /b 1
:DoneParsing

call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"

REM Build the compiler so we can self host it for the full build
nuget.exe restore -nocache -verbosity quiet %RoslynRoot%build/ToolsetPackages/project.json
nuget.exe restore -nocache -verbosity quiet %RoslynRoot%build/Toolset.sln
REM Set the build version only so the assembly version is set to the semantic version,
REM which allows analyzers to laod because the compiler has binding redirects to the
REM semantic version
msbuild /nologo /v:m /m /p:BuildVersion=0.0.0.0 %RoslynRoot%build/Toolset.sln /p:Configuration=%BuildConfiguration%

mkdir %RoslynRoot%Binaries\Bootstrap
move Binaries\%BuildConfiguration%\* %RoslynRoot%Binaries\Bootstrap
msbuild /v:m /t:Clean build/Toolset.sln /p:Configuration=%BuildConfiguration%
taskkill /F /IM vbcscompiler.exe

nuget.exe restore -nocache %RoslynRoot%build\ToolsetPackages\project.json
nuget.exe restore -nocache %RoslynRoot%Roslyn.sln
nuget.exe restore -nocache %RoslynRoot%src\Samples\Samples.sln
msbuild /v:m /m /p:BootstrapBuildPath=%RoslynRoot%Binaries\Bootstrap BuildAndTest.proj /p:Configuration=%BuildConfiguration% /p:Test64=%Test64%
if ERRORLEVEL 1 (
    taskkill /F /IM vbcscompiler.exe
    echo Build failed
    exit /b 1
)

REM Kill any instances of VBCSCompiler.exe to release locked files;
REM otherwise future CI runs may fail while trying to delete those files.
taskkill /F /IM vbcscompiler.exe

REM Verify that our project.lock.json files didn't change as a result of 
REM restore.  If they do then the commit changed the dependencies without 
REM updating the lock files.
REM git diff --exit-code --quiet
REM if ERRORLEVEL 1 (
REM    echo Commit changed dependencies without updating project.lock.json
REM    git diff --exit-code
REM    exit /b 1
REM )

REM It is okay and expected for taskkill to fail (it's a cleanup routine).  Ensure
REM caller sees successful exit.
exit /b 0

:Usage
@echo Usage: cibuild.cmd [/debug^|/release]
@echo   /debug 	Perform debug build.  This is the default.
@echo   /release Perform release build
@echo   /test32 Run unit tests in the 32-bit runner.  This is the default.
@echo   /test64 Run units tests in the 64-bit runner.
@goto :eof
