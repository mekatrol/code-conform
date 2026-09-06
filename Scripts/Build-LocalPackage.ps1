param(
    [string] $PackageSource = 'D:\NuGetLocal',
    [string] $Version
)

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repositoryRoot 'CodeConform.slnx'
$packageProject = Join-Path $repositoryRoot 'CodeConform.CSharp.Analyzers\CodeConform.CSharp.Analyzers.csproj'

New-Item -ItemType Directory -Path $PackageSource -Force | Out-Null

dotnet clean $solution -c Release

if ($LASTEXITCODE -ne 0) {
    throw 'Could not clean the CodeConform solution.'
}

dotnet restore $solution

if ($LASTEXITCODE -ne 0) {
    throw 'Could not restore the CodeConform solution.'
}

dotnet build $solution -c Release --no-restore

if ($LASTEXITCODE -ne 0) {
    throw 'The clean CodeConform solution build failed. The package was not updated.'
}

dotnet test $solution -c Release --no-build --no-restore

if ($LASTEXITCODE -ne 0) {
    throw 'CodeConform tests failed. The package was not updated.'
}

$projectContents = [System.IO.File]::ReadAllText($packageProject)
$versionMatches = [regex]::Matches(
    $projectContents,
    '<Version>(?<version>[^<]+)</Version>'
)

if ($versionMatches.Count -ne 1) {
    throw "Expected one Version property in $packageProject; found $($versionMatches.Count)."
}

$currentVersion = $versionMatches[0].Groups['version'].Value
$semanticVersionPattern = '^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$'

if ([string]::IsNullOrWhiteSpace($Version)) {
    $semanticVersion = [regex]::Match($currentVersion, $semanticVersionPattern)

    if (-not $semanticVersion.Success) {
        throw "Cannot increment package version '$currentVersion'."
    }

    $major = [int] $semanticVersion.Groups['major'].Value
    $minor = [int] $semanticVersion.Groups['minor'].Value + 1
    $Version = "$major.$minor.0"
}
elseif (-not [regex]::IsMatch($Version, $semanticVersionPattern)) {
    throw "Package version '$Version' is not a valid semantic version."
}

$versionGroup = $versionMatches[0].Groups['version']
$updatedProjectContents = $projectContents.Remove(
    $versionGroup.Index,
    $versionGroup.Length
).Insert($versionGroup.Index, $Version)

[System.IO.File]::WriteAllText(
    $packageProject,
    $updatedProjectContents,
    [System.Text.UTF8Encoding]::new($true)
)

$packArguments = @(
    'pack'
    $packageProject
    '-c'
    'Release'
    '--no-build'
    '--no-restore'
    '--output'
    $PackageSource
)

dotnet @packArguments

if ($LASTEXITCODE -ne 0) {
    [System.IO.File]::WriteAllText(
        $packageProject,
        $projectContents,
        [System.Text.UTF8Encoding]::new($true)
    )

    throw "Could not build the CodeConform package in $PackageSource."
}

Write-Host "CodeConform package version updated from $currentVersion to $Version."
Write-Host "Package written to $PackageSource."
