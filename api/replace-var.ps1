#!/usr/bin/env pwsh

# PowerShell script to replace var with explicit types in C# files

param(
    [string]$FilePath
)

if (-not (Test-Path $FilePath)) {
    Write-Error "File not found: $FilePath"
    exit 1
}

$content = Get-Content $FilePath -Raw
$lines = Get-Content $FilePath

# Common patterns and their replacements
$patterns = @(
    # Lists and collections
    @{ Pattern = '(\s+)var (\w+) = new List<(.+?)>\('; Replace = '$1List<$3> $2 = new List<$3>(' },
    @{ Pattern = '(\s+)var (\w+) = new Dictionary<(.+?)>\('; Replace = '$1Dictionary<$3> $2 = new Dictionary<$3>(' },
    @{ Pattern = '(\s+)var (\w+) = new HashSet<(.+?)>\('; Replace = '$1HashSet<$3> $2 = new HashSet<$3>(' },
    
    # LINQ queries returning lists
    @{ Pattern = '(\s+)var (\w+) = (.*?)\.ToList\(\);'; Replace = '$1List<dynamic> $2 = $3.ToList();' },
    @{ Pattern = '(\s+)var (\w+) = (.*?)\.ToArray\(\);'; Replace = '$1dynamic[] $2 = $3.ToArray();' },
    
    # Async operations
    @{ Pattern = '(\s+)var (\w+) = await _context\.Users\.'; Replace = '$1User? $2 = await _context.Users.' },
    @{ Pattern = '(\s+)var (\w+) = await _context\.Resources\.'; Replace = '$1Resource? $2 = await _context.Resources.' },
    @{ Pattern = '(\s+)var (\w+) = await _context\.Students\.'; Replace = '$1Student? $2 = await _context.Students.' },
    
    # Common types
    @{ Pattern = '(\s+)var (\w+) = Guid\.Parse\('; Replace = '$1Guid $2 = Guid.Parse(' },
    @{ Pattern = '(\s+)var (\w+) = DateTime\.'; Replace = '$1DateTime $2 = DateTime.' },
    @{ Pattern = '(\s+)var (\w+) = new StringBuilder\('; Replace = '$1StringBuilder $2 = new StringBuilder(' },
    @{ Pattern = '(\s+)var (\w+) = string\.'; Replace = '$1string $2 = string.' },
    
    # Numbers
    @{ Pattern = '(\s+)var (\w+) = (\d+);'; Replace = '$1int $2 = $3;' },
    @{ Pattern = '(\s+)var (\w+) = (\d+\.\d+);'; Replace = '$1double $2 = $3;' },
    @{ Pattern = '(\s+)var (\w+) = (\d+)m;'; Replace = '$1decimal $2 = $3m;' },
    
    # Booleans
    @{ Pattern = '(\s+)var (\w+) = true;'; Replace = '$1bool $2 = true;' },
    @{ Pattern = '(\s+)var (\w+) = false;'; Replace = '$1bool $2 = false;' },
    
    # New object creation
    @{ Pattern = '(\s+)var (\w+) = new (\w+)\('; Replace = '$1$3 $2 = new $3(' },
    @{ Pattern = '(\s+)var (\w+) = new (\w+)\s*\{'; Replace = '$1$3 $2 = new $3 {' }
)

# Apply patterns
foreach ($pattern in $patterns) {
    $content = $content -replace $pattern.Pattern, $pattern.Replace
}

# Manual inspection needed for complex cases
$remainingVars = [regex]::Matches($content, '\bvar\s+\w+\s*=')
if ($remainingVars.Count -gt 0) {
    Write-Host "Remaining var declarations that need manual review: $($remainingVars.Count)"
    foreach ($match in $remainingVars) {
        $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
        Write-Host "  Line $lineNumber`: $($match.Value)"
    }
}

# Save the modified content
Set-Content -Path $FilePath -Value $content -NoNewline
Write-Host "Updated $FilePath"