# Builds the nuget package for the SlugEnt FluentAssertions
# And Copies it to Local Repository.

$Projects =@(
  'TimeUnit'
)

$repo = Read-Host "Which Repository to push to:  L = Local,  N = Nuget Public"
$repo = $repo.ToUpper()

$repository = "L"
if ($repo -eq "N") { $repository = "N"}

Write-Host "Script= $PSScriptRoot"

$scriptPath = $PSScriptRoot


& $Env:SlugEnt_DevScripts\BuildAndDeployToNuget_ver0002.ps1 $Projects $PSScriptRoot $repository