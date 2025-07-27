#!/bin/bash

# Test execution script for TherapyDocs API
echo "🔧 Running TherapyDocs API Tests..."

# Navigate to API directory
cd "$(dirname "$0")/../api" || exit 1

echo "📦 Restoring packages..."
dotnet restore

echo "🏗️ Building project..."
dotnet build --no-restore

echo "🧪 Running unit tests..."
dotnet test --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"

echo "📊 Generating code coverage report..."
dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory TestResults

echo "📈 Generating coverage summary..."
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/coverage.opencover.xml

echo "✅ Tests completed. Check TestResults/ directory for detailed reports."
echo ""
echo "📋 Test Results Summary:"
echo "- Unit Tests: Check test-results.trx"
echo "- Coverage: Check TestResults/coverage.opencover.xml" 
echo "- Detailed reports available in TestResults/ directory"