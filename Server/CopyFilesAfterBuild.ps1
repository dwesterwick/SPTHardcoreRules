param (
    [string]$modName
)

Write-Host ('Copying server files for {0}...' -f $modName)

Set-Location $PSScriptRoot

$destinationAbsolute = Join-Path $PSScriptRoot ('..\..\..\SPT\user\mods\{0}-Server\' -f $modName)
$serverLibraryAbsolute = Join-Path $PSScriptRoot ('bin\Debug\{0}-Server\{0}-Server.dll' -f $modName)

try
{
    if (!(Test-Path -PathType Container $destinationAbsolute))
    {
        New-Item -Path $destinationAbsolute -ItemType Directory
    }

    Copy-Item -Path $serverLibraryAbsolute -Destination $destinationAbsolute -errorAction stop | Out-Null
}
catch
{
    Write-Error ('Could not copy file {0} to {1}: {2}' -f $serverLibraryAbsolute, $destinationAbsolute, $_.Exception.Message)
    exit 1
}

Write-Host ('Copying server files for {0}...done.' -f $modName)