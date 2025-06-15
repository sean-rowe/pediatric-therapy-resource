# STORY: IS-001 - Docker Development Environment with .NET SDK

**As a:** Developer or CI/CD system  
**I want:** A containerized development environment with all necessary tools  
**So that:** I can build, test, and lint the application without local SDK installation

## Business Context
- **Problem:** Inconsistent development environments causing "works on my machine" issues
- **Impact:** 15% of PR failures due to environment differences, 2-3 hours per developer per week
- **Cost Savings:** Reduce onboarding time from 2 days to 2 hours

## Acceptance Criteria

### Functional Requirements
1. **Docker container includes:**
   - .NET 8.0 SDK (latest patch version)
   - Node.js 18 LTS (for UI development)
   - Git, curl, wget utilities
   - SQL Server command-line tools
   - Code coverage tools (coverlet, ReportGenerator)
   
2. **Development tools:**
   - StyleCop.Analyzers pre-installed
   - dotnet-format global tool
   - Entity Framework Core tools
   - SonarScanner for code quality

3. **Container behavior:**
   - Mounts source code as volume
   - Preserves NuGet cache between runs
   - Supports hot reload for development
   - Integrates with existing docker-compose.yml

4. **IDE integration:**
   - VS Code devcontainer.json configuration
   - Visual Studio container support
   - IntelliJ IDEA/Rider configuration

### Non-Functional Requirements
- Container build time < 5 minutes
- Container size < 2GB
- Startup time < 30 seconds
- Works on Windows (WSL2), macOS, Linux

## Gherkin Scenarios

```gherkin
Feature: Docker Development Environment

  Background:
    Given Docker Desktop is installed and running
    And docker-compose is available

  Scenario: Build and run development container
    Given I am in the project root directory
    When I run "docker-compose up dev-env"
    Then the container starts within 30 seconds
    And I can access the application at http://localhost:5000
    And all development tools are available

  Scenario: Run tests in container
    Given the development container is running
    When I run "docker exec therapy-dev dotnet test"
    Then all tests execute successfully
    And coverage report is generated
    And results are available on host machine

  Scenario: Run linter in container
    Given the development container is running
    When I run "docker exec therapy-dev dotnet format --verify-no-changes"
    Then StyleCop analysis runs
    And any violations are reported
    And exit code reflects linting status

  Scenario: Hot reload during development
    Given the development container is running
    And I have the API running in watch mode
    When I modify a C# file
    Then the application recompiles automatically
    And changes are reflected without container restart

  Scenario: Database migrations in container
    Given the development container is running
    And SQL Server container is running
    When I run "docker exec therapy-dev dotnet ef database update"
    Then migrations are applied successfully
    And database schema is updated

  Scenario: Consistent environment across platforms
    Given I run the container on <platform>
    When I execute "docker exec therapy-dev dotnet --info"
    Then the output shows .NET 8.0 SDK
    And the version matches across all platforms

    Examples:
      | platform     |
      | Windows WSL2 |
      | macOS Intel  |
      | macOS ARM    |
      | Linux x64    |
```

## Technical Implementation

### Dockerfile.dev
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev-env

# Install Node.js for UI development
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
    && apt-get install -y nodejs

# Install development tools
RUN apt-get update && apt-get install -y \
    git \
    curl \
    wget \
    vim \
    procps \
    unzip \
    && rm -rf /var/lib/apt/lists/*

# Install SQL Server tools
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev \
    && echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc

# Install .NET global tools
RUN dotnet tool install --global dotnet-ef \
    && dotnet tool install --global dotnet-format \
    && dotnet tool install --global coverlet.console \
    && dotnet tool install --global dotnet-reportgenerator-globaltool \
    && dotnet tool install --global dotnet-sonarscanner

ENV PATH="${PATH}:/root/.dotnet/tools"

# Set working directory
WORKDIR /workspace

# Keep container running
CMD ["tail", "-f", "/dev/null"]
```

### docker-compose.override.yml addition
```yaml
services:
  dev-env:
    build:
      context: .
      dockerfile: Dockerfile.dev
    container_name: therapy-dev
    volumes:
      - .:/workspace
      - ${HOME}/.nuget/packages:/root/.nuget/packages
      - dev-nuget-cache:/root/.nuget/packages
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - DOTNET_USE_POLLING_FILE_WATCHER=true
      - DOTNET_RUNNING_IN_CONTAINER=true
    networks:
      - therapy-network
    depends_on:
      - sqlserver
      - redis
    command: tail -f /dev/null

volumes:
  dev-nuget-cache:
```

### devcontainer.json for VS Code
```json
{
  "name": "TherapyDocs Dev",
  "dockerComposeFile": ["../docker-compose.yml", "../docker-compose.override.yml"],
  "service": "dev-env",
  "workspaceFolder": "/workspace",
  "features": {
    "ghcr.io/devcontainers/features/github-cli:1": {},
    "ghcr.io/devcontainers/features/azure-cli:1": {}
  },
  "customizations": {
    "vscode": {
      "extensions": [
        "ms-dotnettools.csharp",
        "ms-azuretools.vscode-docker",
        "ms-vscode.azurecli",
        "github.vscode-pull-request-github",
        "streetsidesoftware.code-spell-checker",
        "shardulm94.trailing-spaces"
      ]
    }
  },
  "forwardPorts": [5000, 5001, 1433],
  "postCreateCommand": "dotnet restore && npm install"
}
```

### Scripts

#### scripts/dev-env.sh
```bash
#!/bin/bash
# Development environment helper script

case "$1" in
  "test")
    docker exec therapy-dev dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
    ;;
  "lint")
    docker exec therapy-dev dotnet format --verify-no-changes
    ;;
  "build")
    docker exec therapy-dev dotnet build
    ;;
  "watch")
    docker exec -it therapy-dev dotnet watch run --project api/TherapyDocs.Api.csproj
    ;;
  "migrate")
    docker exec therapy-dev dotnet ef database update --project api/TherapyDocs.Api.csproj
    ;;
  "bash")
    docker exec -it therapy-dev bash
    ;;
  *)
    echo "Usage: ./scripts/dev-env.sh {test|lint|build|watch|migrate|bash}"
    exit 1
    ;;
esac
```

## Test Requirements

### Unit Tests
- Container build process (using container-structure-test)
- Tool availability verification
- Environment variable validation

### Integration Tests
- Full development workflow (build, test, lint)
- Database connectivity from container
- File watching and hot reload
- Cross-platform compatibility

### Performance Tests
- Container startup time < 30 seconds
- Build performance within container
- Memory usage < 2GB under load

### Security Tests
- No exposed secrets in image
- Non-root user configuration
- Minimal attack surface

## Dependencies
- Docker Desktop or Docker Engine
- docker-compose v2.0+
- 8GB RAM minimum
- 20GB disk space

## Definition of Done
- [ ] Dockerfile.dev created and optimized
- [ ] docker-compose.override.yml updated
- [ ] devcontainer.json configured
- [ ] Helper scripts created and tested
- [ ] Documentation updated
- [ ] All tests passing in container
- [ ] Linting working in container
- [ ] Hot reload confirmed working
- [ ] Tested on Windows, macOS, Linux
- [ ] Container size < 2GB
- [ ] CI/CD pipeline updated to use container
- [ ] Team onboarding guide updated
- [ ] Performance benchmarks met

## Notes
- Consider multi-stage build for smaller production images
- May need platform-specific versions for ARM64
- NuGet cache mounting improves performance significantly
- SQL Server tools add ~300MB but necessary for migrations