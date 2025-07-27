#!/usr/bin/env python3
"""
Analyze BDD/Gherkin coverage against CLAUDE.md functional requirements
"""

import os
import re
from pathlib import Path

# 42 Functional Requirements from CLAUDE.md
FUNCTIONAL_REQUIREMENTS = {
    "FR-001": "User Management - Multi-tier subscription management",
    "FR-002": "Resource Library - Searchable, filterable library of 50,000+ therapy resources",
    "FR-003": "Therapy Planning - Automated session planning with IEP goal integration",
    "FR-004": "Data Collection - Digital data collection for therapy sessions with progress tracking",
    "FR-005": "Content Management - Admin portal for content upload, categorization, and quality review",
    "FR-006": "AI Content Generation - AI-powered generation of personalized therapy materials",
    "FR-007": "AI Quality Assurance - Automated and manual review system for AI-generated content",
    "FR-008": "Marketplace - Therapist marketplace for buying/selling original resources",
    "FR-009": "Interactive Digital Activities - Self-grading digital task cards with real-time feedback",
    "FR-010": "EHR Integration - Bi-directional integration with major therapy EHR systems",
    "FR-011": "Seller Tools - Comprehensive seller dashboard and storefront system",
    "FR-012": "Student Management - Comprehensive student roster and assignment system",
    "FR-013": "Physical/Digital Hybrid - Integration of physical therapy materials with digital platform",
    "FR-014": "Communication Tools - Multi-channel communication and sharing system",
    "FR-015": "Assessment & Screening - Built-in assessment tools and protocols",
    "FR-016": "Adult Therapy Resources - Resources for adult/geriatric populations",
    "FR-017": "Movement & Sensory - Gross motor and sensory integration resources",
    "FR-018": "Professional Development - Therapist training and self-care resources",
    "FR-019": "Multilingual Support - Comprehensive multilingual resource system",
    "FR-020": "Seasonal & Holiday - Themed seasonal content management",
    "FR-021": "Free Resources - Free educational handouts and sample system",
    "FR-022": "External Integrations - Third-party marketplace and platform integrations",
    "FR-023": "Specialized Content - Highly specialized therapy content modules",
    "FR-024": "Virtual Tools - Teletherapy-specific tools and backgrounds",
    "FR-025": "Caseload Integration - Unified caseload and resource management",
    "FR-026": "Creation Tools - Template-based resource creation tools",
    "FR-027": "Gamification - Student motivation and reward systems",
    "FR-028": "Documentation Helpers - Integrated documentation support tools",
    "FR-029": "Research & Evidence - Research library and evidence base tracking",
    "FR-030": "Community Features - Limited community interaction features",
    "FR-031": "Curriculum Planning - Long-term therapy planning and curriculum mapping",
    "FR-032": "Outcome Measurement - Standardized outcome measurement integration",
    "FR-033": "PECS Implementation - Complete Picture Exchange Communication System",
    "FR-034": "ABA Integration - Applied Behavior Analysis tools and tracking",
    "FR-035": "AAC Comprehensive - Full augmentative/alternative communication suite",
    "FR-036": "Clinical Education - Student clinician and supervision tools",
    "FR-037": "Transition Planning - Life skills and transition assessment tools",
    "FR-038": "Specialized Protocols - Evidence-based therapy protocol libraries",
    "FR-039": "Advocacy & Legal - Advocacy resources and legal templates",
    "FR-040": "Sensory Rooms - Sensory room design and equipment guides",
    "FR-041": "Feeding Therapy - Comprehensive feeding and oral motor resources",
    "FR-042": "Multi-Sensory Learning - Resources for different learning styles"
}

def find_fr_references_in_feature(file_path):
    """Find FR-XXX references in a feature file"""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
        
    # Find FR-XXX patterns
    fr_pattern = r'FR-(\d{3})'
    matches = re.findall(fr_pattern, content)
    return [f"FR-{match}" for match in matches]

def analyze_feature_coverage():
    """Analyze which functional requirements have Gherkin coverage"""
    features_dir = Path("/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/Features")
    
    coverage_map = {}
    feature_files = []
    
    # Find all .feature files
    for feature_file in features_dir.rglob("*.feature"):
        feature_files.append(feature_file)
        
        # Get FR references from this file
        fr_refs = find_fr_references_in_feature(feature_file)
        
        for fr_ref in fr_refs:
            if fr_ref not in coverage_map:
                coverage_map[fr_ref] = []
            coverage_map[fr_ref].append(str(feature_file.relative_to(features_dir)))
    
    return coverage_map, feature_files

def generate_coverage_report():
    """Generate a comprehensive coverage report"""
    coverage_map, feature_files = analyze_feature_coverage()
    
    print("="*80)
    print("GHERKIN COVERAGE ANALYSIS REPORT")
    print("="*80)
    print(f"Total feature files found: {len(feature_files)}")
    print(f"Total functional requirements: {len(FUNCTIONAL_REQUIREMENTS)}")
    print(f"Requirements with Gherkin coverage: {len(coverage_map)}")
    print(f"Coverage percentage: {len(coverage_map)/len(FUNCTIONAL_REQUIREMENTS)*100:.1f}%")
    print()
    
    print("COVERED FUNCTIONAL REQUIREMENTS:")
    print("-" * 50)
    covered_count = 0
    for fr_id in sorted(FUNCTIONAL_REQUIREMENTS.keys()):
        if fr_id in coverage_map:
            covered_count += 1
            print(f"✓ {fr_id}: {FUNCTIONAL_REQUIREMENTS[fr_id]}")
            for feature_file in coverage_map[fr_id]:
                print(f"    └── {feature_file}")
    print(f"\nCovered: {covered_count}/{len(FUNCTIONAL_REQUIREMENTS)}")
    print()
    
    print("MISSING FUNCTIONAL REQUIREMENTS:")
    print("-" * 50)
    missing_count = 0
    for fr_id in sorted(FUNCTIONAL_REQUIREMENTS.keys()):
        if fr_id not in coverage_map:
            missing_count += 1
            print(f"✗ {fr_id}: {FUNCTIONAL_REQUIREMENTS[fr_id]}")
    print(f"\nMissing: {missing_count}/{len(FUNCTIONAL_REQUIREMENTS)}")
    print()
    
    # List feature files that don't reference any FR
    print("FEATURE FILES WITHOUT FR REFERENCES:")
    print("-" * 50)
    orphaned_files = []
    for feature_file in feature_files:
        fr_refs = find_fr_references_in_feature(feature_file)
        if not fr_refs:
            relative_path = feature_file.relative_to(Path("/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/Features"))
            orphaned_files.append(str(relative_path))
    
    for orphaned in sorted(orphaned_files):
        print(f"  {orphaned}")
    print(f"\nOrphaned files: {len(orphaned_files)}")
    print()
    
    return coverage_map

if __name__ == "__main__":
    generate_coverage_report()