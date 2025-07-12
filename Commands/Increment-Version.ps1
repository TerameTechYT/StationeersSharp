# Increment-Version.ps1 v1.0.2.0

$sourceFile = "Plugin.cs"

$fullPath = Resolve-Path $sourceFile
$parentFolder = Split-Path $fullPath -Parent | Split-Path -Leaf

if ($parentFolder -ieq "Template") {
    Write-Host "Versioning cancelled: Plugin.cs is inside a 'Template' folder."
    exit 0
}

$content = Get-Content $fullPath -Raw
$pattern = 'Version\s*=\s*new\s+Version\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)'

if ($content -match $pattern) {
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

    $newVersion = "Version = new Version($major, $minor, $build, $revision)"

    $newContent = [regex]::Replace($content, $pattern, {
        param($m) $newVersion
    })

    Set-Content -Path $fullPath -Value $newContent -Encoding UTF8

    Write-Host "Version updated to $major.$minor.$build.$revision"
} else {
    Write-Warning "Version pattern not found in file."
}
