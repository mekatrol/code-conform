param(
    [string] $PackageSource = 'D:\NuGetLocal'
)

$solutionFiles = @(Get-ChildItem -File | Where-Object Extension -In '.sln', '.slnx')

if ($solutionFiles.Count -ne 1) {
    throw "Expected one .sln or .slnx file in the current directory; found $($solutionFiles.Count)."
}

$solution = $solutionFiles[0]
$projectList = dotnet sln $solution.FullName list

if ($LASTEXITCODE -ne 0) {
    throw "Could not read the project list from $($solution.Name)."
}

$solutionDirectory = $solution.DirectoryName
$projects = @(
    $projectList |
        Where-Object { $_.Trim() -match '\.csproj$' } |
        ForEach-Object { [System.IO.Path]::GetFullPath($_.Trim(), $solutionDirectory) }
)

if ($projects.Count -eq 0) {
    throw "The solution does not contain any C# projects."
}

$isCodeConformSource = $projects | Where-Object {
    (Split-Path -Leaf $_) -eq 'CodeConform.CSharp.Analyzers.csproj'
}

if ($null -ne $isCodeConformSource) {
    Write-Warning 'CodeConform cannot install its analyzer package into its own solution because it would cause conflicts.'
    return
}

foreach ($project in $projects) {
    dotnet add $project package CodeConform.CSharp `
        --prerelease `
        --source $PackageSource

    if ($LASTEXITCODE -ne 0) {
        throw "Could not add or update CodeConform in $project."
    }
}
