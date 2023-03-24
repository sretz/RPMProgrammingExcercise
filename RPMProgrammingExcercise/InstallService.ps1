$path = Join-Path $PSScriptRoot "RPMProgrammingExcercise.exe"

New-Service -Name RPMProgrammingExcercise -BinaryPathName $path
