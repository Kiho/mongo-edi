@ECHO OFF
ECHO.
ECHO.
ECHO This script deletes all temporary build files in their
ECHO corresponding BIN and OBJ Folder contained in the following projects
ECHO.
ECHO AvalonDock
ECHO AvalonDock.Themes.Aero
ECHO AvalonDock.Theme.ExpressionDark
ECHO AvalonDock.Theme.VS2010
ECHO Edi
ECHO EdiViews
ECHO Themes
REM ECHO FindReplace
ECHO ICSharpCode.AvalonEdit
ECHO SimpleControls
ECHO.
ECHO.
REM Ask the user if hes really sure to continue beyond this point XXXXXXXX
set /p choice=Are you sure to continue (Y/N)?
if not '%choice%'=='Y' Goto EndOfBatch
REM Script does not continue unless user types 'Y' in upper case letter
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO Deleting BIN and OBJ Folders in AvalonDock
ECHO.
RMDIR /S /Q AvalonDock\bin
RMDIR /S /Q AvalonDock\obj
REM DEL /F /Q /S /A:H Automation.Tasks.Core\StyleCop.Cache >> C:\TEMP\CleanFiles.txt

RMDIR /S /Q Edi\bin
RMDIR /S /Q Edi\obj

RMDIR /S /Q EdiViews\bin
RMDIR /S /Q EdiViews\obj

RMDIR /S /Q AvalonDock.Themes.Aero\bin
RMDIR /S /Q AvalonDock.Themes.Aero\obj

RMDIR /S /Q AvalonDock.Theme.VS2010\bin
RMDIR /S /Q AvalonDock.Theme.VS2010\obj

RMDIR /S /Q AvalonDock.Theme.ExpressionDark\bin
RMDIR /S /Q AvalonDock.Theme.ExpressionDark\obj

RMDIR /S /Q Themes\bin
RMDIR /S /Q Themes\obj

RMDIR /S /Q ICSharpCode.AvalonEdit\bin
RMDIR /S /Q ICSharpCode.AvalonEdit\obj

RMDIR /S /Q SimpleControls\bin
RMDIR /S /Q SimpleControls\obj

REM RMDIR /S /Q FindReplace\bin
REM RMDIR /S /Q FindReplace\obj

PAUSE

:EndOfBatch
	