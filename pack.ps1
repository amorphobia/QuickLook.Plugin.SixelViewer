$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    dotnet build -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Build Failed.' }

    $image  = 'QuickLook.Plugin.SixelViewer'
    $source = 'QuickLook.Plugin.SixelSource'
    $dist = New-Item -ItemType Directory -Force dist

    $version = git describe --always --tags
    if ($LASTEXITCODE -ne 0 -or -not $version) {
        throw 'Cannot get version number.'
    }

    $template = Join-Path $PSScriptRoot 'QuickLook.Plugin.Metadata.Base.config'
    $metadata = [xml](Get-Content -LiteralPath $template -Raw -Encoding utf8)
    $metadata.Metadata.Version = $version

    $generated = Join-Path $dist.FullName 'QuickLook.Plugin.Metadata.config'
    $metadata.Save($generated)

    $files = @(
        "src/$image/bin/Release/net462/$image.dll"
        "src/$source/bin/Release/net462/$source.dll"
        $generated
        'COPYING'
    )

    $zip = Join-Path $dist.FullName "$image-$version.zip"
    $package = [IO.Path]::ChangeExtension($zip, '.qlplugin')

    Compress-Archive -LiteralPath $files -DestinationPath $zip -Force
    Move-Item -LiteralPath $zip -Destination $package -Force
    Get-Item -LiteralPath $package
}
finally {
    Pop-Location
}
