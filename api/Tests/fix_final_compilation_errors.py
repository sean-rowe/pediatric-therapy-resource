#\!/usr/bin/env python3
import os

def fix_final_compilation_errors():
    """Fix the final compilation errors in specific files"""
    
    fixes = {
        'ParentPortalSteps.cs': [
            # Fix invalid token '}' at line 80
            ('        });\n    }\n    }', '        });\n    }'),
            # Fix top-level statements issue
            ('    }\n    [Given(@', '    }\n    \n    [Given(@'),
        ],
        'SeasonalHolidaySteps.cs': [
            # Fix missing ) at line 71
            ('await WhenISendAPOSTRequestToWithData("/api/seasonal/apply-theme", new Dictionary<string, object>\n        {\n            ["theme"] = theme\n        });', 
             'await WhenISendAPOSTRequestToWithData("/api/seasonal/apply-theme", new Dictionary<string, object>\n        {\n            ["theme"] = theme\n        });'),
        ],
        'DataCollectionSteps.cs': [
            # Fix missing ) at line 56
            ('await WhenISendAPOSTRequestToWithData("/api/data-collection/start", new Dictionary<string, object>\n        {\n            ["sessionId"] = _currentSessionId\n        });',
             'await WhenISendAPOSTRequestToWithData("/api/data-collection/start", new Dictionary<string, object>\n        {\n            ["sessionId"] = _currentSessionId\n        });'),
        ],
        'ResourceSearchSteps.cs': [
            # Fix semicolon issues
            ('_searchResults = results as List<object> ?? new List<object>();\n        ScenarioContext["SearchResults"] = _searchResults',
             '_searchResults = results as List<object> ?? new List<object>();\n        ScenarioContext["SearchResults"] = _searchResults;'),
        ],
        'OutcomeMeasurementSteps.cs': [
            # Fix missing ) at line 72
            ('await WhenISendAPOSTRequestToWithData("/api/outcomes/foto/assessment", new Dictionary<string, object>\n        {\n            ["patientId"] = ScenarioContext["PatientId"]\n        });',
             'await WhenISendAPOSTRequestToWithData("/api/outcomes/foto/assessment", new Dictionary<string, object>\n        {\n            ["patientId"] = ScenarioContext["PatientId"]\n        });'),
        ],
        'ResourceCreationSteps.cs': [
            # Fix missing ) 
            ('await WhenISendAPOSTRequestToWithData("/api/resources/create", new Dictionary<string, object>\n        {\n            ["resourceType"] = "worksheet"\n        });',
             'await WhenISendAPOSTRequestToWithData("/api/resources/create", new Dictionary<string, object>\n        {\n            ["resourceType"] = "worksheet"\n        });'),
        ],
    }
    
    for filename, replacements in fixes.items():
        filepath = os.path.join('BDD/StepDefinitions', filename)
        if os.path.exists(filepath):
            with open(filepath, 'r') as f:
                content = f.read()
            
            original = content
            for old, new in replacements:
                if old in content:
                    content = content.replace(old, new)
                    print(f"Fixed pattern in {filename}")
            
            # Additional generic fixes
            # Fix dictionary elements outside braces
            import re
            content = re.sub(
                r'}\s*\)\s*\n(\s*)\[("[^"]+" < /dev/null | \'[^\']+\')\]\s*=\s*([^,\n]+),?',
                r'\n\1            [\2] = \3\n\1        });',
                content
            )
            
            if content \!= original:
                with open(filepath, 'w') as f:
                    f.write(content)
                print(f"Updated {filename}")
        else:
            print(f"File not found: {filename}")

if __name__ == "__main__":
    fix_final_compilation_errors()
