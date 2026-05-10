# 创建快捷方式到启动文件夹
$SourceFilePath = "C:\Users\Administrator\Desktop\EamilWebApp\EamilWebApp\EamilWebApp.exe"
$ShortcutPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\EamilWebApp.lnk"

# 创建快捷方式
$WScriptShell = New-Object -ComObject WScript.Shell
$Shortcut = $WScriptShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = $SourceFilePath
$Shortcut.WorkingDirectory = "C:\Users\Administrator\Desktop\EamilWebApp\EamilWebApp"
$Shortcut.Save()

Write-Host "Startup item has been created" -ForegroundColor Green