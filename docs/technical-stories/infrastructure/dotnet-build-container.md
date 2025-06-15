# STORY: IS-002 - Minimal .NET Build Container for Immediate Use

**As a:** Developer working on PR #182  
**I want:** A minimal .NET container to run builds, linters, and tests  
**So that:** I can verify code quality before pushing changes

## Business Context
- **Immediate Need:** PR #182 is blocked because we cannot run linters/tests locally
- **Problem:** Using CI/CD as a compiler wastes resources and creates bad commits
- **Impact:** Each failed CI/CD run costs 5-10 minutes and creates commit noise

## Acceptance Criteria

### Functional Requirements
1. **Minimal container with:**
   - .NET 8.0 SDK (latest)
   - Essential build tools only
   - No IDE integrations needed (for now)
   
2. **Must support:**
   - `dotnet restore` - Package restoration
   - `dotnet build` - Compilation with linting
   - `dotnet test` - Unit test execution
   - StyleCop analyzer execution

3. **Simple usage:**
   - Single command to build container
   - Single command to run any dotnet command
   - Works with existing api/ directory structure

### Non-Functional Requirements
- Container builds in < 2 minutes
- Minimal size (< 1GB)
- Works immediately without additional setup

## Gherkin Scenarios

```gherkin
Feature: Minimal .NET Build Container

  Scenario: Build the container
    Given I have Docker installed
    When I run "docker build -t therapy-dotnet -f Dockerfile.build ."
    Then the container is built successfully
    And it takes less than 2 minutes

  Scenario: Run linter
    Given the therapy-dotnet container exists
    When I run "docker run -v $(pwd):/src therapy-dotnet dotnet build /src/api --verbosity normal"
    Then I see build output with StyleCop warnings/errors
    And the exit code reflects success/failure

  Scenario: Run tests
    Given the therapy-dotnet container exists
    When I run "docker run -v $(pwd):/src therapy-dotnet dotnet test /src/api"
    Then all tests execute
    And I see test results
    And coverage information is displayed

  Scenario: Package restore with security check
    Given the therapy-dotnet container exists
    When I run "docker run -v $(pwd):/src therapy-dotnet dotnet restore /src/api"
    Then packages are restored
    And security vulnerabilities are reported
    And the build fails if vulnerabilities exist
```

## Technical Implementation

### Dockerfile.build (Minimal)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install only essential tools
RUN apt-get update && apt-get install -y \
    git \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /src

# Default to bash for interactive use
CMD ["/bin/bash"]
```

### Helper script: dotnet-docker.sh
```bash
#!/bin/bash
# Simple wrapper to run dotnet commands in container

# Build container if it doesn't exist
if [[ "$(docker images -q therapy-dotnet 2> /dev/null)" == "" ]]; then
  echo "Building .NET container..."
  docker build -t therapy-dotnet -f Dockerfile.build .
fi

# Run the command
docker run --rm -v "$(pwd):/src" -w /src therapy-dotnet dotnet "$@"
```

### Usage Examples
```bash
# Make script executable
chmod +x dotnet-docker.sh

# Run linter
./dotnet-docker.sh build api/ --verbosity normal

# Run tests
./dotnet-docker.sh test api/

# Run specific test project
./dotnet-docker.sh test api/Tests/

# Restore packages
./dotnet-docker.sh restore api/

# Clean
./dotnet-docker.sh clean api/
```

## Test Requirements

### Verification Steps
1. Container builds successfully
2. Can restore packages from PR #182
3. Can run build with linting
4. Can execute all tests
5. Exit codes properly propagated

## Dependencies
- Docker installed
- Current working directory is repo root
- No other dependencies

## Definition of Done
- [ ] Dockerfile.build created
- [ ] dotnet-docker.sh script created and executable
- [ ] Container builds in < 2 minutes
- [ ] Successfully runs linter on PR #182 code
- [ ] Successfully runs tests on PR #182 code
- [ ] Documentation in README
- [ ] No additional configuration required

## Notes
- This is a tactical solution for immediate needs
- Full development environment (IS-001) still needed for comprehensive tooling
- Focus on minimal complexity for immediate use
- Can be enhanced later with caching, better volumes, etc.