#!/bin/bash

# Run BDD tests with coverage
echo "Running BDD Tests for UPTRMS API..."

# Set test environment
export ASPNETCORE_ENVIRONMENT=Test

# Run specific BDD test categories
dotnet test \
  --filter "FullyQualifiedName~BDD" \
  --logger "console;verbosity=detailed" \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/bdd-coverage.xml

# Generate coverage report
if [ -f "./TestResults/bdd-coverage.xml" ]; then
  echo "BDD Test Coverage Report generated at: ./TestResults/bdd-coverage.xml"
fi

echo "BDD Tests completed."