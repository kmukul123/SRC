<#

.SYNOPSIS
This is a Powershell script to copy and rename a project template.

.DESCRIPTION
This Powershell script will download NuGet if missing, restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER CopyTo 
The location to copy the solution to.
.PARAMETER RenameTo
The name of the new solution.

#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)]
    [string]$CopyTo,
    
    [Parameter(Mandatory=$true)]
    [string]$RenameTo
    
)

$scriptName = $MyInvocation.MyCommand.Name 
$scriptDir = Split-Path -parent $MyInvocation.MyCommand.Definition

$newLocation = $CopyTo
$oldName = "AzureFunction.IOCSample"
$newName = $RenameTo

Write-Host
Write-Host "Running $scriptName"
Write-Host

Write-Host "//////////////////////////////////////////////////////////////////////"
Write-Host "//"
Write-Host "// Copying all files and folders (recursively) from '$scriptDir'"
Write-Host "// to '$newLocation'"
Write-Host "//"
Write-Host "//////////////////////////////////////////////////////////////////////"
Write-Host ""

Copy-Item -Path $scriptDir -Destination $newLocation -recurse -Force
Set-Location -Path $newLocation

If (Test-Path $scriptName) {
    Remove-Item $scriptName
}

Write-Host "//////////////////////////////////////////////////////////////////////////////"
Write-Host "//"
Write-Host "// Searching for files with text '$oldName' and replacing it with '$newName'"
Write-Host "//"
Write-Host "//////////////////////////////////////////////////////////////////////////////"
Write-Host ""

$filesToUpdate = Get-ChildItem -recurse | Select-String -pattern $oldName | group path | select name
$filesToUpdateCount = $filesToUpdate.Count

Write-Host "Found $filesToUpdateCount file(s) containing '$oldName'"
Write-Host "Updating..."

Foreach ($fileToUpdate in $filesToUpdate){
    If ($fileToUpdate.Name -ne "InputStream"){
        $file = Get-Item $fileToUpdate.Name 
        (Get-Content $file) | Foreach-Object { $_ -replace $oldName, $newName } | Set-Content $file
        
        Write-Host $fileToUpdate.Name
    }
}


Write-Host ""
Write-Host "//////////////////////////////////////////////////////////////////////////////"
Write-Host "//"
Write-Host "// Renaming files and folders..."
Write-Host "//"
Write-Host "//////////////////////////////////////////////////////////////////////////////"
Write-Host ""

# Rename folders
$foldersToRename = Get-ChildItem -filter *$oldName* -recurse | ? { $_.PSIsContainer } 

Foreach ($folderToRename in $foldersToRename){
    $oldPath = $folderToRename.FullName
    $newFolderName = $folderToRename -replace $oldName, $newName
    
    Write-Host "Renaming '$folderToRename' to '$newFolderName'"   
    
    Rename-Item -Path $oldPath -NewName $newFolderName
}

# Rename files
$filesToRename = Get-ChildItem -filter *$oldName* -recurse | ? { ! $_.PSIsContainer }

Foreach ($fileToRename in $filesToRename){
    $oldFileName = $fileToRename.FullName
    $newFileName = $fileToRename -replace $oldName, $newName
    
    Write-Host "Renaming '$fileToRename' to '$newFileName'"   
    
    Rename-Item -Path $oldFileName -NewName $newFileName
}



Set-Location -Path $scriptDir
Write-Host