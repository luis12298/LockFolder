@echo off
set /p MI_VARIABLE=Introduce la contrasena: 
setx PASS_FOLDER "%MI_VARIABLE%" /M

echo Requiere de reinicio
pause
