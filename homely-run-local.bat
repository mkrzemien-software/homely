@echo off

wt new-tab --tabColor "#ffb3ba" -p "Command Prompt" -d "." --title "Claude" cmd /k "claude" ^
; new-tab --tabColor "#ffdfba" -p "Git Bash" -d ".\frontend" --title "Frontend" bash -c "npm run start" ^
; new-tab --tabColor "#e1baff" -p "PowerShell" -d ".\backend\HomelyApi\Homely.API" --title "Backend" powershell -NoExit -Command "dotnet run"