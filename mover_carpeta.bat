@echo off
set "source=%cd%"
set "destination=C:\LockFolder"
robocopy "%source%" "%destination%" /E
rd /S /Q "%source%"
pause
