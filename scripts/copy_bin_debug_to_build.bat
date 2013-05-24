@echo off
rem Copies data from {svnRoot}/source/Stareater.UI.WinForms/bin/Debug/
rem to {svnRoot}/build
@echo on

robocopy ../source/Stareater.UI.WinForms/bin/Debug/data/ ../build/data/ /e
robocopy ../source/Stareater.UI.WinForms/bin/Debug/languages/ ../build/languages/ /e
robocopy ../source/Stareater.UI.WinForms/bin/Debug/maps/ ../build/maps/ /e
robocopy ../source/Stareater.UI.WinForms/bin/Debug/players/ ../build/players/ /e
robocopy ../source/Stareater.UI.WinForms/bin/Debug/images/ ../build/images/ /e
pause
