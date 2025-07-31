# TODO: Replace var Usage with Explicit Types

## Summary
Total files with var usage: 118
Total estimated var declarations: 500+

## Controllers (5 files)

### ContentManagementController.cs
```csharp
// Line 89: var userId = Guid.Parse(...)
Guid userId = Guid.Parse(...)

// Line 91: var resources = await _context.Resources...
List<Resource> resources = await _context.Resources...

// Line 105: var dto = new ResourceManagementDto
ResourceManagementDto dto = new ResourceManagementDto
```

### GoalsController.cs
- Replace all var declarations with explicit Goal, StudentGoal, List<Goal> types

### SessionsController.cs
- Replace all var declarations with explicit Session, List<Session> types

### CaseloadController.cs
- Replace all var declarations with explicit Student, List<Student>, CaseloadDto types

### UsersController.cs
- Replace all var declarations with explicit User, UserDto, List<User> types

## Services (4 files)

### EmailService.cs
- Replace var with MimeMessage, BodyBuilder, SmtpClient types

### AuthenticationService.cs
- Replace var with User, ClaimsPrincipal, AuthenticationResult types

### AuditService.cs
- Replace var with AuditLog, List<AuditLog> types

### TokenService.cs
- Replace var with JwtSecurityToken, ClaimsPrincipal, SecurityKey types

## Repositories (2 files)

### UserRepository.cs
- Replace var with IQueryable<User>, User, List<User> types

### ResourceRepository.cs
- Replace var with IQueryable<Resource>, Resource, List<Resource> types

## Test Files Pattern

### Common var Patterns in Tests:
```csharp
// BEFORE:
var response = await client.GetAsync("/api/users");
var content = await response.Content.ReadAsStringAsync();
var users = JsonSerializer.Deserialize<List<UserDto>>(content);

// AFTER:
HttpResponseMessage response = await client.GetAsync("/api/users");
string content = await response.Content.ReadAsStringAsync();
List<UserDto> users = JsonSerializer.Deserialize<List<UserDto>>(content);
```

### StepDefinitions Pattern:
```csharp
// BEFORE:
var table = new Table("Field", "Value");
var data = table.Rows.ToDictionary(row => row[0], row => row[1]);

// AFTER:
Table table = new Table("Field", "Value");
Dictionary<string, string> data = table.Rows.ToDictionary(row => row[0], row => row[1]);
```

## Automated Replacement Script

Create a PowerShell or bash script to help with replacements:

```powershell
# PowerShell script to find var usage
Get-ChildItem -Path . -Filter *.cs -Recurse | 
    Select-String -Pattern '\bvar\b' | 
    Group-Object Path | 
    ForEach-Object { 
        Write-Host "`n$($_.Name) - $($_.Count) occurrences" 
        $_.Group | ForEach-Object { Write-Host "  Line $($_.LineNumber): $($_.Line.Trim())" }
    }
```

## Regex Patterns for Common Replacements

1. **LINQ Queries**:
   - Pattern: `var (\w+) = await _context\.(\w+)\.`
   - Replace: `List<$2> $1 = await _context.$2.`

2. **Single Entity**:
   - Pattern: `var (\w+) = await _context\.(\w+)\.FirstOrDefaultAsync`
   - Replace: `$2? $1 = await _context.$2.FirstOrDefaultAsync`

3. **New Object Creation**:
   - Pattern: `var (\w+) = new (\w+)`
   - Replace: `$2 $1 = new $2`

4. **Method Returns**:
   - Pattern: `var result = await (\w+)\.(\w+)Async`
   - Check return type and replace accordingly

## Testing After Replacement

1. Run build to ensure no compilation errors:
   ```bash
   dotnet build
   ```

2. Run all tests to ensure functionality unchanged:
   ```bash
   dotnet test
   ```

3. Run StyleCop to ensure no new violations:
   ```bash
   dotnet format --verify-no-changes
   ```