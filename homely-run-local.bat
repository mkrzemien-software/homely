@echo off
wt new-tab --tabColor "#ffb3ba" --title "Claude" -p "Command Prompt" -d "."  cmd /k "claude" ^
; new-tab --tabColor "#ffdfba" --title "Frontend" -p "Command Prompt" -d ".\frontend"  cmd /k "npm start" ^
; new-tab --tabColor "#e1baff" --title "Backend" -p "PowerShell" -d ".\backend\HomelyApi\Homely.API"  powershell -NoExit -Command "dotnet run"