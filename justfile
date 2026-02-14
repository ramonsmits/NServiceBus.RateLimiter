solution := "src/NServiceBus.RateLimiter.sln"
project  := "src/NServiceBus.RateLimiter/NServiceBus.RateLimiter.csproj"
demo     := "src/Demo/Demo.csproj"
config   := "Release"
artifacts := "artifacts"

default: build

build:
    dotnet build {{solution}} -c {{config}}

clean:
    dotnet clean {{solution}}
    rm -rf {{artifacts}}

pack: clean
    dotnet pack {{project}} -c {{config}} -o {{artifacts}}

publish api_key="": pack
    #!/usr/bin/env bash
    set -euo pipefail
    key="${NUGET_API_KEY:-{{api_key}}}"
    [[ -z "$key" ]] && echo "Set NUGET_API_KEY or pass api_key argument" && exit 1
    dotnet nuget push '{{artifacts}}/*.nupkg' -s https://api.nuget.org/v3/index.json -k "$key"

release version="":
    #!/usr/bin/env bash
    set -euo pipefail
    last=$(git describe --tags --abbrev=0 2>/dev/null || echo "none")
    if [[ -z "{{version}}" ]]; then
        echo "Last version: $last"
        echo ""
        if [[ "$last" != "none" ]]; then
            git --no-pager log --oneline "$last"..HEAD
        else
            git --no-pager log --oneline
        fi
        echo ""
        read -rp "Enter version to release: " version
    else
        version="{{version}}"
    fi
    [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.]+)?$ ]] && echo "Invalid semver: $version" && exit 1
    grep -q '## \[Unreleased\]' CHANGELOG.md || { echo "No [Unreleased] section in CHANGELOG.md"; exit 1; }
    git diff --quiet && git diff --cached --quiet || { echo "Working tree is dirty, commit or stash first"; exit 1; }
    date=$(date +%Y-%m-%d)
    sed -i "s/## \[Unreleased\]/## [$version] - $date/" CHANGELOG.md
    sed -i "s|\[Unreleased\]: \(.*\)/compare/\(.*\)\.\.\.HEAD|[$version]: \1/compare/\2...$version|" CHANGELOG.md
    git add CHANGELOG.md
    git commit -m "Release $version"
    git tag "$version"
    echo "Tagged $version — push with: git push origin $version"

run *args:
    dotnet run --project {{demo}} -- {{args}}

outdated:
    dotnet-outdated {{project}}
