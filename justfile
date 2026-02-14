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

run *args:
    dotnet run --project {{demo}} -- {{args}}

outdated:
    dotnet-outdated {{project}}
