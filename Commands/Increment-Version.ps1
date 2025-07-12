# Increment-Version.ps1 v1.1.1

$sourceFile = "Plugin.cs"
$assemblyInfoFile = "AssemblyInfo.cs"
$aboutFile = "About.xml"

# --- Changelog Section --- #
$fullPath = Resolve-Path $sourceFile
$parentFolder = Split-Path $fullPath -Parent
$folderName = Split-Path $parentFolder -Leaf

if ($folderName -ieq "Template") {
    Write-Host "Versioning cancelled: $sourceFile is inside a 'Template' folder."
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

    [System.IO.File]::WriteAllText($fullPath, $newContent, [System.Text.Encoding]::UTF8)
    Write-Host "Version updated to $newVersionString"
} else {
    Write-Warning "Version pattern not found in $sourceFile"
    exit 1
}

# --- AssemblyInfo.cs Section --- #
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
[System.IO.File]::WriteAllText($assemblyPath, $assemblyContent, [System.Text.Encoding]::UTF8)
Write-Host "$assemblyInfoFile generated at: $assemblyPath"

# --- About.xml Section --- #
$aboutPath = Join-Path $parentFolder "About/About.xml"
if (Test-Path $aboutPath) {
    $aboutContent = Get-Content $aboutPath -Raw

    if ($aboutContent -match '<Version>\s*([\d\.]+)\s*</Version>') {
        $aboutContent = [regex]::Replace(
            $aboutContent,
            '<Version>\s*([\d\.]+)\s*</Version>',
            "<Version>$newVersionString</Version>"
        )
        [System.IO.File]::WriteAllText($aboutPath, $aboutContent, [System.Text.Encoding]::UTF8)
        Write-Host "About.xml version updated to $newVersionString"
    } else {
        Write-Warning "<Version> tag not found in About.xml"
    }
} else {
    Write-Warning "About.xml not found in $parentFolder"
}

# --- Changelog Section --- #
$commitCommentPattern = '<!--\s*LastProcessedCommit:\s*([a-f0-9]{7,40})\s*-->'
$lastProcessedCommit = $null

if ($aboutContent -match $commitCommentPattern) {
    $lastProcessedCommit = $matches[1]
    Write-Host "Found last processed commit: $lastProcessedCommit"
}

$currentCommit = (git rev-parse HEAD).Trim()
if (-not $currentCommit) {
    Write-Warning "Unable to get current commit hash"
} else {
    if ($lastProcessedCommit) {
        $gitMessages = git log "$lastProcessedCommit..HEAD" --pretty=format:"- %s" 2>&1
    } else {
        $gitMessages = git log -n 5 --pretty=format:"- %s" 2>&1
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Git log failed: $gitMessages"
    } else {
        $logBody = ($gitMessages -join "`n")
        $changelogText = "<ChangeLog>`n$logBody`n</ChangeLog>"

        $aboutContent = [regex]::Replace(
            $aboutContent,
            '<ChangeLog>.*?</ChangeLog>',
            '',
            [System.Text.RegularExpressions.RegexOptions]::Singleline
        )

        # Re-insert new <ChangeLog> before </ModMetadata>
        $aboutContent = $aboutContent -replace '</ModMetadata>', "$changelogText`n</ModMetadata>"

        $commitComment = "<!-- LastProcessedCommit: $currentCommit -->"
        if ($aboutContent -match $commitCommentPattern) {
            $aboutContent = [regex]::Replace($aboutContent, $commitCommentPattern, $commitComment)
        } else {
            $aboutContent += "`n$commitComment"
        }

        [System.IO.File]::WriteAllText($aboutPath, $aboutContent, [System.Text.Encoding]::UTF8)
        Write-Host "ChangeLog updated from commits since $lastProcessedCommit"
    }
}