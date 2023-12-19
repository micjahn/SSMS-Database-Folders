@ECHO OFF

SETLOCAL EnableDelayedExpansion EnableExtensions

SET VERSION=1.0.2

SET CURRENT_DIR=%CD%
SET BUILD_DIR=%CD%\Build
SET LOGFILE=%CD%\build_deployment.log
SET DEPLOYMENT_DIR=%BUILD_DIR%\Deployment
SET BINARY_DIR=%BUILD_DIR%\Release
SET FILENAME_BINARY=%DEPLOYMENT_DIR%\SSMSDatabaseFolders.%VERSION%.zip
SET ZIP_TOOL=%CD%\3rdparty\zip\7za.exe
SET HAS_VALIDATION_ERROR=0

echo. > %LOGFILE%

echo.
echo Check for missing files...
echo.

CD "%BINARY_DIR%"

FOR /F %%b IN (build_deployment_files.txt) DO (
 SET f=%%b
 SET f=!f:%%BINARY_DIR%%=%BINARY_DIR%!
 SET f=!f:%%CURRENT_DIR%%=%CURRENT_DIR%!
 IF NOT EXIST !f! (
  ECHO The file !f! were not found
  SET HAS_VALIDATION_ERROR=1
 )
)

CD "%CURRENT_DIR%"

IF NOT "!HAS_VALIDATION_ERROR!" == "0" (
 ECHO.
 ECHO The file validation procedure was not successful.
 GOTO END
)

ECHO.
ECHO Build deployment files in directory
ECHO %DEPLOYMENT_DIR%...
ECHO.

REM
REM prepare deployment directory
REM ***************************************************************************************

IF NOT EXIST "%BUILD_DIR%" GOTO BUILD_DIR_NOT_FOUND
IF NOT EXIST "%BINARY_DIR%" GOTO BINARY_DIR_NOT_FOUND

MKDIR "%DEPLOYMENT_DIR%" >NUL: 2>&1
DEL /F "%DEPLOYMENT_DIR%\%FILENAME_BINARY%" >NUL: 2>&1

REM
REM build archives for binaries
REM ***************************************************************************************

CD "%BINARY_DIR%"

echo Build assembly archive...
echo.

"%ZIP_TOOL%" a -tzip -mx9 -r "%FILENAME_BINARY%" -i@%CURRENT_DIR%\build_deployment_files.txt >> %LOGFILE% 2>&1
if ERRORLEVEL 1 GOTO ERROR_OPERATION

CD "%CURRENT_DIR%"

GOTO END

:BUILD_DIR_NOT_FOUND
ECHO The directory 
ECHO %BUILD_DIR%
ECHO doesn't exist.
ECHO.
GOTO END

:BINARY_DIR_NOT_FOUND
ECHO The directory 
ECHO %BINARY_DIR%
ECHO doesn't exist.
ECHO.
GOTO END

:ERROR_OPERATION
ECHO An error occurred, please check the logfile %LOGFILE%

:END

ENDLOCAL
