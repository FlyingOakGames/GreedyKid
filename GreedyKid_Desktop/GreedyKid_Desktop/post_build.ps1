$ErrorActionPreference = "Stop"

$directory = $args[0]
$identifier = $args[1]
$projectPath = $args[2]

Write-Host "Repairing white spaces in Launch.sh in case git overriden them..." -ForegroundColor DarkCyan

if (Test-Path $directory"Launch.sh") {
    $original_file = "$($directory)Launch.sh"
    $text = [IO.File]::ReadAllText($original_file) -replace "`r`n", "`n"
    [IO.File]::WriteAllText($original_file, $text)
}

if (!(Test-Path $directory"MonoGame.Framework.dll")) {
    Write-Host "Looks like we're on NativeAOT, let's remove useless json files..." -ForegroundColor DarkCyan
    Get-ChildItem -Path $directory *.json | foreach { Remove-Item -Path $_.FullName }
}

if ($identifier -eq "osx-x64" -Or $identifier -eq "osx-arm64") {
    Write-Host "Creating macOS app bundle..." -ForegroundColor DarkCyan

    Remove-Item "$($directory)GreedyKid.app/" -Recurse -ErrorAction SilentlyContinue

    if (!(Test-Path "$($directory)GreedyKid.app/Contents/MacOS/")) {
        New-Item "$($directory)GreedyKid.app/Contents/MacOS/" -Type Directory >$null
    }
    Move-Item -Path (Get-Item -Path "$($directory)/*" -Exclude ('GreedyKid.app')).FullName -Destination "$($directory)GreedyKid.app/Contents/MacOS/" -Force

    Copy-Item -Path "$($projectPath)Info.plist" -Destination "$($directory)GreedyKid.app/Contents/Info.plist" -Force

    if (!(Test-Path "$($directory)GreedyKid.app/Contents/Resources/")) {
        New-Item "$($directory)GreedyKid.app/Contents/Resources/" -Type Directory >$null
    }
    Copy-Item -Path "$($projectPath)GreedyKid.icns" -Destination "$($directory)GreedyKid.app/Contents/Resources/GreedyKid.icns" -Force

    if ((Test-Path "$($directory)GreedyKid.app/Contents/MacOS/Content/")) {
        Move-Item -Path "$($directory)GreedyKid.app/Contents/MacOS/Content" -Destination "$($directory)GreedyKid.app/Contents/Resources/Content" -Force
    }

    $original_file = "$($directory)GreedyKid.app/Contents/MacOS/Launch.sh"
    $text = [IO.File]::ReadAllText($original_file) -replace "`r`n", "`n"
    [IO.File]::WriteAllText($original_file, $text)
}

Write-Host "Done!" -ForegroundColor Green
