#!/bin/bash

set -e

# 1. Check for version argument
if [ -z "$1" ]; then
  echo "Missing next version"
  exit 1
fi

VERSION="$1"

echo "Setting version to $VERSION"

# 2. Update CSPROJ files
find . -name "*.csproj" | while read -r csproj; do
  echo "Updating version in $csproj"
  
  # Use xmlstarlet if available, otherwise use sed
  if command -v xmlstarlet >/dev/null 2>&1; then
    xmlstarlet ed -L \
      -u "//Project/PropertyGroup/Version" -v "$VERSION" \
      -u "//Project/PropertyGroup/AssemblyVersion" -v "$VERSION" \
      -u "//Project/PropertyGroup/FileVersion" -v "$VERSION" \
      -u "//Project/PropertyGroup/InformationalVersion" -v "$VERSION" \
      "$csproj"
  else
    sed -i.bak "s|<Version>.*</Version>|<Version>$VERSION</Version>|" "$csproj"
    sed -i.bak "s|<AssemblyVersion>.*</AssemblyVersion>|<AssemblyVersion>$VERSION</AssemblyVersion>|" "$csproj"
    sed -i.bak "s|<FileVersion>.*</FileVersion>|<FileVersion>$VERSION</FileVersion>|" "$csproj"
    sed -i.bak "s|<InformationalVersion>.*</InformationalVersion>|<InformationalVersion>$VERSION</InformationalVersion>|" "$csproj"
    rm "$csproj.bak"
  fi
done

# 3. Build and publish
echo "Publishing solution..."
dotnet publish ./Vectra.CLI/Vectra.CLI.csproj -c Release -o ./out

echo "Build and publish complete. Output is in ./out"