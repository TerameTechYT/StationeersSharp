# Increment-Version.ps1 v1.0.5.0

$sourceFile = "Plugin.cs"
$assemblyInfoFile = "AssemblyInfo.cs"

$fullPath = Resolve-Path $sourceFile
$parentFolder = Split-Path $fullPath -Parent
$folderName = Split-Path $parentFolder -Leaf

if ($folderName -ieq "Template") {
    Write-Host "Versioning cancelled: Plugin.cs is inside a 'Template' folder."
    exit 0
}

$content = Get-Content $fullPath -Raw

$nameMatch = $content | Select-String 'Name\s*=\s*"([^"]+)"'
$guidMatch = $content | Select-String 'Guid\s*=\s*"([^"]+)"'
$workshopIdMatch = $content | Select-String 'WorkshopId\s*=\s*([0-9]+)ul'
$gameTypeMatch = $content | Select-String 'GameType\s*=\s*GameType\.([A-Za-z]+)'
$versionPattern = 'Version\s*=\s*new\s+Version\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)'

if ($content -match $versionPattern) {
    $major = [int]$matches[1]
    $minor = [int]$matches[2]
    $build = [int]$matches[3]
    $revision = [int]$matches[4]

    $revision++

    if ($revision -ge 1000) {
        $revision = 0
        $build++
        if ($build -ge 100) {
            $build = 0
            $minor++
            if ($minor -ge 10) {
                $minor = 0
                $major++
            }
        }
    }

    $newVersionString = "$major.$minor.$build.$revision"
    $newVersionCode = "Version = new Version($major, $minor, $build, $revision)"

    $newContent = [regex]::Replace($content, $versionPattern, {
        param($m) $newVersionCode
    })

    Set-Content -Path $fullPath -Value $newContent -Encoding UTF8
    Write-Host "Version updated to $newVersionString"
} else {
    Write-Warning "Version pattern not found in Plugin.cs"
    exit 1
}

$name = if ($nameMatch) { $nameMatch.Matches[0].Groups[1].Value } else { "unknown" }
$guid = if ($guidMatch) { $guidMatch.Matches[0].Groups[1].Value } else { "unknown" }
$workshopId = if ($workshopIdMatch) { $workshopIdMatch.Matches[0].Groups[1].Value } else { "unknown" }
$gameType = if ($gameTypeMatch) { $gameTypeMatch.Matches[0].Groups[1].Value } else { "unknown" }

$assemblyContent = @"
[assembly: AssemblyTitle("$name")]
[assembly: AssemblyDescription("Workshop ID: $workshopId | GameType: $gameType")]
[assembly: AssemblyCompany("Viven (@mommyvivi on discord)")]
[assembly: AssemblyProduct("$name")]
[assembly: AssemblyVersion("$newVersionString")]
[assembly: AssemblyFileVersion("$newVersionString")]
"@

$assemblyPath = Join-Path $parentFolder $assemblyInfoFile
Set-Content -Path $assemblyPath -Value $assemblyContent -Encoding UTF8
Write-Host "AssemblyInfo.cs generated at: $assemblyPath"