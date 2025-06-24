param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Switch]$DryRun
)

Write-Host "Setting version to $Version"
if ($DryRun) {
    Write-Host "Dry run mode: no files will be changed."
}

# 1. Find all .csproj files
$csprojFiles = @(Get-ChildItem -Recurse -Filter *.csproj)

foreach ($csproj in $csprojFiles) {
    Write-Host "Updating version in $($csproj.FullName)"
    
    try {
        [xml]$xml = Get-Content $csproj.FullName
        $versionNodes = @(
            "//Project/PropertyGroup/Version",
            "//Project/PropertyGroup/AssemblyVersion",
            "//Project/PropertyGroup/FileVersion",
            "//Project/PropertyGroup/InformationalVersion"
        )
        
        foreach ($xpath in $versionNodes) {
            $node = $xml.SelectSingleNode($xpath)
            if ($null -ne $node) {
                $node.InnerText = $Version
            } else {
                # If the tag doesn't exist, add it to the first property group
                $pg = @($xml.Project.PropertyGroup)[0]
                if ($null -ne $pg) {
                    $tagName = $xpath.Split('/')[-1]
                    $newNode = $xml.CreateElement($tagName)
                    $newNode.InnerText = $Version
                    $pg.AppendChild($newNode) | Out-Null
                }
            }
        }
        if (-not $DryRun)
        {
            $xml.Save($csproj.FullName)
        }
    }
    catch {
        Write-Error "Failed to update $($csproj.FullName): $_"
        exit 1
    }
}

# 2. Build and Publish
Write-Host "Publishing solution..."
dotnet publish ./Vectra.CLI/Vectra.CLI.csproj -c Release -o ./out

Write-Host "Build and publish complete. Output is in ./out"