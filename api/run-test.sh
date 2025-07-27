#!/bin/bash

# Run BDD test in Docker with simplified environment
docker run --rm -v $(pwd):/src -w /src \
  -e NUGET_PACKAGES=/src/.nuget/packages \
  -e HOME=/tmp \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -c "
    # Clean and build
    dotnet clean Tests/TherapyDocs.Api.Tests.csproj -v q
    dotnet build Tests/TherapyDocs.Api.Tests.csproj -v q
    
    # Run the test
    dotnet test Tests/TherapyDocs.Api.Tests.csproj \
      --filter 'FullyQualifiedName~Successfully_register_a_new_user' \
      --no-build \
      -v normal
  "