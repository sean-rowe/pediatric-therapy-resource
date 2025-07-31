using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class VarReplacer
{
    // Map of known method return types
    private static readonly Dictionary<string, string> MethodReturnTypes = new Dictionary<string, string>
    {
        // Repository methods
        { "GetByIdAsync", "?" }, // Nullable suffix
        { "GetAllAsync", "List<{0}>" },
        { "GetByReviewStatusAsync", "List<{0}>" },
        { "AddAsync", "{0}" },
        { "UpdateAsync", "{0}" },
        { "GetResourceReviewsAsync", "List<ReviewEvaluation>" },
        
        // LINQ methods
        { "FirstOrDefaultAsync", "?" },
        { "FirstAsync", "{0}" },
        { "SingleOrDefaultAsync", "?" },
        { "ToListAsync", "List<{0}>" },
        { "ToArrayAsync", "{0}[]" },
        { "CountAsync", "int" },
        { "AnyAsync", "bool" },
        { "Select", "IEnumerable<{0}>" },
        { "Where", "IEnumerable<{0}>" },
        { "OrderBy", "IOrderedEnumerable<{0}>" },
        { "OrderByDescending", "IOrderedEnumerable<{0}>" },
        
        // Common methods
        { "Guid.Parse", "Guid" },
        { "Guid.NewGuid", "Guid" },
        { "DateTime.Now", "DateTime" },
        { "DateTime.UtcNow", "DateTime" },
        { "string.Empty", "string" },
        { "string.Join", "string" },
        { "string.Format", "string" },
        { "ToString", "string" },
        { "ToList", "List<{0}>" },
        { "ToArray", "{0}[]" },
        { "ToDictionary", "Dictionary<{0}, {1}>" }
    };

    // Map of known types based on variable names
    private static readonly Dictionary<string, string> VariableNameTypes = new Dictionary<string, string>
    {
        { "userId", "Guid" },
        { "resourceId", "Guid" },
        { "studentId", "Guid" },
        { "user", "User?" },
        { "resource", "Resource?" },
        { "student", "Student?" },
        { "resources", "List<Resource>" },
        { "students", "List<Student>" },
        { "users", "List<User>" },
        { "count", "int" },
        { "total", "int" },
        { "index", "int" },
        { "isValid", "bool" },
        { "hasAccess", "bool" },
        { "exists", "bool" },
        { "name", "string" },
        { "email", "string" },
        { "message", "string" },
        { "result", "dynamic" },
        { "response", "dynamic" },
        { "dto", "dynamic" },
        { "dtos", "List<dynamic>" }
    };

    public static void ReplaceVarInFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        string content = File.ReadAllText(filePath);
        string[] lines = File.ReadAllLines(filePath);
        
        var replacements = new List<(int line, string original, string replacement)>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            
            // Skip comments
            if (line.Trim().StartsWith("//") || line.Trim().StartsWith("/*") || line.Trim().StartsWith("*"))
                continue;

            // Find var declarations
            var varMatch = Regex.Match(line, @"(\s*)var\s+(\w+)\s*=\s*(.+);?$");
            if (varMatch.Success)
            {
                string indent = varMatch.Groups[1].Value;
                string variableName = varMatch.Groups[2].Value;
                string assignment = varMatch.Groups[3].Value.TrimEnd(';');
                
                string type = InferType(variableName, assignment, lines, i);
                
                if (!string.IsNullOrEmpty(type) && type != "var")
                {
                    string newLine = $"{indent}{type} {variableName} = {assignment};";
                    replacements.Add((i, line, newLine));
                }
            }
        }

        // Apply replacements
        foreach (var (lineIndex, original, replacement) in replacements)
        {
            lines[lineIndex] = replacement;
            Console.WriteLine($"Line {lineIndex + 1}: Replaced var with explicit type");
        }

        File.WriteAllLines(filePath, lines);
        Console.WriteLine($"Updated {filePath}: {replacements.Count} var declarations replaced");
    }

    private static string InferType(string variableName, string assignment, string[] lines, int currentLine)
    {
        assignment = assignment.Trim();

        // 1. Check for new object creation
        var newMatch = Regex.Match(assignment, @"new\s+(\w+)(<[^>]+>)?\s*[\(\{]");
        if (newMatch.Success)
        {
            return newMatch.Groups[1].Value + (newMatch.Groups[2].Value ?? "");
        }

        // 2. Check for numeric literals
        if (Regex.IsMatch(assignment, @"^\d+$"))
            return "int";
        if (Regex.IsMatch(assignment, @"^\d+\.\d+$"))
            return "double";
        if (Regex.IsMatch(assignment, @"^\d+(\.\d+)?m$"))
            return "decimal";
        if (Regex.IsMatch(assignment, @"^\d+(\.\d+)?f$"))
            return "float";
        if (Regex.IsMatch(assignment, @"^\d+L$"))
            return "long";

        // 3. Check for boolean literals
        if (assignment == "true" || assignment == "false")
            return "bool";

        // 4. Check for string literals
        if (assignment.StartsWith("\"") || assignment.StartsWith("@\"") || assignment.StartsWith("$\""))
            return "string";

        // 5. Check for null
        if (assignment == "null")
            return "object?";

        // 6. Check for Guid operations
        if (assignment.Contains("Guid.Parse") || assignment.Contains("Guid.NewGuid"))
            return "Guid";

        // 7. Check for DateTime operations
        if (assignment.Contains("DateTime.") || assignment.Contains("DateTimeOffset."))
            return assignment.Contains("DateTimeOffset") ? "DateTimeOffset" : "DateTime";

        // 8. Check for async/await patterns
        if (assignment.StartsWith("await"))
        {
            // Try to find the repository type
            var repoMatch = Regex.Match(assignment, @"_(\w+)Repository\.");
            if (repoMatch.Success)
            {
                string entityType = repoMatch.Groups[1].Value;
                
                // Check for specific method patterns
                if (assignment.Contains("GetByIdAsync"))
                    return $"{entityType}?";
                if (assignment.Contains("GetAllAsync") || assignment.Contains("ToListAsync"))
                    return $"List<{entityType}>";
                if (assignment.Contains("FirstOrDefaultAsync") || assignment.Contains("SingleOrDefaultAsync"))
                    return $"{entityType}?";
            }

            // Check for _context patterns
            var contextMatch = Regex.Match(assignment, @"_context\.(\w+)\.");
            if (contextMatch.Success)
            {
                string entityType = contextMatch.Groups[1].Value;
                // Remove 's' from plural to get singular type
                if (entityType.EndsWith("s"))
                    entityType = entityType.Substring(0, entityType.Length - 1);
                
                if (assignment.Contains("FirstOrDefaultAsync") || assignment.Contains("SingleOrDefaultAsync"))
                    return $"{entityType}?";
                if (assignment.Contains("ToListAsync"))
                    return $"List<{entityType}>";
            }
        }

        // 9. Check for LINQ operations
        if (assignment.Contains(".Select(") && assignment.Contains(".ToList()"))
        {
            // Try to infer from the variable name
            if (variableName.EndsWith("Dtos"))
                return "List<dynamic>";
            if (variableName.EndsWith("Ids"))
                return "List<Guid>";
            return "List<dynamic>";
        }

        // 10. Check variable name patterns
        foreach (var kvp in VariableNameTypes)
        {
            if (variableName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        // 11. Check for collection patterns in variable names
        if (variableName.EndsWith("List") || variableName.EndsWith("s"))
            return "List<dynamic>";
        if (variableName.EndsWith("Dict") || variableName.EndsWith("Dictionary"))
            return "Dictionary<string, object>";
        if (variableName.EndsWith("Set"))
            return "HashSet<dynamic>";

        // 12. Default to dynamic for complex expressions
        return "dynamic";
    }

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: VarReplacer <file-path>");
            return;
        }

        string filePath = args[0];
        
        if (File.Exists(filePath))
        {
            ReplaceVarInFile(filePath);
        }
        else if (Directory.Exists(filePath))
        {
            // Process all .cs files in directory
            var csFiles = Directory.GetFiles(filePath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("bin") && !f.Contains("obj"));
            
            foreach (var file in csFiles)
            {
                Console.WriteLine($"Processing {file}...");
                ReplaceVarInFile(file);
            }
        }
        else
        {
            Console.WriteLine($"Path not found: {filePath}");
        }
    }
}