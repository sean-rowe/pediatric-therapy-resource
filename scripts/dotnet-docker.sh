#!/bin/bash
# Simple wrapper to run dotnet commands in container

# Use the existing mcr.microsoft.com/dotnet/sdk:8.0 image directly
docker run --rm -v "$(pwd):/src" -w /src/api mcr.microsoft.com/dotnet/sdk:8.0 dotnet "$@"