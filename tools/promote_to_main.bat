@echo off
REM Thin launcher. Real script is promote_to_main.ps1 (UTF-8).
REM cmd.exe reads .bat in the ANSI codepage, so Korean text lives in the .ps1 instead.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0promote_to_main.ps1"
