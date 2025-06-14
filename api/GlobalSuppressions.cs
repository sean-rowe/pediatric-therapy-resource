// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Suppress documentation requirements for Program.cs and startup code
[assembly: SuppressMessage("Documentation", "SA1600:Elements should be documented", Justification = "Program.cs doesn't require documentation", Scope = "type", Target = "~T:Program")]
[assembly: SuppressMessage("Documentation", "SA1633:File should have header", Justification = "File headers not required for this project")]

// Suppress for DTOs and Models where init-only properties are used
[assembly: SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Properties use init accessors in DTOs")]

// Suppress for using statements location (we prefer them outside namespace)
[assembly: SuppressMessage("Style", "SA1200:Using directives should be placed correctly", Justification = "Project convention is to place usings outside namespace")]

// Suppress for test projects
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Not required in test methods", Scope = "namespaceanddescendants", Target = "~N:TherapyDocs.Api.Tests")]